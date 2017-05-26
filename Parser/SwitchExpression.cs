using System;
using System.Collections.Generic;
using System.Text;

namespace SharpSoft.Data
{
    public class SwitchExpression : IExpression
    {
        public IValue Condition { get; set; }
        /// <summary>
        /// 提供默认值
        /// </summary>
        public IValue Default { get; set; }

        public Dictionary<IValue, IValue> ValueResults { get; set; }
        public bool Alone { get => true; set { } }
    }
}
