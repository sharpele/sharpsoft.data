
using System;
using System.Collections.Generic;
using System.Text;

namespace SharpSoft.Data.Expressions
{ 
    public class OrderExpression : Expression
    {
        public IExpression Expression { get; set; }
        public OrderType OrderType { get; set; }

        public override string ToString()
        {
            return $"{Expression} {OrderType}";
        }
    }

    public enum OrderType
    {
        Asc,
        Desc
    }
}
