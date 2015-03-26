using System;

namespace Alt
{
    public sealed class SchyntaxParseException : Exception
    {
        internal SchyntaxParseException(string message, int index, string input) : base (message)
        {
            Data["Index"] = index;
            Data["Input"] = input;
        }
    }
}
