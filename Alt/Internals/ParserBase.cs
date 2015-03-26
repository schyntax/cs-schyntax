using System;
using System.Text;

namespace Alt.Internals
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
                ThrowWrongTokenException(type);

            return Advance();
        }

        protected bool IsNext(TokenType type)
        {
            return Peek().Type == type;
        }

        protected void ThrowWrongTokenException(params TokenType[] expectedTokenTypes)
        {
            var next = Peek();
            var msg = String.Format("Unexpected token type {0}. Was expecting one of: {1}\n\n{2}", next.Type, String.Join(", ", expectedTokenTypes), GetPointerToToken(next));
            throw new SchyntaxParseException(msg, next.Index, Input);
        }

        protected string GetPointerToToken(Token token)
        {
            return _lexer.GetPointerToIndex(token.Index);
        }
    }
}
