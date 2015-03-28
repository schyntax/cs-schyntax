namespace Alt.Internals
{
    public class Token
    {
        public TokenType Type { get; internal set; }
        public string RawValue { get; internal set; }
        public string Value { get; internal set; }
        public int Index { get; internal set; }
        public string LeadingTrivia { get; internal set; }
        public string TrailingTrivia { get; internal set; }

        internal Token() { }
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