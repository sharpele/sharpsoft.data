using System;
using System.Collections.Generic;
using System.Text;

namespace SharpSoft.Data.Expressions
{
    /// <summary>
    /// 条件表达式
    /// </summary>
    public class ConditionalExpression:Expression
    {
        public IExpression Condition { get; set; }
        public IExpression TrueValue { get; set; }
        public IExpression FalseValue { get; set; }
        public override string ToString()
        {
            return $"{Condition}?{TrueValue}:{FalseValue}";
        }
    }
}
