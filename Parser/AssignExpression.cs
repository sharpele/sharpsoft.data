using System;
using System.Collections.Generic;
using System.Text;

namespace SharpSoft.Data
{
    /// <summary>
    /// 赋值表达式
    /// </summary>
    public class AssignExpression : IExpression
    {
        public IValue Left { get; set; }

        public IValue Right { get; set; }
        public bool Alone { get => true; set { } }
    }
}
