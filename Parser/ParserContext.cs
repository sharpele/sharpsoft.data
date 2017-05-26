using System;
using System.Collections.Generic;
using System.Text;

namespace SharpSoft.Data
{
    /// <summary>
    /// 解析器上下文
    /// </summary>
    public class ParserContext
    {
        public int StratIndex { get; set; }

        public int EndIndex { get; set; }

        public ParserContext Parent { get; set; }
    }
}
