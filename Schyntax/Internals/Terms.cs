using System.Text.RegularExpressions;

namespace Schyntax.Internals
{
    internal static class Terms
    {
        // literal terminals
        public static Terminal RangeInclusive { get; } = new Terminal(TokenType.RangeInclusive, "..");
        public static Terminal RangeHalfOpen { get; } = new Terminal(TokenType.RangeHalfOpen, "...");
        public static Terminal Interval { get; } = new Terminal(TokenType.Interval, "%");
        public static Terminal Not { get; } = new Terminal(TokenType.Not, "!");
        public static Terminal OpenParen { get; } = new Terminal(TokenType.OpenParen, "(");
        public static Terminal CloseParen { get; } = new Terminal(TokenType.CloseParen, ")");
        public static Terminal OpenCurly { get; } = new Terminal(TokenType.OpenCurly, "{");
        public static Terminal CloseCurly { get; } = new Terminal(TokenType.CloseCurly, "}");
        public static Terminal ForwardSlash { get; } = new Terminal(TokenType.ForwardSlash, "/");
        public static Terminal Comma { get; } = new Terminal(TokenType.Comma, ",");
        public static Terminal Wildcard { get; } = new Terminal(TokenType.Wildcard, "*");

        // regex terminals
        public static Terminal PositiveInteger { get; } = new Terminal(TokenType.PositiveInteger, null, new Regex(@"\G[0-9]+"));
        public static Terminal NegativeInteger { get; } = new Terminal(TokenType.NegativeInteger, null, new Regex(@"\G-[0-9]+"));

        public static Terminal Sunday { get; } = new Terminal(TokenType.DayLiteral, "SUNDAY", new Regex(@"\G(su|sun|sunday)(?:\b)", RegexOptions.IgnoreCase));
        public static Terminal Monday { get; } = new Terminal(TokenType.DayLiteral, "MONDAY", new Regex(@"\G(mo|mon|monday)(?:\b)", RegexOptions.IgnoreCase));
        public static Terminal Tuesday { get; } = new Terminal(TokenType.DayLiteral, "TUESDAY", new Regex(@"\G(tu|tue|tuesday|tues)(?:\b)", RegexOptions.IgnoreCase));
        public static Terminal Wednesday { get; } = new Terminal(TokenType.DayLiteral, "WEDNESDAY", new Regex(@"\G(we|wed|wednesday)(?:\b)", RegexOptions.IgnoreCase));
        public static Terminal Thursday { get; } = new Terminal(TokenType.DayLiteral, "THURSDAY", new Regex(@"\G(th|thu|thursday|thur|thurs)(?:\b)", RegexOptions.IgnoreCase));
        public static Terminal Friday { get; } = new Terminal(TokenType.DayLiteral, "FRIDAY", new Regex(@"\G(fr|fri|friday)(?:\b)", RegexOptions.IgnoreCase));
        public static Terminal Saturday { get; } = new Terminal(TokenType.DayLiteral, "SATURDAY", new Regex(@"\G(sa|sat|saturday)(?:\b)", RegexOptions.IgnoreCase));

        public static Terminal Seconds { get; } = new Terminal(ExpressionType.Seconds, new Regex(@"\G(s|sec|second|seconds|secondofminute|secondsofminute)(?:\b)", RegexOptions.IgnoreCase));
        public static Terminal Minutes { get; } = new Terminal(ExpressionType.Minutes, new Regex(@"\G(m|min|minute|minutes|minuteofhour|minutesofhour)(?:\b)", RegexOptions.IgnoreCase));
        public static Terminal Hours { get; } = new Terminal(ExpressionType.Hours, new Regex(@"\G(h|hour|hours|hourofday|hoursofday)(?:\b)", RegexOptions.IgnoreCase));
        public static Terminal DaysOfWeek { get; } = new Terminal(ExpressionType.DaysOfWeek, new Regex(@"\G(day|days|dow|dayofweek|daysofweek)(?:\b)", RegexOptions.IgnoreCase));
        public static Terminal DaysOfMonth { get; } = new Terminal(ExpressionType.DaysOfMonth, new Regex(@"\G(dom|dayofmonth|daysofmonth)(?:\b)", RegexOptions.IgnoreCase));
        public static Terminal Dates { get; } = new Terminal(ExpressionType.Dates, new Regex(@"\G(date|dates)(?:\b)", RegexOptions.IgnoreCase));

        internal class Terminal
        {
            public TokenType TokenType { get; }
            public string Value { get; }
            public Regex Regex { get; }

            internal Terminal(ExpressionType type, Regex regex) : this(TokenType.ExpressionName, type.ToString(), regex) { }

            internal Terminal(TokenType type, string value, Regex regex = null)
            {
                TokenType = type;
                Value = value;
                Regex = regex;
            }

            public Token GetToken(string input, int index)
            {
                if (Regex == null)
                {
                    if (input.Length - index < Value.Length)
                        return null;

                    for (var i = 0; i < Value.Length; i++)
                    {
                        if (input[index + i] != Value[i])
                            return null;
                    }

                    return CreateToken(index, Value);
                }

                var match = Regex.Match(input, index);
                if (!match.Success)
                    return null;

                return CreateToken(index, match.Value, match.Value);
            }

            private Token CreateToken(int index, string raw, string value = null)
            {
                return new Token()
                {
                    Type = TokenType,
                    Index = index,
                    RawValue = raw,
                    Value = Value ?? value,
                };
            }
        }
    }

}
