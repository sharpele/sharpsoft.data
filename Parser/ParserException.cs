using SharpSoft.Data.Lexing;
using System;
using System.Collections.Generic;
using System.Text;

namespace SharpSoft.Data
{
    internal class ParserException : Exception
    {
        public ParserException(string message) : base(message)
        {
        }
        public Token Token { get; set; }

        public override string ToString()
        {
            return base.ToString() + Token.ToString();
        }
    }
}
