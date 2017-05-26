using System;
using System.Collections.Generic;
using System.Text;

namespace SharpSoft.Data.Lexing
{
    public class LexerException:Exception
    {
        public LexerException(string message) : base(message)
        {
        }

        public int Position { get; set; }
    }
}
