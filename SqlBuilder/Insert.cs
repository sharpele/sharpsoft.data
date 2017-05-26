using System;
using System.Collections.Generic;
using System.Text;

namespace SharpSoft.Data
{
    public class Insert:IStatement
    {
        public Target Table { get; set; }
        /// <summary>
        /// 需要插入的字段列表，可为空
        /// </summary>
        public List<Target> Fields { get; set; }

        public List<IValue> Values { get; set; }
    }
}
