using System;
using System.Collections.Generic;
using System.Text;

namespace SharpSoft.Data
{
    public class WildcardExpression : IExpression
    {
        public Variable Table { get; set; }
        public bool Alone { get => true; set { } }
    }
}
