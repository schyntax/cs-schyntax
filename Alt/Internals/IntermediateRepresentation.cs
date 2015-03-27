using System.Collections.Generic;

namespace Alt.Internals
{
    /// <summary>
    /// This is the data structure used by the runtime search algorithm.
    /// 
    /// This is not a true IR because it doesn't get compiled into anything lower. However, there's no reason it 
    /// couldn't be compiled into an optimized search algorithm someday in the future, and in that case, it would be 
    /// a real IR. So I'm just going to call it that anyway.
    /// </summary>
    public class IrProgram
    {
        public List<IrGroup> Groups { get; } = new List<IrGroup>();

        internal IrProgram() { }
    }

    public class IrGroup
    {
        private List<IrIntegerRange> _seconds;
        public bool HasSeconds => _seconds != null && _seconds.Count > 1;
        public List<IrIntegerRange> Seconds => _seconds ?? (_seconds = new List<IrIntegerRange>());

        private List<IrIntegerRange> _secondsExcluded;
        public bool HasSecondsExcluded => _secondsExcluded != null && _secondsExcluded.Count > 1;
        public List<IrIntegerRange> SecondsExcluded => _secondsExcluded ?? (_secondsExcluded = new List<IrIntegerRange>());

        private List<IrIntegerRange> _minutes;
        public bool HasMinutes => _minutes != null && _minutes.Count > 1;
        public List<IrIntegerRange> Minutes => _minutes ?? (_minutes = new List<IrIntegerRange>());

        private List<IrIntegerRange> _minutesExcluded;
        public bool HasMinutesExcluded => _minutesExcluded != null && _minutesExcluded.Count > 1;
        public List<IrIntegerRange> MinutesExcluded => _minutesExcluded ?? (_minutesExcluded = new List<IrIntegerRange>());

        private List<IrIntegerRange> _hours;
        public bool HasHours => _hours != null && _hours.Count > 1;
        public List<IrIntegerRange> Hours => _hours ?? (_hours = new List<IrIntegerRange>());

        private List<IrIntegerRange> _hoursExcluded;
        public bool HasHoursExcluded => _hoursExcluded != null && _hoursExcluded.Count > 1;
        public List<IrIntegerRange> HoursExcluded => _hoursExcluded ?? (_hoursExcluded = new List<IrIntegerRange>());

        private List<IrIntegerRange> _daysOfWeek;
        public bool HasDaysOfWeek => _daysOfWeek != null && _daysOfWeek.Count > 1;
        public List<IrIntegerRange> DaysOfWeek => _daysOfWeek ?? (_daysOfWeek = new List<IrIntegerRange>());

        private List<IrIntegerRange> _daysOfWeekExcluded;
        public bool HasDaysOfWeekExcluded => _daysOfWeekExcluded != null && _daysOfWeekExcluded.Count > 1;
        public List<IrIntegerRange> DaysOfWeekExcluded => _daysOfWeekExcluded ?? (_daysOfWeekExcluded = new List<IrIntegerRange>());

        private List<IrIntegerRange> _daysOfMonth;
        public bool HasDaysOfMonth => _daysOfMonth != null && _daysOfMonth.Count > 1;
        public List<IrIntegerRange> DaysOfMonth => _daysOfMonth ?? (_daysOfMonth = new List<IrIntegerRange>());

        private List<IrIntegerRange> _daysOfMonthExcluded;
        public bool HasDaysOfMonthExcluded => _daysOfMonthExcluded != null && _daysOfMonthExcluded.Count > 1;
        public List<IrIntegerRange> DaysOfMonthExcluded => _daysOfMonthExcluded ?? (_daysOfMonthExcluded = new List<IrIntegerRange>());

        private List<IrDateRange> _dates;
        public bool HasDates => _dates != null && _dates.Count > 1;
        public List<IrDateRange> Dates => _dates ?? (_dates = new List<IrDateRange>());

        private List<IrDateRange> _datesExcluded;
        public bool HasDatesExcluded => _datesExcluded != null && _datesExcluded.Count > 1;
        public List<IrDateRange> DatesExcluded => _datesExcluded ?? (_datesExcluded = new List<IrDateRange>());

        internal IrGroup() { }
    }

    public class IrIntegerRange
    {
        public bool IsRange { get; }
        public bool IsSplit { get; }
        public int Start { get; }
        public int End { get; }
        public int Interval { get; }
        public bool HasInterval { get; }

        internal IrIntegerRange(int start, int? end, int interval, bool isSplit)
        {
            Start = start;
            End = end ?? 0;
            IsRange = end.HasValue;
            Interval = interval;
            HasInterval = interval != 0;
            IsSplit = isSplit;
        }
    }

    public class IrDateRange
    {
        public bool IsRange { get; }
        public bool IsSplit { get; }
        public IrDate Start { get; }
        public IrDate End { get; }
        public bool DatesHaveYear { get; }
        public int Interval { get; }
        public bool HasInterval { get; }

        internal IrDateRange(IrDate start, IrDate? end, int interval, bool isSplit)
        {
            Start = start;
            DatesHaveYear = start.Year != 0;

            if (end.HasValue)
            {
                IsRange = true;
                End = end.Value;
            }

            Interval = interval;
            HasInterval = interval != 0;
            IsSplit = isSplit;
        }
    }

    public struct IrDate
    {
        public int Year { get; }
        public int Month { get; }
        public int Day { get; }

        internal IrDate(int? year, int month, int day)
        {
            Year = year ?? 0;
            Month = month;
            Day = day;
        }
    }
}
