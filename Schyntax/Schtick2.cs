using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Schyntax
{
    public delegate void ScheduledTaskCallback2(ScheduledTask2 task, DateTime timeIntendedToRun);

    public class Schtick2
    {
        private readonly object _lockTasks = new object();
        private readonly Dictionary<string, ScheduledTask2> _tasks = new Dictionary<string, ScheduledTask2>();

        public event Action<ScheduledTask2, Exception> OnTaskException;

        public ScheduledTask2 AddTask(
            string name,
            string schedule,
            ScheduledTaskCallback2 callback,
            bool autoRun = true,
            DateTime lastKnownRun = default(DateTime),
            TimeSpan window = default(TimeSpan),
            bool skipIfSlowCallback = true)
        {
            if (schedule == null)
                throw new ArgumentNullException(nameof(schedule));

            return AddTask(name, new Schedule(schedule), callback, autoRun, lastKnownRun, window, skipIfSlowCallback);
        }

        public ScheduledTask2 AddTask(
            string name,
            Schedule schedule,
            ScheduledTaskCallback2 callback,
            bool autoRun = true,
            DateTime lastKnownRun = default(DateTime),
            TimeSpan window = default(TimeSpan),
            bool skipIfSlowCallback = true)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));

            ScheduledTask2 task;
            lock (_lockTasks)
            {
                if (_tasks.ContainsKey(name))
                    throw new Exception($"A scheduled task named \"{name}\" already exists.");

                task = new ScheduledTask2(name, schedule, callback)
                {
                    Window = window,
                    SkipIfSlowCallback = skipIfSlowCallback,
                };

                _tasks.Add(name, task);
            }

            task.OnException += TaskOnOnException;

            if (autoRun)
                task.Start(lastKnownRun);

            return task;
        }

        private void TaskOnOnException(ScheduledTask2 task, Exception ex)
        {
            var ev = OnTaskException;
            ev?.Invoke(task, ex);
        }

        public bool TryGetTask(string name, out ScheduledTask2 task)
        {
            return _tasks.TryGetValue(name, out task);
        }

        public bool RemoveTask(string name)
        {
            lock (_lockTasks)
            {
                ScheduledTask2 task;
                if (!_tasks.TryGetValue(name, out task))
                    return false;

                if (task.IsRunning)
                    throw new Exception($"Cannot remove task \"{name}\". It is still running.");

                task.IsAttached = false;
                _tasks.Remove(name);
                return true;
            }
        }

        // Shutdown
    }

    public class ScheduledTask2
    {
        private int _runId = 0;
        private int _runLocked = 0;

        private readonly HashSet<int> _runsExecuting = new HashSet<int>();
        private Dictionary<int, List<Action<ScheduledTask2>>> _stopCallbacks;

        public string Name { get; }
        public Schedule Schedule { get; private set; }
        public ScheduledTaskCallback2 Callback { get; }
        public bool IsRunning { get; internal set; }
        public bool IsAttached { get; internal set; }
        public TimeSpan Window { get; set; }
        public DateTime NextEvent { get; private set; }
        public DateTime PrevEvent { get; private set; }
        public bool SkipIfSlowCallback { get; set; }

        public event Action<ScheduledTask2, Exception> OnException;

        internal ScheduledTask2(string name, Schedule schedule, ScheduledTaskCallback2 callback)
        {
            Name = name;
            Schedule = schedule;
            Callback = callback;
        }

        public void Start(DateTime lastKnownRun)
        {
            TakeRunLock();
            try
            {
                if (!IsAttached)
                    throw new InvalidOperationException("Cannot start task which is not attached to a Schtick instance.");

                if (IsRunning)
                    return;

                var firstEvent = default(DateTime);
                var firstEventSet = false;
                var window = Window;
                if (window > TimeSpan.Zero && lastKnownRun != default(DateTime))
                {
                    // check if we actually want to run the first event right away
                    var prev = Schedule.Previous();
                    lastKnownRun = lastKnownRun.AddSeconds(1); // add a second for good measure
                    if (prev > lastKnownRun && prev > (DateTime.UtcNow - window))
                    {
                        firstEvent = prev;
                        firstEventSet = true;
                    }
                }

                if (!firstEventSet)
                    firstEvent = Schedule.Next();

                while (firstEvent <= PrevEvent)
                {
                    // we don't want to run the same event twice
                    firstEvent = Schedule.Next(firstEvent);
                }

                NextEvent = firstEvent;
                Run(_runId).ContinueWith(task => { });

                IsRunning = true;
            }
            finally
            {
                ReleaseRunLock();
            }
        }

        public void Stop(Action<ScheduledTask2> whenStopped = null)
        {
            bool isExecuting;
            TakeRunLock();
            try
            {
                isExecuting = StopInternal();
                if (isExecuting && whenStopped != null)
                {
                    if (_stopCallbacks == null)
                        _stopCallbacks = new Dictionary<int, List<Action<ScheduledTask2>>>();

                    if (!_stopCallbacks.ContainsKey(_runId))
                        _stopCallbacks[_runId] = new List<Action<ScheduledTask2>>();

                    _stopCallbacks[_runId].Add(whenStopped);
                }
            }
            finally
            {
                ReleaseRunLock();
            }

            if (!isExecuting)
                whenStopped?.Invoke(this);
        }
        
        private bool StopInternal()
        {
            if (!IsRunning) // todo - this whole IsRunning thing isn't _quite_ right.
                return false;

            var isExecuting = _runsExecuting.Contains(_runId);

            _runId++;
            IsRunning = false;

            return isExecuting;
        }

        public void UpdateSchedule(string schedule)
        {
            UpdateSchedule(new Schedule(schedule));
        }

        public void UpdateSchedule(Schedule schedule)
        {
            TakeRunLock();
            try
            {
                StopInternal();
                Schedule = schedule;
            }
            finally
            {
                ReleaseRunLock();
            }

            Start(PrevEvent);
        }

        private async Task Run(int runId)
        {
            while (true)
            {
                var delay = NextEvent - DateTime.UtcNow;
                var eventTime = NextEvent;
                await Task.Delay(delay > TimeSpan.Zero ? delay : TimeSpan.Zero).ConfigureAwait(false);
                
                TakeRunLock();
                try
                {
                    if (runId != _runId)
                        return;

                    PrevEvent = eventTime;
                    _runsExecuting.Add(runId);
                }
                finally
                {
                    ReleaseRunLock();
                }

                try
                {
                    Callback(this, eventTime);
                }
                catch (Exception ex)
                {
                    RaiseException(ex);
                }

                var stoppingAfterExec = false;
                TakeRunLock();
                try
                {
                    _runsExecuting.Remove(runId);

                    if (runId != _runId) // stop was called while we were executing the callback
                    {
                        stoppingAfterExec = true;
                        return;
                    }

                    NextEvent = Schedule.Next(eventTime);
                    if (SkipIfSlowCallback && NextEvent < DateTime.UtcNow)
                    {
                        NextEvent = Schedule.Next(DateTime.UtcNow);
                    }
                }
                catch (ValidTimeNotFoundException ex)
                {
                    CrashSchedule("Schtick Schedule has been terminated because the next valid time could not be found.", ex);
                    return; // can't continue if there's a problem setting up the next schedule
                }
                catch (Exception ex)
                {
                    CrashSchedule("Schtick Schedule has crashed.", ex); // I can't think of any reason this would actually hit, but if it does, it's probably not recoverable
                    return;
                }
                finally
                {
                    ReleaseRunLock();

                    if (stoppingAfterExec)
                    {
                        List<Action<ScheduledTask2>> callbacks;
                        if (_stopCallbacks != null && _stopCallbacks.TryGetValue(runId, out callbacks))
                        {
                            _stopCallbacks.Remove(runId);
                            if (_stopCallbacks.Count == 0)
                                _stopCallbacks = null;

                            foreach(var cb in callbacks)
                            {
                                cb(this);
                            }
                        }
                    }
                }
            }
        }

        private void TakeRunLock()
        {
            SpinWait.SpinUntil(() => Interlocked.CompareExchange(ref _runLocked, 1, 0) == 0);
        }

        private void ReleaseRunLock()
        {
            _runLocked = 0;
        }

        private void RaiseException(Exception ex)
        {
            Task.Run(() =>
            {
                var ev = OnException;
                ev?.Invoke(this, ex);

            }).ContinueWith(task => { });
        }

        private void CrashSchedule(string msg, Exception ex)
        {
            IsRunning = false;
            Interlocked.Increment(ref _runId);
            RaiseException(new ScheduleCrashException2(msg, this, ex));
        }
    }
}
