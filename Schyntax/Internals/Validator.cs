using System;
using System.Collections.Generic;

namespace Schyntax.Internals
{
    public class Validator
    {
        public string Input { get; }
        public ProgramNode Program { get; }

        public Validator(string input, ProgramNode program)
        {
            Input = input;
            Program = program;
        }

        public void AssertValid()
        {
            AssertProgram(Program);
        }

        private void AssertProgram(ProgramNode program)
        {
            if (program.Expressions.Count == 0)
            {
                // no free-floating expressions, so we need to make sure there is at least one group with an expression
                var hasExpressions = false;
                foreach (var group in program.Groups)
                {
                    if (group.Expressions.Count > 0)
                    {
                        hasExpressions = true;
                        break;
                    }
                }

                if (!hasExpressions)
                {
                    throw new SchyntaxParseException("Schedule must contain at least one expression.", Input, 0);
                }
            }

            foreach (var group in program.Groups)
            {
                Group(group);
            }

            ExpressionList(program.Expressions);
        }

        private void Group(GroupNode group)
        {
            ExpressionList(group.Expressions);
        }

        private void ExpressionList(IReadOnlyList<ExpressionNode> expressions)
        {
            foreach (var expression in expressions)
            {
                Expression(expression);
            }
        }

        private void Expression(ExpressionNode expression)
        {
            if (expression.Arguments.Count == 0)
                throw new SchyntaxParseException("Expression has no arguments.", Input, expression.Index);

            foreach (var arg in expression.Arguments)
            {
                if (arg.HasInterval && arg.IntervalValue == 0)
                {
                    throw new SchyntaxParseException("\"%0\" is not a valid interval. If your intention was to include all " + 
                        ExpressionTypeToHumanString(expression.ExpressionType) + 
                        " use the wildcard operator \"*\" instead of an interval", Input, arg.IntervalTokenIndex);
                }

                var validator = GetValidator(expression.ExpressionType);

                if (arg.IsWildcard)
                {
                    if (arg.IsExclusion && !arg.HasInterval)
                    {
                        throw new SchyntaxParseException(
                            "Wildcards can't be excluded with the ! operator, except when part of an interval (using %)",
                            Input, arg.Index);
                    }
                }
                else
                {
                    if (arg.Range == null || arg.Range.Start == null)
                    {
                        throw new SchyntaxParseException("Expected a value or range.", Input, arg.Index);
                    }
                    
                    Range(expression.ExpressionType, arg.Range, validator);
                }

                if (arg.HasInterval)
                {
                    validator(ExpressionType.IntervalValue, arg.Interval);
                }
            }
        }

        private ValueValidator GetValidator(ExpressionType expType)
        {
            switch (expType)
            {
                case ExpressionType.Seconds:
                case ExpressionType.Minutes:
                    return SecondOrMinute;
                case ExpressionType.Hours:
                    return Hour;
                case ExpressionType.DaysOfWeek:
                    return DayOfWeek;
                case ExpressionType.DaysOfMonth:
                    return DayOfMonth;
                case ExpressionType.DaysOfYear:
                    return DayOfYear;
                case ExpressionType.Dates:
                    return Date;
                default:
                    throw new NotImplementedException("ExpressionType " + expType + " has not been implemented by the validator.");
            }
        }

        private delegate void ValueValidator(ExpressionType expType, ValueNode value);

        private void Range(ExpressionType expType, RangeNode range, ValueValidator validator)
        {
            validator(expType, range.Start);
            if (range.End != null)
            {
                validator(expType, range.End);

                if (range.IsHalfOpen && ValuesAreEqual(expType, range.Start, range.End))
                    throw new SchyntaxParseException("Start and end values of a half-open range cannot be equal.", Input, range.Start.Index);
            }

            if (expType == ExpressionType.Dates && range.End != null)
            {
                // special validation to make the date range is sane
                var start = (DateValueNode)range.Start;
                var end = (DateValueNode)range.End;

                if (start.Year != null || end.Year != null)
                {
                    if (start.Year == null || end.Year == null)
                        throw new SchyntaxParseException("Cannot mix full and partial dates in a date range.", Input, start.Index);

                    if (!IsStartBeforeEnd(start, end))
                        throw new SchyntaxParseException("End date of range is before the start date.", Input, start.Index);
                }
            }
        }

        private void SecondOrMinute(ExpressionType expType, ValueNode value)
        {
            IntegerValue(expType, value, 0, 59);
        }

        private void Hour(ExpressionType expType, ValueNode value)
        {
            IntegerValue(expType, value, 0, 23);
        }

        private void DayOfWeek(ExpressionType expType, ValueNode value)
        {
            IntegerValue(expType, value, 1, 7);
        }

        private void DayOfMonth(ExpressionType expType, ValueNode value)
        {
            var ival = IntegerValue(expType, value, -31, 31);
            if (ival == 0)
                throw new SchyntaxParseException("Day of month cannot be zero.", Input, value.Index);
        }

        private void DayOfYear(ExpressionType expType, ValueNode value)
        {
            var ival = IntegerValue(expType, value, -366, 366);
            if (ival == 0)
                throw new SchyntaxParseException("Day of year cannot be zero.", Input, value.Index);
        }

        private void Date(ExpressionType expType, ValueNode value)
        {
            var date = (DateValueNode)value;

            if (date.Year != null)
            {
                if (date.Year < 1900 || date.Year > 2200)
                    throw new SchyntaxParseException("Year " + date.Year + " is not a valid year. Must be between 1900 and 2200.", Input, date.Index);
            }

            if (date.Month < 1 || date.Month > 12)
            {
                throw new SchyntaxParseException("Month " + date.Month + " is not a valid month. Must be between 1 and 12.", Input, date.Index);
            }

            var daysInMonth = DateTime.DaysInMonth(date.Year ?? 2000, date.Month); // default to a leap year, if no year is specified
            if (date.Day < 1 || date.Day > daysInMonth)
            {
                throw new SchyntaxParseException(date.Day + " is not a valid day for the month specified. Must be between 1 and " + daysInMonth, Input, date.Index);
            }
        }

        private int IntegerValue(ExpressionType type, ValueNode value, int min, int max)
        {
            var ival = ((IntegerValueNode)value).Value;
            if (ival < min || ival > max)
            {
                var msg = String.Format("{0} cannot be {1}. Value must be between {2} and {3}.",
                                    ExpressionTypeToHumanString(type), ival, min, max);

                throw new SchyntaxParseException(msg, Input, value.Index);
            }

            return ival;
        }

        private bool ValuesAreEqual(ExpressionType expType, ValueNode a, ValueNode b)
        {
            if (expType == ExpressionType.Dates)
            {
                var ad = (DateValueNode)a;
                var bd = (DateValueNode)b;

                if (ad.Day != bd.Day || ad.Month != bd.Month)
                    return false;

                if (ad.Year.HasValue && ad.Year != bd.Year)
                    return false;

                return true;
            }

            // integer values
            var ai = ((IntegerValueNode)a).Value;
            var bi = ((IntegerValueNode)b).Value;

            return ai == bi;
        }

        // returns true if the start date is before or equal to the end date
        private bool IsStartBeforeEnd(DateValueNode start, DateValueNode end)
        {
            if (start.Year < end.Year)
                return true;

            if (start.Year > end.Year)
                return false;

            // must be the same start and end year if we get here

            if (start.Month < end.Month)
                return true;

            if (start.Month > end.Month)
                return false;

            // must be the same month

            return start.Day <= end.Day;
        }

        private static string ExpressionTypeToHumanString(ExpressionType type)
        {
            switch (type)
            {
                case ExpressionType.DaysOfYear:
                    return "days of the year";
                case ExpressionType.DaysOfMonth:
                    return "days of the month";
                case ExpressionType.DaysOfWeek:
                    return "days of the week";
                case ExpressionType.IntervalValue:
                    return "interval";
                default:
                    return type.ToString().ToLowerInvariant();
            }
        }
    }
}