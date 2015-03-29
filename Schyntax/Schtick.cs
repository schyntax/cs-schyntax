using System;
using System.Collections.Generic;
using System.Threading;

namespace Schyntax
{
    public delegate void ScheduledTaskCallback(ScheduledTask task, DateTime timeIntendedToRun);

    public class Schtick
    {
        private readonly object _lockTasks = new object();
        private readonly List<ScheduledTask> _tasks = new List<ScheduledTask>();

        public IReadOnlyList<ScheduledTask> Tasks => _tasks.AsReadOnly();

        public event Action<ScheduledTask, Exception> OnTaskException;

        /// <summary>
        /// Adds a scheduled task to this instance of Schtick.
        /// </summary>
        /// <param name="schedule">A string representing a valid schyntax schedule.</param>
        /// <param name="callback">Function which will be called each time the task is supposed to run.</param>
        /// <param name="autoRun">If true, Start() will be called on the task automatically.</param>
        /// <param name="name">Optional name for the task. This is not used by Schtick, but may be useful for application code.</param>
        /// <param name="lastKnownRun">The last Date when the task is known to have run. Used for Task Windows.</param>
        /// <param name="window">
        /// The period of time (in milliseconds) after an event should have run where it would still be appropriate to run it.
        /// See Task Windows documentation for more details.
        /// </param>
        /// <param name="skipIfSlowCallback">
        /// If true, and a callback does not complete before the next time it is supposed to be called, then it won't be called until the next
        /// interval after it has completed. If false, then the callback will always be called once per interval, even though the number of calls
        /// may start to get backed up.
        /// </param>
        /// <returns></returns>
        public ScheduledTask AddTask(
            string schedule, 
            ScheduledTaskCallback callback, 
            bool autoRun = true, 
            string name = null, 
            DateTime lastKnownRun = default(DateTime), 
            int window = 0,
            bool skipIfSlowCallback = true)
        {
            var task = new ScheduledTask(this, new Schedule(schedule), callback)
            {
                Name = name,
                Window = window,
                SkipIfSlowCallback = skipIfSlowCallback
            };

            lock (_lockTasks)
            {
                _tasks.Add(task);
            }

            if (autoRun)
                task.Start(lastKnownRun);
            
            return task;
        }

        /// <summary>
        /// If the task in contained in this instance of Schtick, then the task is removed from the list and stopped (if running).
        /// Attempting to re-start the task after removing it will throw an exception.
        /// </summary>
        /// <param name="task"></param>
        /// <returns>True if the task was removed and stopped, otherwise false.</returns>
        public bool RemoveTask(ScheduledTask task)
        {
            lock (_lockTasks)
            {
                var found = false;
                for (var i = 0; i < _tasks.Count; i++)
                {
                    if (_tasks[i] == task)
                    {
                        _tasks.RemoveAt(i);
                        found = true;
                    }
                }

                if (!found)
                    return false;
            }

            task.IsAttached = false;
            task.Stop();
            return true;
        }

        internal void InvokeExceptionEvent(ScheduledTask task, Exception ex)
        {
            OnTaskException?.Invoke(task, ex);
        }
    }

    public class ScheduledTask
    {
        private struct ScheduleThreadData
        {
            public int ThreadVersion;
            public DateTime FirstEvent;
        }

        private readonly object _lockStartStop = new object();
        private int _threadVersion = 0;

        public Schtick Schtick { get; }
        public bool IsAttached { get; internal set; }
        public string Name { get; set; }
        public bool IsRunning { get; private set; }
        public Schedule Schedule { get; }
        public ScheduledTaskCallback Callback { get; }
        public DateTime NextTime { get; private set; }
        public int Window { get; set; }
        public bool SkipIfSlowCallback { get; set; }

        internal ScheduledTask(Schtick schtick, Schedule schedule, ScheduledTaskCallback callback)
        {
            Schtick = schtick;
            Schedule = schedule;
            Callback = callback;
            IsAttached = true;
        }

        public void Start(DateTime lastKnownRun = default(DateTime))
        {
            if (!IsAttached)
                throw new InvalidOperationException("Cannot start task which is not attached to a Schtick instance.");

            lock (_lockStartStop)
            {
                if (IsRunning)
                    return;

                _threadVersion++;
                IsRunning = true;

                var set = false;
                var data = new ScheduleThreadData { ThreadVersion = _threadVersion };
                if (Window > 0 && lastKnownRun != default(DateTime))
                {
                    // check if we actually want to run the first event right away
                    var prev = Schedule.Previous();
                    lastKnownRun = lastKnownRun.AddSeconds(1); // add a second for good measure
                    if (prev > lastKnownRun && prev > DateTime.UtcNow.AddMilliseconds(-Window))
                    {
                        data.FirstEvent = prev;
                        set = true;
                    }
                }

                if (!set)
                    data.FirstEvent = Schedule.Next();

                NextTime = data.FirstEvent;

                // start schedule thread
                var thread = new Thread(RunSchedule);
                thread.IsBackground = true;
                thread.Start(data);
            }
        }

        public void Stop()
        {
            lock (_lockStartStop)
            {
                if (!IsRunning)
                    return;

                IsRunning = false;
            }
        }

        private void RunSchedule(object o)
        {
            const int MAXIMUM_WAIT = 30000;

            var data = (ScheduleThreadData)o;

            var nextTime = data.FirstEvent;
            var runOnNext = false;
            var thread = Thread.CurrentThread;

            // ReSharper disable once LoopVariableIsNeverChangedInsideLoop
            while (IsRunning && data.ThreadVersion == _threadVersion)
            {
                if (runOnNext || DateTime.UtcNow >= nextTime)
                {
                    runOnNext = false;

                    try
                    {
                        // promote to a foreground thread so that it's less likely to get killed while the task is running
                        thread.IsBackground = false;
                        Callback(this, nextTime);
                    }
                    catch (Exception ex)
                    {
                        // invoke the exception handler on a separate thread
                        var exThread = new Thread(InvokeExceptionEvent);
                        exThread.Start(ex);
                    }

                    // go back to being a background thread so that we don't keep the application alive for no reason.
                    thread.IsBackground = true;

                    try
                    {
                        nextTime = Schedule.Next(nextTime);
                        if (SkipIfSlowCallback && nextTime < DateTime.UtcNow)
                        {
                            nextTime = Schedule.Next(DateTime.UtcNow);
                        }
                    }
                    catch (Exception ex)
                    {
                        IsRunning = false;
                        InvokeExceptionEvent(new ScheduleCrashException("Schtick Schedule has been terminated because the next valid time could not be found.", this, ex));
                        return;
                    }
                }

                NextTime = nextTime;

                // get how long we would need to wait for the next 
                var ms = (int)(nextTime - DateTime.UtcNow).TotalMilliseconds;
                if (ms < MAXIMUM_WAIT)
                {
                    runOnNext = true;
                    Thread.Sleep(Math.Max(ms, 0));
                }
                else
                {
                    Thread.Sleep(MAXIMUM_WAIT);
                }
            }
        }

        private void InvokeExceptionEvent(object o)
        {
            Schtick.InvokeExceptionEvent(this, (Exception)o);
        }
    }
}
