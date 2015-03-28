using System;
using System.Collections.Generic;
using Schyntax.Internals;

namespace Schyntax
{
    public class Schedule
    {
        private readonly IrProgram _ir;
        public string OriginalText { get; }

        public Schedule(string schedule)
        {
            OriginalText = schedule;

            var parser = new Parser(schedule);
            var ast = parser.Parse();

            var validator = new Validator(schedule, ast);
            validator.AssertValid();

            _ir = IrBuilder.CompileAst(ast);
        }

        public DateTime Next()
        {
            return Next(DateTime.UtcNow);
        }

        public DateTime Next(DateTime after)
        {
            return GetEvent(after, SearchMode.After);
        }

        public DateTime Previous()
        {
            return Next(DateTime.UtcNow);
        }

        public DateTime Previous(DateTime atOrBefore)
        {
            return GetEvent(atOrBefore, SearchMode.AtOrBefore);
        }

        private enum SearchMode : byte
        {
            AtOrBefore,
            After
        }

        private DateTime GetEvent(DateTime start, SearchMode mode)
        {
            start = start.ToUniversalTime();
            var found = false;
            var result = default(DateTime);

            foreach (var group in _ir.Groups)
            {
                DateTime e;
                if (TryGetGroupEvent(group, start, mode, out e))
                {
                    if (!found 
                        || (mode == SearchMode.After && e < result) 
                        || (mode == SearchMode.AtOrBefore && e > result))
                    {
                        result = e;
                        found = true;
                    }
                }
            }

            if (!found)
                throw new ValidTimeNotFoundException(OriginalText);

            return result;
        }

        // yes yes, I know it's complicated, but settle down ReSharper
        // ReSharper disable once FunctionComplexityOverflow
        private static bool TryGetGroupEvent(IrGroup group, DateTime start, SearchMode mode, out DateTime result)
        {
            var after = mode == SearchMode.After;
            var inc = after ? 1 : -1; // used for incrementing values up or down depending on the direction we're searching

            var initHour = after ? 0 : 23;
            var initMinute = after ? 0 : 59;
            var initSecond = after ? 0 : 59;

            // todo: make the length of the search configurable
            for (var d = 0; d < 367; d++)
            {
                DateTime date;
                int hour, minute, second;
                if (d == 0)
                {
                    // "after" events must be in the future
                    date = after ? start.AddSeconds(1) : start;

                    hour = date.Hour;
                    minute = date.Minute;
                    second = date.Second;
                }
                else
                {
                    date = start.AddDays(d * inc);

                    hour = initHour;
                    minute = initMinute;
                    second = initSecond;
                }

                var year = date.Year;
                var month = date.Month;
                var dayOfWeek = (int)date.DayOfWeek + 1; // DayOfWeek enum is zero-indexed
                var dayOfMonth = date.Day;
                var daysInMonth = DateTime.DaysInMonth(year, month);

                // check if today is an applicable date
                if (group.HasDates)
                {
                    var applicable = false;
                    foreach (var range in group.Dates)
                    {
                        if (InDateRange(range, year, month, dayOfMonth))
                        {
                            applicable = true;
                            break;
                        }
                    }

                    if (!applicable)
                        goto CONTINUE_DATE_LOOP;
                }

                if (group.HasDatesExcluded)
                {
                    foreach (var range in group.DatesExcluded)
                    {
                        if (InDateRange(range, year, month, dayOfMonth))
                            goto CONTINUE_DATE_LOOP;
                    }
                }

                // check if date is an applicable day of month
                if (group.HasDaysOfMonth)
                {
                    var applicable = false;
                    foreach (var range in group.DaysOfMonth)
                    {
                        if (InDayOfMonthRange(range, year, month, dayOfMonth))
                        {
                            applicable = true;
                            break;
                        }
                    }

                    if (!applicable)
                        goto CONTINUE_DATE_LOOP;
                }

                if (group.HasDaysOfMonthExcluded)
                {
                    foreach (var range in group.DaysOfMonthExcluded)
                    {
                        if (InDayOfMonthRange(range, year, month, dayOfMonth))
                            goto CONTINUE_DATE_LOOP;
                    }
                }

                // check if date is an applicable day of week
                if (group.HasDaysOfWeek && !InRule(7, group.DaysOfWeek, dayOfWeek))
                    goto CONTINUE_DATE_LOOP;

                if (group.HasDaysOfWeekExcluded && InRule(7, group.DaysOfWeekExcluded, dayOfWeek))
                    goto CONTINUE_DATE_LOOP;

                // if we've gotten this far, then today is an applicable day, let's keep going with hour checks
                var hourCount = after ? 24 - hour : hour + 1;
                for (; hourCount-- > 0; hour += inc, minute = initMinute, second = initSecond)
                {
                    if (group.HasHours && !InRule(24, group.Hours, hour))
                        continue;

                    if (group.HasHoursExcluded && InRule(24, group.HoursExcluded, hour))
                        continue;

                    // if we've gotten here, the date and hour are valid. Let's check for minutes
                    var minuteCount = after ? 60 - minute : minute + 1;
                    for (; minuteCount-- > 0; minute += inc, second = initSecond)
                    {
                        if (group.HasMinutes && !InRule(60, group.Minutes, minute))
                            continue;

                        if (group.HasMinutesExcluded && InRule(60, group.MinutesExcluded, minute))
                            continue;

                        // check for valid seconds
                        var secondCount = after ? 60 - second : second + 1;
                        for (; secondCount-- > 0; second += inc)
                        {
                            if (group.HasSeconds && !InRule(60, group.Seconds, second))
                                continue;

                            if (group.HasSecondsExcluded && InRule(60, group.SecondsExcluded, second))
                                continue;

                            // we've found our event
                            result = new DateTime(year, month, dayOfMonth, hour, minute, second, DateTimeKind.Utc);
                            return true;
                        }
                    }
                }

                CONTINUE_DATE_LOOP:;
            }

            // we didn't find an applicable date
            result = default(DateTime);
            return false;
        }

        private static bool InRule(int lengthOfUnit, List<IrIntegerRange> ranges, int value)
        {
            foreach (var range in ranges)
            {
                if (InIntegerRange(range, value, lengthOfUnit))
                    return true;
            }

            return false;
        }

        private static bool InDateRange(IrDateRange range, int year, int month, int dayOfMonth)
        {
            // first, check if this is actually a range
            if (!range.IsRange)
            {
                // not a range, so just to a straight comparison
                if (range.Start.Month != month || range.Start.Day != dayOfMonth)
                    return false;

                if (range.DatesHaveYear && range.Start.Year != year)
                    return false;

                return true;
            }

            // check if in-between start and end dates.
            if (range.DatesHaveYear)
            {
                // when we have a year, the check is much simpler because the range can't be split
                if (year < range.Start.Year || year > range.End.Year)
                    return false;

                if (year == range.Start.Year && CompareMonthAndDay(month, dayOfMonth, range.Start.Month, range.Start.Day) == -1)
                    return false;

                if (year == range.End.Year && CompareMonthAndDay(month, dayOfMonth, range.End.Month, range.End.Day) == 1)
                    return false;
            }
            else if (range.IsSplit) // split ranges aren't allowed to have years (it wouldn't make any sense)
            {
                if (month == range.Start.Month || month == range.End.Month)
                {
                    if (month == range.Start.Month && dayOfMonth < range.Start.Day)
                        return false;

                    if (month == range.End.Month && dayOfMonth > range.End.Day)
                        return false;
                }
                else if (!(month < range.End.Month || month > range.Start.Month))
                {
                    return false;
                }
            }
            else
            {
                // not a split range, and no year information - just month and day to go on
                if (CompareMonthAndDay(month, dayOfMonth, range.Start.Month, range.Start.Day) == -1)
                    return false;

                if (CompareMonthAndDay(month, dayOfMonth, range.End.Month, range.End.Day) == 1)
                    return false;
            }

            // If we get here, then we're definitely somewhere within the range.
            // If there's no interval, then there's nothing else we need to check
            if (!range.HasInterval)
                return true;

            // figure out the actual date of the low date so we know whether we're on the desired interval
            int startYear;
            if (range.DatesHaveYear)
            {
                startYear = range.Start.Year;
            }
            else if (range.IsSplit && month <= range.End.Month)
            {
                // start date is from the previous year
                startYear = year - 1;
            }
            else
            {
                startYear = year;
            }

            var startDay = range.Start.Day;

            // check if start date was actually supposed to be February 29th, but isn't because of non-leap-year.
            if (range.Start.Month == 2 && range.Start.Day == 29 && DateTime.DaysInMonth(startYear, 2) != 29)
            {
                // bump the start day back to February 28th so that interval schemes work based on that imaginary date
                // but seriously, people should probably just expect weird results if they're doing something that stupid.
                startDay = 28;
            }

            var start = new DateTime(startYear, range.Start.Month, startDay, 0, 0, 0, DateTimeKind.Utc);
            var current = new DateTime(year, month, dayOfMonth, 0, 0, 0, DateTimeKind.Utc);
            var dayCount = (current - start).Days;

            return (dayCount % range.Interval) == 0;
        }

        // returns 0 if A and B are equal, -1 if A is before B, or 1 if A is after B
        private static int CompareMonthAndDay(int monthA, int dayA, int monthB, int dayB)
        {
            if (monthA == monthB)
            {
                if (dayA == dayB)
                    return 0;

                return dayA > dayB ? 1 : -1;
            }

            return monthA > monthB ? 1 : -1;
        }

        private static bool InDayOfMonthRange(IrIntegerRange range, int year, int month, int dayOfMonth)
        {
            if (range.Start < 0 || (range.IsRange && range.End < 0))
            {
                // one of the range values is negative, so we need to convert it to a positive by counting back from the end of the month
                var daysInMonth = DateTime.DaysInMonth(year, month);
                range = range.CloneWithRevisedRange(
                    range.Start < 0 ? daysInMonth + range.Start + 1 : range.Start,
                    range.End < 0 ? daysInMonth + range.End + 1 : range.End
                );
            }

            return InIntegerRange(range, dayOfMonth, DaysInPreviousMonth(year, month), 1);
        }

        private static bool InIntegerRange(IrIntegerRange range, int value, int lengthOfUnit, int min = 0)
        {
            if (!range.IsRange)
            {
                return value == range.Start;
            }

            if (range.IsSplit) // range spans across the max value and loops back around
            {
                if (value <= range.End || value >= range.Start)
                {
                    if (range.HasInterval)
                    {
                        if (value >= range.Start)
                            return (value - range.Start) % range.Interval == 0;

                        return (value + lengthOfUnit - range.Start) % range.Interval == 0;
                    }

                    return true;
                }
            }
            else // not a split range (easier case)
            {
                if (value >= range.Start && value <= range.End)
                {
                    if (range.HasInterval)
                        return (value - range.Start) % range.Interval == 0;

                    return true;
                }
            }

            return false;
        }

        private static int DaysInPreviousMonth(int year, int month)
        {
            month--;
            if (month == 0)
            {
                year--;
                month = 12;
            }

            return DateTime.DaysInMonth(year, month);
        }
    }
}
