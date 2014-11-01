using System;
using Irony.Parsing;
using NUnit.Framework;

namespace Schyntax.Tests
{
    [TestFixture]
    public class ParseTests
    {
        private Parser _parser;
        private SchyntaxGrammar _grammar;
        private ParseTree _tree;

        [TestCase("dates(12/25)")]
        [TestCase("dates(12/25..12/31)")]
        [TestCase("dates(2010/12/25)")]
        [TestCase("dates(2010/12/25..2010/12/31)")]
        [TestCase("dom(1)")]
        [TestCase("dom(30)")]
        [TestCase("dom(29)")]
        [TestCase("dom(8..10)")]
        [TestCase("dom(25..31)")]
        [TestCase("dom(-1)")]
        [TestCase("dom(-3)")]
        [TestCase("dom(-3..-1)")]
        [TestCase("dom(5..-1)")]
        [TestCase("dom(5..-2%2)")]
        [TestCase("dom(5%2)")]
        [TestCase("dom(%3)")]
        [TestCase("dom(20..10)")]
        [TestCase("dom(20..10%2)")]
        [TestCase("dow(sun)")]
        [TestCase("dow(sat..sun)")]
        [TestCase("dow(mon..fri)")]
        [TestCase("dow(mon..thu)] sat)", ExpectedException = typeof(AssertionException))]
        [TestCase("dow(%3)")]
        [TestCase("dow(thu%2)")]
        [TestCase("dow(thu..tue%2)")]
        [TestCase("h(6)")]
        [TestCase("h(!6)")]
        [TestCase("h(0..23)")]
        [TestCase("h(12..20)] 21)", ExpectedException = typeof(AssertionException))]
        [TestCase("h(12..22)")]
        [TestCase("h(!12..22)")]
        [TestCase("h(20..4)")]
        [TestCase("h(!20..4)")]
        [TestCase("h(%2)")]
        [TestCase("h(0%2)")]
        [TestCase("h(0..23%2)")]
        [TestCase("h(3%2)")]
        [TestCase("h(3..20%2)")]
        [TestCase("h(%3)")]
        [TestCase("m(6)")]
        [TestCase("m(!6)")]
        [TestCase("m(0..59)")]
        [TestCase("m(12..26)] 27)", ExpectedException = typeof(AssertionException))]
        [TestCase("m(12..28)")]
        [TestCase("m(!12..28)")]
        [TestCase("m(50..10)")]
        [TestCase("m(!50..10)")]
        [TestCase("m(%2)")]
        [TestCase("m(0%2)")]
        [TestCase("m(0..59%2)")]
        [TestCase("m(3%2)")]
        [TestCase("m(3..58%2)")]
        [TestCase("m(%3)")]
        [TestCase("s(6)")]
        [TestCase("s(!6)")]
        [TestCase("s(0..59)")]
        [TestCase("s(12..26)] 27)", ExpectedException = typeof(AssertionException))]
        [TestCase("s(12..28)")]
        [TestCase("s(!12..28)")]
        [TestCase("s(50..10)")]
        [TestCase("s(!50..10)")]
        [TestCase("s(%2)")]
        [TestCase("s(0%2)")]
        [TestCase("s(0..59%2)")]
        [TestCase("s(3%2)")]
        [TestCase("s(3..58%2)")]
        [TestCase("s(%3)")]
        // Complex stuff
        [TestCase("s(0) s(30)")]
        [TestCase("(s(0) s(30))")]
        [TestCase("(s(0) s(30)) s(!45)")]
        public void TestParse(string input)
        {
            DoParse(input);
        }

        [SetUp]
        public void SetUp()
        {
            _grammar = new SchyntaxGrammar();
            _parser = new Parser(_grammar) { Context = { TracingEnabled = false } };
            _tree = null;
        }
        
        private void DoParse(string input)
        {
            // Check for errors
            _parser.Language.Errors.ForEach(ge => Console.WriteLine("Grammar Error [{0} : {1}] {2}", ge.Level, ge.State, ge.Message));
            Assert.AreEqual(0, _parser.Language.Errors.Count);

            _tree = _parser.Parse(input);

            if (_parser.Context.ParserTrace.Count > 0)
            {
                Console.WriteLine("Parser Trace: ");
                const string columns = @"{0, -5} {1, -7} {2, -30} {3, -30} {4}";
                Console.WriteLine(columns, "State", "IsError", "StackTop", "Input", "Message");
                Console.WriteLine(new string('-', 100));
                _parser.Context.ParserTrace.ForEach(pte => Console.WriteLine(columns, pte.State, pte.IsError, pte.StackTop, "\"" + pte.Input + "\"", pte.Message));
            }

            if (_tree != null)
            {
                if (_tree.ParserMessages.Count > 0)
                    _tree.ParserMessages.ForEach(lm => Console.WriteLine("Parser Message: at [{0}]: [{1}]", lm.Location.ToUiString(), lm.Message));

                Assert.AreEqual(ParseTreeStatus.Parsed, _tree.Status);
                Assert.AreEqual(ParserStatus.Accepted, _parser.Context.Status);
                Assert.AreEqual(0, _tree.ParserMessages.Count);

                Assert.NotNull(_tree.Root.AstNode);
                //Assert.IsInstanceOf<Program>(_tree.Root.AstNode);
            }
        }
    }
}
