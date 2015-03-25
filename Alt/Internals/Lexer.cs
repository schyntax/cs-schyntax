using System;

namespace Alt.Internals
{
    public class Lexer : LexerBase
    {
        public Lexer(string input) : base(input)
        {
            _lexMethod = LexList;
        }

        private LexMethod LexPastEndOfInput()
        {
            throw new Exception("Lexer was advanced past the end of the input.");
        }

        private LexMethod LexList()
        {
            ConsumeOptionalTerm(Terms.Comma);

            if (EndOfInput())
                return LexPastEndOfInput;

            if (Context == ContextMode.Program)
            {
                if (IsNextTerm(Terms.OpenCurly))
                    return LexGroup;
            }
            else if (Context == ContextMode.Group)
            {
                if (ConsumeOptionalTerm(Terms.CloseCurly))
                {
                    ExitContext();
                    return LexList;
                }
            }
            else if (Context == ContextMode.Expression)
            {
                if (ConsumeOptionalTerm(Terms.CloseParen))
                {
                    ExitContext();
                    return LexList;
                }
            }

            if (Context == ContextMode.Expression)
                return LexExpressionArgument;

            return LexExpression;
        }

        private LexMethod LexGroup()
        {
            ConsumeTerm(Terms.OpenCurly);
            EnterContext(ContextMode.Group);
            return LexList;
        }

        private LexMethod LexExpression()
        {
            if (ConsumeOptionalTerm(Terms.Seconds) ||
                ConsumeOptionalTerm(Terms.Minutes) ||
                ConsumeOptionalTerm(Terms.Hours) ||
                ConsumeOptionalTerm(Terms.DaysOfWeek) ||
                ConsumeOptionalTerm(Terms.DaysOfMonth) ||
                ConsumeOptionalTerm(Terms.Dates))
            {
                ConsumeTerm(Terms.OpenParen);
                EnterContext(ContextMode.Expression);

                return LexList;
            }

            throw new Exception("Expected an expression at index " + _index);
        }

        private LexMethod LexExpressionArgument()
        {
            ConsumeOptionalTerm(Terms.Not);

            var hasValues = ConsumeNumberDayOrDate(false);
            if (hasValues)
            {
                // might be a range
                if (ConsumeOptionalTerm(Terms.Range))
                    ConsumeNumberDayOrDate(true);
            }

            var hasInterval = ConsumeOptionalTerm(Terms.Interval);
            if (hasInterval)
            {
                ConsumeTerm(Terms.PositiveInteger);
            }

            if (!hasValues && !hasInterval)
                throw new Exception("Expected an argument at index " + _index);

            if (!IsNextTerm(Terms.Comma) && !IsNextTerm(Terms.CloseParen))
                throw new Exception("Expected a comma or close paren at index " + _index);

            return LexList;
        }

        // this lexes a number, day, or date
        private bool ConsumeNumberDayOrDate(bool required)
        {
            if (ConsumeOptionalTerm(Terms.PositiveInteger))
            {
                // this might be a date - check for slashes
                if (ConsumeOptionalTerm(Terms.ForwardSlash))
                {
                    ConsumeTerm(Terms.PositiveInteger);

                    // might have a year... one more check
                    if (ConsumeOptionalTerm(Terms.ForwardSlash))
                        ConsumeTerm(Terms.PositiveInteger);
                }

                return true;
            }

            if (ConsumeOptionalTerm(Terms.NegativeInteger) ||
                ConsumeOptionalTerm(Terms.Sunday) ||
                ConsumeOptionalTerm(Terms.Monday) ||
                ConsumeOptionalTerm(Terms.Tuesday) ||
                ConsumeOptionalTerm(Terms.Wednesday) ||
                ConsumeOptionalTerm(Terms.Thursday) ||
                ConsumeOptionalTerm(Terms.Friday) ||
                ConsumeOptionalTerm(Terms.Saturday))
            {
                return true;
            }

            if (required)
                throw new Exception("Expected a number, day, or date at index " + _index);

            return false;
        }
    }
}
