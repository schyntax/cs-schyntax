using System;

namespace Schyntax.Internals
{
    public abstract class ParserBase
    {
        private readonly Lexer _lexer;

        public string Input => _lexer.Input;

        protected ParserBase(string input)
        {
            _lexer = new Lexer(input);
        }

        protected Token Peek()
        {
            return _lexer.Peek();
        }

        protected Token Advance()
        {
            return _lexer.Advance();
        }

        /// <summary>
        /// Throws an exception if the next token is not of the specified type.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        protected Token Expect(TokenType type)
        {
            if (!IsNext(type))
                throw WrongTokenException(type);

            return Advance();
        }

        protected bool IsNext(TokenType type)
        {
            return Peek().Type == type;
        }

        protected SchyntaxParseException WrongTokenException(params TokenType[] expectedTokenTypes)
        {
            var next = Peek();

            var msg = "Unexpected token type " + next.Type + " at index " + next.Index + ". Was expecting " +
                (expectedTokenTypes.Length > 1
                    ? "one of: " + String.Join(", ", expectedTokenTypes)
                    : expectedTokenTypes[0].ToString());

            return new SchyntaxParseException(msg, Input, next.Index);
        }
    }
}
