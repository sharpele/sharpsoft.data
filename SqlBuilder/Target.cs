using System;
using System.Collections.Generic;
using System.Text;

namespace SharpSoft.Data
{
    /// <summary>
    /// 在sql语句中描述的数据库目标对象
    /// </summary>
    public class Target
    {
        /// <summary>
        /// 目标的名称组成部分
        /// </summary>
        public List<string> NameParts { get; set; }

        public override string ToString()
        {
            return string.Join(",", NameParts?.ToArray());
        }
    }
    /// <summary>
    /// 带有别名的数据库目标
    /// </summary>
    public class TargetWithAlias : Target,IValue
    {
        public string Alias { get; set; }
        public override string ToString()
        {
            return base.ToString() + (Alias == null ? "" : (" as " + Alias));
        }
    }

}
