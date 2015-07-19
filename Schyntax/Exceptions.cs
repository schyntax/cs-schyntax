using System;
using System.Text;

namespace Schyntax
{
    public abstract class SchyntaxException : Exception
    {
        internal const string PLEASE_REPORT_BUG_MSG = " This indicates a bug in Schyntax. Please open an issue on github.";
        protected SchyntaxException(string message, Exception innerException = null) : base(message, innerException) { }
    }

    public sealed class SchyntaxParseException : SchyntaxException
    {
        internal SchyntaxParseException(string message, string input, int index) : base (message + "\n\n" + GetPointerToIndex(input, index))
        {
            Data["Index"] = index;
            Data["Input"] = input;
        }

        internal static string GetPointerToIndex(string input, int index)
        {
            var start = Math.Max(0, index - 20);
            var length = Math.Min(input.Length - start, 50);

            StringBuilder sb = new StringBuilder(73);
            sb.Append(input.Substring(start, length));
            sb.Append("\n");

            for (var i = start; i < index; i++)
                sb.Append(' ');

            sb.Append('^');
            return sb.ToString();
        }
    }

    public sealed class InvalidScheduleException : SchyntaxException
    {
        internal InvalidScheduleException(string message, string input) : base (message)
        {
            Data["Input"] = input;
        }
    }

    public sealed class ValidTimeNotFoundException : SchyntaxException
    {
        public const string NOT_FOUND_MSG = "A valid time was not found for the schedule.";

        internal ValidTimeNotFoundException(string schedule, string message = NOT_FOUND_MSG) : base (message)
        {
            Data["Schedule"] = schedule;
        }
    }
}
