﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Jil;
using NUnit.Framework;

namespace Schyntax.Tests
{
    [TestFixture]
    public class Dates : SchyntaxTestRunner
    {
        public override string SuiteName => "dates";
    }

    [TestFixture]
    public class DaysOfYear : SchyntaxTestRunner
    {
        public override string SuiteName => "daysOfYear";
    }

    [TestFixture]
    public class DaysOfMonth : SchyntaxTestRunner
    {
        public override string SuiteName => "daysOfMonth";
    }

    [TestFixture]
    public class DaysOfWeek : SchyntaxTestRunner
    {
        public override string SuiteName => "daysOfWeek";
    }

    [TestFixture]
    public class Hours : SchyntaxTestRunner
    {
        public override string SuiteName => "hours";
    }

    [TestFixture]
    public class Minutes : SchyntaxTestRunner
    {
        public override string SuiteName => "minutes";
    }

    [TestFixture]
    public class Seconds : SchyntaxTestRunner
    {
        public override string SuiteName => "seconds";
    }

    [TestFixture]
    public class SyntaxErrors : SchyntaxTestRunner
    {
        public override string SuiteName => "syntaxErrors";
    }

    [TestFixture]
    public class ArgumentErrors : SchyntaxTestRunner
    {
        public override string SuiteName => "argumentErrors";
    }

    [TestFixture]
    public class Commas : SchyntaxTestRunner
    {
        public override string SuiteName => "commas";
    }

    public abstract class SchyntaxTestRunner
    {
        private static SchyntaxTests _tests;
        
        private static SchyntaxTests Tests
        {
            get
            {
                if (_tests != null)
                    return _tests;

                var refs = Assembly.GetAssembly(typeof(SchyntaxTestRunner)).GetManifestResourceNames();
                Console.WriteLine(refs);

                // load json file
                using (var stream = Assembly.GetAssembly(typeof(SchyntaxTestRunner)).GetManifestResourceStream("Schyntax.Tests.tests.json"))
                using (var reader = new StreamReader(stream))
                {
                    _tests = JSON.Deserialize<SchyntaxTests>(reader, new Options(dateFormat: DateTimeFormat.ISO8601));
                }

                return _tests;
            }
        }

        public abstract string SuiteName { get; }

        public IEnumerable Checks => Tests.Suites[SuiteName].Select(c => new object[] { c.Format, c.Date, c.Prev, c.Next, c.ParseErrorIndex });

        [TestCaseSource("Checks")]
        public void Check(string format, DateTimeOffset start, DateTimeOffset? prev, DateTimeOffset? next, int? parseErrorIndex)
        {
            Schedule sch;

            try
            {
                sch = new Schedule(format);

                if (parseErrorIndex.HasValue)
                    throw new Exception($"Expected a parse error at index {parseErrorIndex}, but no exception was thrown.");
            }
            catch (SchyntaxParseException ex)
            {
                if (parseErrorIndex.HasValue)
                {
                    if (ex.Index == parseErrorIndex)
                        return;

                    throw new Exception($"Wrong parse error index. Expected: {parseErrorIndex}. Actual: {ex.Index}", ex);
                }

                throw;
            }
            
            try
            {
                var actualPrev = sch.Previous(start);
                if (!prev.HasValue)
                    throw new Exception($"Expected a ValidTimeNotFoundException. Date returned from Previous: {actualPrev}.");

                Assert.AreEqual(prev.Value.ToString("o"), actualPrev.ToString("o"), "Previous time was incorrect.");
            }
            catch (ValidTimeNotFoundException)
            {
                if (prev.HasValue)
                    throw;
            }

            try
            {
                var actualNext = sch.Next(start);
                if (!next.HasValue)
                    throw new Exception($"Expected a ValidTimeNotFoundException. Date returned from Next: {actualNext}.");

                Assert.AreEqual(next.Value.ToString("o"), actualNext.ToString("o"), "Next time was incorrect.");
            }
            catch (ValidTimeNotFoundException)
            {
                if (next.HasValue)
                    throw;
            }
        }
    }

    public class SchyntaxTests
    {
        [JilDirective("testsVersion")]
        public int TestsVersion { get; set; }

        [JilDirective("hash")]
        public string Hash { get; set; }

        [JilDirective("suites")]
        public Dictionary<string, List<SchyntaxCheck>> Suites { get; set; }
    }

    public class SchyntaxCheck
    {
        [JilDirective("format")]
        public string Format { get; set; }

        [JilDirective("date")]
        public DateTimeOffset Date { get; set; }

        [JilDirective("prev")]
        public DateTimeOffset? Prev { get; set; }

        [JilDirective("next")]
        public DateTimeOffset? Next { get; set; }

        [JilDirective("parseErrorIndex")]
        public int? ParseErrorIndex { get; set; }
    }
}
