using System;
using System.Collections.Generic;

namespace Alt.Internals
{
    public static class IrBuilder
    {
        /// <summary>
        /// This method assumes the AST is valid and makes absolutely no attempt to further validate any node.
        /// </summary>
        /// <param name="program"></param>
        /// <returns></returns>
        public static IrProgram CompileAst(ProgramNode program)
        {
            var ir = new IrProgram();

            // free-floating expressions are placed in an implicit group
            var irGroup = CompileGroup(program.Expressions);
            if (irGroup != null)
                ir.Groups.Add(irGroup);

            // compile all groups
            foreach (var groupNode in program.Groups)
            {
                irGroup = CompileGroup(groupNode.Expressions);
                if (irGroup != null)
                    ir.Groups.Add(irGroup);
            }

            return ir;
        }

        private static IrGroup CompileGroup(IReadOnlyList<ExpressionNode> expressions)
        {
            if (expressions == null || expressions.Count == 0)
                return null;

            var irGroup = new IrGroup();

            foreach (var expression in expressions)
            {
                CompileExpression(irGroup, expression);
            }

            // setup implied rules
            if (irGroup.HasSeconds || irGroup.HasSecondsExcluded)
            {
                // don't need to setup any defaults if seconds are defined
            }
            else if (irGroup.HasMinutes || irGroup.HasMinutesExcluded)
            {
                irGroup.Seconds.Add(GetZeroInteger());
            }
            else if (irGroup.HasHours || irGroup.HasHoursExcluded)
            {
                irGroup.Seconds.Add(GetZeroInteger());
                irGroup.Minutes.Add(GetZeroInteger());
            }
            else // only a date level expression was set
            {
                irGroup.Seconds.Add(GetZeroInteger());
                irGroup.Minutes.Add(GetZeroInteger());
                irGroup.Hours.Add(GetZeroInteger());
            }

            return irGroup;
        }

        private static void CompileExpression(IrGroup irGroup, ExpressionNode expression)
        {
            foreach (var arg in expression.Arguments)
            {
                switch (expression.ExpressionType)
                {
                    case ExpressionType.Seconds:
                        CompileSecondsArgument(irGroup, arg);
                        break;
                    case ExpressionType.Minutes:
                        CompileMinutesArgument(irGroup, arg);
                        break;
                    case ExpressionType.Hours:
                        CompileHoursArgument(irGroup, arg);
                        break;
                    case ExpressionType.DaysOfWeek:
                        CompileDaysOfWeekArgument(irGroup, arg);
                        break;
                    case ExpressionType.DaysOfMonth:
                        CompileDaysOfMonthArgument(irGroup, arg);
                        break;
                    case ExpressionType.Dates:
                        CompileDateArgument(irGroup, arg);
                        break;
                    default:
                        throw new Exception("Expression type " + expression.ExpressionType + " not supported by the schyntax compiler." + SchyntaxException.PLEASE_REPORT_BUG_MSG);
                }
            }
        }

        private static void CompileDateArgument(IrGroup irGroup, ArgumentNode arg)
        {
            IrDate irStart;
            IrDate? irEnd = null;
            var isSplit = false;

            if (arg.IsWildcard)
            {
                irStart = new IrDate(null, 1, 1);
                irEnd = new IrDate(null, 12, 31);
            }
            else
            {
                var start = (DateValueNode)arg.Range.Start;
                irStart = new IrDate(start.Year, start.Month, start.Day);

                if (arg.Range.End != null)
                {
                    var end = (DateValueNode)arg.Range.End;
                    irEnd = new IrDate(end.Year, end.Month, end.Day);
                }
                else if (arg.HasInterval)
                {
                    // if there is an interval, but no end value specified, then the end value is implied
                    irEnd = new IrDate(null, 12, 31);
                }

                // check for split range (spans January 1) - not applicable for dates with explicit years
                if (irEnd.HasValue && !start.Year.HasValue)
                {
                    if (irStart.Month >= irEnd.Value.Month && 
                        (irStart.Month > irEnd.Value.Month || irStart.Day > irEnd.Value.Day))
                    {
                        isSplit = true;
                    }
                }
            }

            var irArg = new IrDateRange(irStart, irEnd, arg.HasInterval ? arg.IntervalValue : 0, isSplit);
            (arg.IsExclusion ? irGroup.DatesExcluded : irGroup.Dates).Add(irArg);
        }

        private static void CompileSecondsArgument(IrGroup irGroup, ArgumentNode arg)
        {
            var irArg = CompileIntegerArgument(arg, 0, 59);
            (arg.IsExclusion ? irGroup.SecondsExcluded : irGroup.Seconds).Add(irArg);
        }

        private static void CompileMinutesArgument(IrGroup irGroup, ArgumentNode arg)
        {
            var irArg = CompileIntegerArgument(arg, 0, 59);
            (arg.IsExclusion ? irGroup.MinutesExcluded : irGroup.Minutes).Add(irArg);
        }

        private static void CompileHoursArgument(IrGroup irGroup, ArgumentNode arg)
        {
            var irArg = CompileIntegerArgument(arg, 0, 23);
            (arg.IsExclusion ? irGroup.HoursExcluded : irGroup.Hours).Add(irArg);
        }

        private static void CompileDaysOfWeekArgument(IrGroup irGroup, ArgumentNode arg)
        {
            var irArg = CompileIntegerArgument(arg, 1, 7);
            (arg.IsExclusion ? irGroup.DaysOfWeekExcluded : irGroup.DaysOfWeek).Add(irArg);
        }

        private static void CompileDaysOfMonthArgument(IrGroup irGroup, ArgumentNode arg)
        {
            var irArg = CompileIntegerArgument(arg, 1, 31);
            (arg.IsExclusion ? irGroup.DaysOfMonthExcluded : irGroup.DaysOfMonth).Add(irArg);
        }

        private static IrIntegerRange CompileIntegerArgument(ArgumentNode arg, int wildStart, int wildEnd)
        {
            int start;
            int? end;
            var isSplit = false;

            if (arg.IsWildcard)
            {
                start = wildStart;
                end = wildEnd;
            }
            else
            {
                start = ((IntegerValueNode)arg.Range.Start).Value;
                end = (arg.Range.End as IntegerValueNode)?.Value;

                if (!end.HasValue && arg.HasInterval)
                {
                    // if there is an interval, but no end value specified, then the end value is implied
                    end = wildEnd;
                }

                // check for a split range
                if (end.HasValue && end < start)
                {
                    // Start is greater than end, so it's probably a split range, but there's one exception.
                    // If this is a month expression, and end is a negative number (counting back from the end of the month)
                    // then it might not actually be a split range
                    if (start < 0 || end > 0)
                    {
                        // check says that either start is negative or end is positive
                        // (means we're probably not in the weird day of month scenario)
                        // todo: implement a better check which looks for possible overlap between a positive start and negative end
                        isSplit = true;
                    }
                }
            }

            return new IrIntegerRange(start, end, arg.HasInterval ? arg.IntervalValue : 0, isSplit);
        }

        private static IrIntegerRange GetZeroInteger()
        {
            return new IrIntegerRange(0, null, 0, false);
        }
    }
}
