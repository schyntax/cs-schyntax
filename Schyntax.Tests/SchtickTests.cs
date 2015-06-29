using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using NUnit.Framework;

namespace Schyntax.Tests
{
    [TestFixture]
    public class SchtickTests
    {
        private struct RunRecord
        {
            public DateTime Actual { get; }
            public DateTime Intended { get; }

            public int MillisecondsDifference => (int)Math.Abs((Actual - Intended).TotalMilliseconds);

            public RunRecord(DateTime actual, DateTime intended)
            {
                Actual = actual;
                Intended = intended;
            }
        }

        [Test]
        public void BasicExamples()
        {
            var schtick = new Schtick();

            var allRecords = new List<RunRecord>();
            var all = schtick.AddTask("sec(*)", (task, run) => { allRecords.Add(new RunRecord(DateTime.UtcNow, run)); });

            var evenRecords = new List<RunRecord>();
            var even = schtick.AddTask("sec(*%2)", (task, run) => { evenRecords.Add(new RunRecord(DateTime.UtcNow, run)); });

            // give them a chance to run
            Thread.Sleep(4000);

            // look at the results
            all.Stop();
            even.Stop();

            Assert.GreaterOrEqual(allRecords.Count, 3);
            Assert.LessOrEqual(allRecords.Count, 5);
            Assert.GreaterOrEqual(evenRecords.Count, 1);
            Assert.LessOrEqual(evenRecords.Count, 3);

            // make sure all of the events are within 100 milliseconds of the intended time
            foreach (var r in allRecords.Concat(evenRecords))
            {
                Assert.LessOrEqual(r.MillisecondsDifference, 100);
            }
        }

        [Test]
        public void TaskWindow()
        {
            var schtick = new Schtick();

            // generate a schedule for five seconds ago (and every minute at that second)
            var fiveSecAgo = DateTime.UtcNow.AddSeconds(-5);
            var sch = "sec(" + fiveSecAgo.Second + ")";

            // this should trigger an immediate event
            var window1Run = false;
            var window1 = schtick.AddTask(sch, (task, run) => { window1Run = true; }, lastKnownRun: fiveSecAgo.AddMinutes(-1), window: TimeSpan.FromMinutes(1));

            // this should not trigger an immediate event
            var window2Run = false;
            var window2 = schtick.AddTask(sch, (task, run) => { window2Run = true; }, lastKnownRun: fiveSecAgo, window: TimeSpan.FromMinutes(1));
            
            Thread.Sleep(1000);

            window1.Stop();
            window2.Stop();

            Assert.True(window1Run);
            Assert.False(window2Run);
        }
    }
}
