using System;
using System.Collections.Generic;

namespace Schyntax
{
    public class RuleGroup
    {
        public Range<int>[] Seconds { get; set; }
        public Range<int>[] SecondsExclude { get; set; }
        public Range<int>[] Minutes { get; set; }
        public Range<int>[] MinutesExclude { get; set; }
        public Range<int>[] Hours { get; set; }
        public Range<int>[] HoursExclude { get; set; }
        public Range<int>[] DaysOfWeek { get; set; }
        public Range<int>[] DaysOfWeekExclude { get; set; }
        public Range<int>[] DaysOfMonth { get; set; }
        public Range<int>[] DaysOfMonthExclude { get; set; }
        public Range<Range<int>>[] Dates { get; set; }
        public Range<Range<int>>[] DatesExclude { get; set; }


        public DateTime? GetEvent(DateTime start, bool after)
        {
            start = start.ToUniversalTime();

            var inc = after ? 1 : -1;
            var initSecond = after ? 0 : 59;
            var initMinute = after ? 0 : 59;
            var initHour = after ? 0 : 23;

            var yearFromTomorrow = start.AddDays(1).AddYears(1);
            var days = (yearFromTomorrow - start).TotalDays;
            for (var d = 0; d < days; d++)
            {
                var date = start.AddDays(d * inc);
                if (d > 0)
                {
                    date = new DateTime(
                        date.Year,
                        date.Month,
                        date.Day,
                        initHour,
                        initMinute,
                        initSecond,
                        0);
                }
                else if (after)
                {
                    date = date.AddSeconds(1);
                }

                var year = date.Year;
                var dayOfWeek = (int)date.DayOfWeek;
                var dayOfMonth = date.Day;
                var month = date.Month;
                var hour = date.Hour;
                var minute = date.Minute;
                var second = date.Second;
                var daysInMonth = DateTime.DaysInMonth(year, month);

                bool applicable;
                if (Dates != null && Dates.Length > 0)
                {
                    applicable = false;
                    for (var i = 0; i < Dates.Length; ++i)
                    {
                        var range = Dates[i];
                        if (InDateRange(month, dayOfMonth, year, range))
                        {
                            applicable = true;
                            break;
                        }
                    }

                    if (!applicable)
                        continue;
                }

                if (DatesExclude != null && DatesExclude.Length > 0)
                {
                    var excluded = false;
                    for (var i = 0; i < DatesExclude.Length; ++i)
                    {
                        var range = DatesExclude[i];
                        if (InDateRange(month, dayOfMonth, year, range))
                        {
                            excluded = true;
                            break;
                        }
                    }
                    if (excluded)
                        continue;
                }

                if (DaysOfMonth != null && DaysOfMonth.Length > 0)
                {
                    applicable = false;
                    for (var i = 0; i < DaysOfMonth.Length; ++i)
                    {
                        var range = DaysOfMonth[i].Copy();
                        if (range.low < 0)
                            range.low = daysInMonth + range.low + 1;

                        if (range.high < 0)
                            range.high = daysInMonth + range.high + 1;

                        if (InRange(dayOfMonth, range, DateTime.DaysInMonth(month - 1, year), 1))
                        {
                            applicable = true;
                            break;
                        }
                    }
                    if (!applicable)
                        continue;
                }

                if (DaysOfMonthExclude != null && DaysOfMonthExclude.Length > 0)
                {
                    var excluded = false;
                    for (var i = 0; i < DaysOfMonthExclude.Length; ++i)
                    {
                        var range = DaysOfMonthExclude[i].Copy();
                        if (range.low < 0)
                            range.low = daysInMonth + range.low + 1;

                        if (range.high < 0)
                            range.high = daysInMonth + range.high + 1;

                        if (InRange(dayOfMonth, range, DateTime.DaysInMonth(month - 1, year), 1))
                        {
                            excluded = true;
                            break;
                        }
                    }
                    if (excluded)
                        continue;
                }

                if (DaysOfWeek != null && !InRule(DaysOfWeek, 7, dayOfWeek))
                    continue;

                if (DaysOfWeekExclude != null && InRule(DaysOfWeekExclude, 7, dayOfWeek))
                    continue;

                // if we've gotten this far, then today is an applicable day, let's keep going with hour checks
                var firstHour = true;
                var hourCount = after ? 24 - hour : hour + 1;
                for (; hourCount-- > 0; hour += inc, minute = initMinute, second = initSecond)
                {
                    if (Hours != null && !InRule(Hours, 24, hour))
                        continue;

                    if (HoursExclude != null && InRule(HoursExclude, 24, hour))
                        continue;

                    // if we've gotten here, the date and hour are valid. Let's check for minutes
                    var minuteCount = after ? 60 - minute : minute + 1;
                    for (; minuteCount-- > 0; minute += inc, second = initSecond)
                    {
                        if (Minutes != null && !InRule(Minutes, 60, minute))
                            continue;

                        if (MinutesExclude != null && InRule(MinutesExclude, 60, minute))
                            continue;

                        // check for valid seconds
                        var secondCount = after ? 60 - second : second + 1;
                        for (; secondCount-- > 0; second += inc)
                        {
                            if (Seconds != null && !InRule(Seconds, 60, second))
                                continue;

                            if (SecondsExclude != null && InRule(SecondsExclude, 60, second))
                                continue;

                            // we've found our next event
                            return DateTime.SpecifyKind(new DateTime(date.Year, month, dayOfMonth, hour, minute, second), DateTimeKind.Utc);
                        }
                    }
                }
            }

            return null;
        }

        private static bool InRule(IList<Range<int>> ranges, int lengthOfUnit, int value)
        {
            for (var i = 0; i < ranges.Count; i++)
            {
                if (InRange(value, ranges[i], lengthOfUnit))
                    return true;
            }

            return false;
        }

        private static bool InRange(int value, Range<int> range, int lengthOfUnit, int min = 0)
        {
            //function inRange (value, range, lengthOfUnit, min)
            //{
            //    min = min || 0;
            //
            if (range.isSplit) // range spans across the max value and loops back around
            {
                if (value <= range.high || value >= range.low)
                {
                    if (range.modulus != null)
                    {
                        if (value >= range.low)
                            return (value - range.low) % range.modulus == 0;

                        return (value + lengthOfUnit - range.low) % range.modulus == 0;
                    }

                    return true;
                }
            }
            else // not a split range (easier case)
            {
                if (value >= range.low && value <= range.high)
                {
                    if (range.modulus != null)
                        return (value - range.low) % range.modulus == 0;

                    return true;
                }
            }
            //}

            return false;

        }


        private static bool InDateRange(int month, int day, int year, Range<Range<int>> range)
        {
            // first, check if in-between low and high dates.
            if (range.isSplit)
            {
                if (month == range.low.month || month == range.high.month)
                {
                    if (month == range.low.month && day < range.low.day)
                        return false;

                    if (month == range.high.month && day > range.high.day)
                        return false;
                }
                else if (!(month < range.high.month || month > range.low.month))
                {
                    return false;
                }
            }
            else
            {
                // start with month range check
                if (month < range.low.month || month > range.high.month)
                    return false;

                if (month == range.low.month || month == range.high.month)
                {
                    // month is equal, so check month and day
                    if (month == range.low.month && day < range.low.day)
                        return false;

                    if (month == range.high.month && day > range.high.day)
                        return false;
                }
            }

            if (range.modulus == null) // if there's no modulus, in-between dates is the only comparison required
                return true;


            // figure out the actual date of the low date
            DateTime start;
            if (range.isSplit && month <= range.high.month)
            {
                // start date is from previous year
                start = DateTime.SpecifyKind(new DateTime(year - 1, range.low.month, range.low.day), DateTimeKind.Utc);
            }
            else
            {
                start = DateTime.SpecifyKind(new DateTime(year, range.low.month, range.low.day), DateTimeKind.Utc);
            }

            // check if start date was actually supposed to be February 29th, but isn't because of non-leap-year.
            if (range.low.month == 1 && range.low.day == 29 && start.Month != 1)
            {
                // bump the start day back to February 28th so that modulus schemes work based on that imaginary date
                // but seriously, people should probably just expect weird results if they're doing something that stupid.
                start = start.AddDays(-1);
            }

            var current = DateTime.SpecifyKind(new DateTime(year, month, day), DateTimeKind.Utc);
            //var dayCount = Math.round((current - start) / (24 * 60 * 60 * 1000));
            var dayCount = Math.Round((current - start).TotalDays);

            return dayCount % range.modulus == 0;
        }
    }

    public class Range<T>
    {
        public T low { get; set; }
        public T high { get; set; }
        public bool isSplit { get; set; }
        public int? modulus { get; set; }

        public Range<T> Copy()
        {
            return new Range<T>
            {
                low = low,
                high = high,
                isSplit = isSplit,
                modulus = modulus
            };
        }

        public T day
        {
            get { return high; }
            set { high = value; }
        }

        public T month
        {
            get { return low; }
            set { low = value; }
        }
    }
}