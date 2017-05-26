using System;
using System.Collections.Generic;
using System.Text;

namespace SharpSoft.Data.Lexing
{
    public struct LexerSetting
    {
        /// <summary>
        /// 决定在词法分析的过程中，匹配文本时是否忽略大小写。
        /// </summary>
        public bool IgnoreCase { get; set; }
        /// <summary>
        /// 定义关键字
        /// </summary>
        public string[] Keywords { get; set; }

        /// <summary>
        /// 自定义操作符，列表中的内容将被识别为操作符。(注意：包含字符数多的应定义在前面，如“>=”应比“>”靠前，否则“>=”将被识别为“>”和“=”两个操作符)
        /// </summary>
        public string[] CustomOperators { get; set; }
        /// <summary>
        /// 变量名/占位符有效的首字符。（除额外指定的这些外，“下划线”、“英文字母”、“汉字”始终有效）
        /// </summary>
        public char[] LiteralFirstChars { get; set; }
        /// <summary>
        /// 字符串标识字符，一般为单引号或双引号。
        /// </summary>
        public char StringSign { get; set; }
        /// <summary>
        /// 转义类型
        /// </summary>
        public TransferredType TransferredType { get; set; }
        /// <summary>
        /// 行内注释开始符号
        /// </summary>
        public string InlineCommentsStartSign { get; set; }
        /// <summary>
        /// 行内注释结束符号
        /// </summary>
        public string InlineCommentsEndSign { get; set; }
        /// <summary>
        /// 整行注释起始符号
        /// </summary>
        public string OutlineCommentsSign { get; set; }
        /// <summary>
        /// 默认设置
        /// </summary>
        public static LexerSetting Default => new LexerSetting()
        {
            IgnoreCase=true,
            LiteralFirstChars = new char[] { '@', '#', '$' },
            StringSign = '\'',
            TransferredType = TransferredType.UseBackslash,
            InlineCommentsStartSign = "/*",
            InlineCommentsEndSign = "*/",
            OutlineCommentsSign = "--"
        };
    }
    /// <summary>
    /// 字符串内出现字符串标识时的转义方式
    /// </summary>
    public enum TransferredType
    {
        /// <summary>
        /// 用两个标识表示，如SqlServer中'张三''李四''王武'
        /// </summary>
        DoubleSign,
        /// <summary>
        /// 使用反斜杠转义，如MySql中'张三\'李四\'王武'
        /// </summary>
        UseBackslash
    }
}
