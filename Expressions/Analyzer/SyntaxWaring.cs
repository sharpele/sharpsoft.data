
using System;
using System.Collections.Generic;
using System.Text;

namespace SharpSoft.Data.Expressions
{
    using SharpSoft.Data.Lexing;
    /// <summary>
    /// 语法警告
    /// </summary>
    public class SyntaxWaring
    {
        /// <summary>
        /// 发生警告的标记
        /// </summary>
        public Token WaringToken { get; set; }
        /// <summary>
        /// 发生警告的表达式
        /// </summary>
        public IExpression Expression { get; set; }

        public string Message { get; set; }
    }
}
