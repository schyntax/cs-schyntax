using System;
using Alt.Internals;
using NUnit.Framework;

namespace Alt
{
    public class Tests
    {
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
        [TestCase("dow(mon..sun)")]
        [TestCase("dow(mon..thu)] sat)", ExpectedException = typeof(Exception))]
        [TestCase("dow(%3)")]
        [TestCase("dow(thu%2)")]
        [TestCase("dow(thu..tue%2)")]
        [TestCase("h(6)")]
        [TestCase("h(!6)")]
        [TestCase("h(0..23)")]
        [TestCase("h(12..20)] 21)", ExpectedException = typeof(Exception))]
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
        [TestCase("m(12..26)] 27)", ExpectedException = typeof(Exception))]
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
        [TestCase("s(12..26)] 27)", ExpectedException = typeof(Exception))]
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
        // groups
        [TestCase("s(0) s(30)")]
        [TestCase("{s(0) s(30)}")]
        [TestCase("{s(0) s(30)} s(!45)")]
        public void TestLexer(string input)
        {
            var lex = new Lexer(input);
            Token tok;
            while ((tok = lex.Advance()).Type != TokenType.EndOfInput)
            {
                Console.WriteLine(tok.Value);
            }
        }
    }
}