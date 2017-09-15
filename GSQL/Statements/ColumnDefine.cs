
using System;
using System.Collections.Generic;
using System.Text;

namespace SharpSoft.Data.GSQL
{
    using SharpSoft.Data.Expressions;
    /// <summary>
    /// 数据列定义
    /// </summary>
    public class ColumnDefine:IColumnDefine
    {
        public VariableExpression Name { get; set; }
        public DataType Type { get; set; }
        /// <summary>
        /// 数据列类型的描述符，一般为长度和精度。
        /// </summary>
        public ListExpression TypeDescriptor { get; set; }
        public bool IsPrimaryKey { get; set; }
        /// <summary>
        /// 是否为自增列
        /// </summary>
        public bool Autoincrement { get; set; }
        public bool IsUnique { get; set; }
        public bool Nullable { get; set; } = true;
        public IExpression DefaultValue { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        public IExpression Comment { get; set; }

        public override string ToString()
        {
            return $"{Name} {Type}({TypeDescriptor}){(IsPrimaryKey?" PRIMARY KEY":"")}{(IsUnique ? " UNIQUE" : "")}{(Autoincrement ? " AUTO" : "")}{(Nullable ? " NULL" : " NOT NULL")}{(DefaultValue!=null ? $" DEFAULT {DefaultValue}" : "")}{(Comment!=null ? $" COMMENT {Comment}" : "")}";
        }
    }
}
