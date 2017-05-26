using System;
using System.Collections.Generic;
using System.Text;

namespace SharpSoft.Data.Lexing
{
    public class Token
    {
        public Token(TokenType type, string content)
        {
            Type = type;
            Content = content;
        }
        public TokenType Type { get; }
        public string Content { get; set; }

        public int Position { get;internal set; }
        public override string ToString()
        {
            return $"{Type}:[{(Content=="\0"?"\\0":Content)}] Position:{Position}";
        }
    }
    public enum TokenType
    {
        /// <summary>
        /// 文字
        /// </summary>
        Literal,
        /// <summary>
        /// 保留关键字
        /// </summary>
        Keyword,
        /// <summary>
        /// 圆括号
        /// </summary>
        Parenthese,
        /// <summary>
        /// 方括号
        /// </summary>
        Bracket,
        /// <summary>
        /// 花括号
        /// </summary>
        CurlyBracket,
        /// <summary>
        /// 逗号
        /// </summary>
        Comma,
        /// <summary>
        /// 冒号
        /// </summary>
        Colon,
        /// <summary>
        /// 问号
        /// </summary>
        Question,
        /// <summary>
        /// 分号
        /// </summary>
        Semicolon,
        /// <summary>
        /// 运算符
        /// </summary>
        Operator, 
        /// <summary>
        /// 字符串
        /// </summary>
        String,
        /// <summary>
        /// 数字
        /// </summary>
        Numeric, 
        /// <summary>
        /// 注释文本
        /// </summary>
        Comments,
        /// <summary>
        /// 结束标识
        /// </summary>
        End
    }
}
