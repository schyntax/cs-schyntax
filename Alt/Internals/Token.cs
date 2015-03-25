namespace Alt.Internals
{
    public class Token
    {
        public TokenType Type { get; set; }
        public string RawValue { get; set; }
        public string Value { get; set; }
        public int Index { get; set; }
        public string LeadingTrivia { get; set; }
        public string TrailingTrivia { get; set; }
    }

    public enum TokenType
    {
        // meta
        None = 0,
        EndOfInput,

        // operators
        Range,
        Interval,
        Not,
        OpenParen,
        CloseParen,
        OpenCurly,
        CloseCurly,
        ForwardSlash,
        Comma,
        Wildcard,

        // alpha-numeric
        PositiveInteger,
        NegativeInteger,
        ExpressionName,
        DayLiteral,
    }
}