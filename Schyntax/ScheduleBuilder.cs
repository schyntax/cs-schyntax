using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Irony.Parsing;

namespace Schyntax
{
    public static class ScheduleBuilder
    {
        public static List<RuleGroup> ParseAndCompile(string text)
        {
            var grammar = new SchyntaxGrammar();
            var parser = new Parser(grammar);
            var parseTree = parser.Parse(text);

            if (parseTree.Status != ParseTreeStatus.Error)
                throw new InvalidOperationException(parseTree.ParserMessages.First().ToString());

            throw new NotImplementedException();
        }
    }
}