using System;

namespace Schyntax.Internals
{
    public class Parser : ParserBase
    {
        public Parser(string input) : base(input) { }

        public ProgramNode Parse()
        {
            return ParseProgram();
        }

        private ProgramNode ParseProgram()
        {
            var program = new ProgramNode();

            while (!IsNext(TokenType.EndOfInput))
            {
                if (IsNext(TokenType.OpenCurly))
                {
                    program.AddGroup(ParseGroup());
                }
                else if (IsNext(TokenType.ExpressionName))
                {
                    program.AddExpression(ParseExpression());
                }
                else
                {
                    throw WrongTokenException(TokenType.OpenCurly, TokenType.ExpressionName, TokenType.Comma);
                }
                
                if (IsNext(TokenType.Comma)) // optional comma
                {
                    program.AddToken(Advance());
                }
            }

            program.AddToken(Expect(TokenType.EndOfInput));
            return program;
        }

        private GroupNode ParseGroup()
        {
            var group = new GroupNode();
            group.AddToken(Expect(TokenType.OpenCurly));

            while (!IsNext(TokenType.CloseCurly))
            {
                group.AddExpression(ParseExpression());

                if (IsNext(TokenType.Comma)) // optional comma
                {
                    group.AddToken(Advance());
                }
            }

            group.AddToken(Expect(TokenType.CloseCurly));
            return group;
        }

        private ExpressionNode ParseExpression()
        {
            var nameTok = Expect(TokenType.ExpressionName);
            var type = (ExpressionType)Enum.Parse(typeof(ExpressionType), nameTok.Value);
            var exp = new ExpressionNode(type);
            exp.AddToken(Expect(TokenType.OpenParen));

            while (true)
            {
                exp.AddArgument(ParseArgument(type));

                if (IsNext(TokenType.Comma)) // optional comma
                {
                    exp.AddToken(Advance());
                }

                if (IsNext(TokenType.CloseParen))
                {
                    break;
                }
            }

            exp.AddToken(Expect(TokenType.CloseParen));
            return exp;
        }

        private ArgumentNode ParseArgument(ExpressionType expressionType)
        {
            var arg = new ArgumentNode();

            if (IsNext(TokenType.Not))
            {
                arg.IsExclusion = true;
                arg.AddToken(Advance());
            }

            if (IsNext(TokenType.Wildcard))
            {
                arg.IsWildcard = true;
                arg.AddToken(Advance());
            }
            else
            {
                arg.Range = ParseRange(expressionType);
            }

            if (IsNext(TokenType.Interval))
            {
                arg.AddToken(Advance());
                arg.Interval = ParseIntegerValue(ExpressionType.IntervalValue);
            }

            return arg;
        }

        private RangeNode ParseRange(ExpressionType expressionType)
        {
            var range = new RangeNode();
            range.Start = expressionType == ExpressionType.Dates ? (ValueNode)ParseDate() : ParseIntegerValue(expressionType);

            var isRange = false;
            if (IsNext(TokenType.RangeInclusive))
            {
                isRange = true;
            }
            else if (IsNext(TokenType.RangeHalfOpen))
            {
                isRange = true;
                range.IsHalfOpen = true;
            }

            if (isRange)
            {
                range.AddToken(Advance());
                range.End = expressionType == ExpressionType.Dates ? (ValueNode)ParseDate() : ParseIntegerValue(expressionType);
            }

            return range;
        }

        private IntegerValueNode ParseIntegerValue(ExpressionType expressionType)
        {
            var val = new IntegerValueNode();

            if (IsNext(TokenType.PositiveInteger))
            {
                // positive integer is valid for anything
                var tok = Advance();
                val.AddToken(tok);
                val.Value = int.Parse(tok.Value);
            }
            else if (IsNext(TokenType.NegativeInteger))
            {
                if (expressionType != ExpressionType.DaysOfMonth)
                {
                    throw new SchyntaxParseException("Negative values are only allowed in dayofmonth expressions.", Input, Peek().Index);
                }

                var tok = Advance();
                val.AddToken(tok);
                val.Value = int.Parse(tok.Value);
            }
            else if (IsNext(TokenType.DayLiteral))
            {
                var tok = Advance();
                val.AddToken(tok);
                val.Value = DayToInteger(tok.Value);
            }
            else
            {
                switch (expressionType)
                {
                    case ExpressionType.DaysOfMonth:
                        throw WrongTokenException(TokenType.PositiveInteger, TokenType.NegativeInteger);
                    case ExpressionType.DaysOfWeek:
                        throw WrongTokenException(TokenType.PositiveInteger, TokenType.DayLiteral);
                    default:
                        throw WrongTokenException(TokenType.PositiveInteger);
                }
            }

            return val;
        }

        private DateValueNode ParseDate()
        {
            var date = new DateValueNode();

            var tok = Expect(TokenType.PositiveInteger);
            date.AddToken(tok);
            var one = int.Parse(tok.Value);

            date.AddToken(Expect(TokenType.ForwardSlash));

            tok = Expect(TokenType.PositiveInteger);
            date.AddToken(tok);
            var two = int.Parse(tok.Value);

            int three = -1;
            if (IsNext(TokenType.ForwardSlash))
            {
                date.AddToken(Expect(TokenType.ForwardSlash));

                tok = Expect(TokenType.PositiveInteger);
                date.AddToken(tok);
                three = int.Parse(tok.Value);
            }

            if (three != -1)
            {
                // date has a year
                date.Year = one;
                date.Month = two;
                date.Day = three;
            }
            else
            {
                // no year
                date.Month = one;
                date.Day = two;
            }

            return date;
        }

        public int DayToInteger(string day)
        {
            switch (day)
            {
                case "SUNDAY":
                    return 1;
                case "MONDAY":
                    return 2;
                case "TUESDAY":
                    return 3;
                case "WEDNESDAY":
                    return 4;
                case "THURSDAY":
                    return 5;
                case "FRIDAY":
                    return 6;
                case "SATURDAY":
                    return 7;
                default:
                    throw new Exception(day + " is not a day");
            }
        }
    }
}
