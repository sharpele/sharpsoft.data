using System;
using System.Collections.Generic;
using System.Text;

namespace SharpSoft.Data
{
    /// <summary>
    /// As表达式，用于类型转换函数CAST中或用于指定别名
    /// </summary>
    public class AsExpression : IExpression
    {
        public IValue Value { get; set; }

        public string As { get; set; }
        public bool Alone { get => true; set { } }
    }
}
