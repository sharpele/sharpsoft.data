using SharpSoft.Data.Lexing;
using System;

namespace SharpSoft.Data.Expressions
{
    public class SyntaxException : Exception
    {
        public SyntaxException(string message) : base(message)
        {
        }
        public Token Token { get; set; }

        public override string Message => base.Message + (Token == null ? "" : Token.ToString());

        public override string ToString()
        {
            return base.ToString() + Token.ToString();
        }
    }
}
