using SharpSoft.Data.Expressions;
using System;
using System.Collections.Generic;
using System.Text;

namespace SharpSoft.Data.GSQL
{
    using Expressions;
    /// <summary>
    /// 定义复杂的约束
    /// </summary>
    public class ConstraintDefine : IColumnDefine
    {
        public VariableExpression Name { get; set; }
        public ConstraintType Type { get; set; }
        public ListExpression Columns { get; set; }
        public ForeignReferences References { get; set; }
        public override string ToString()
        {
            string ct="UNKNOW";
            switch (Type)
            {
                case ConstraintType.Unique:
                    ct = "UNIQUE";
                    break;
                case ConstraintType.PrimaryKey:
                    ct = "PRIMARY KEY";
                    break;
                case ConstraintType.ForeignKey:
                    ct = "FOREIGN KEY";
                    break; 
            }
            return $"CONSTRAINT {Name} {ct}({Columns}) {References}";
        }
    }
    /// <summary>
    /// 约束类型
    /// </summary>
    public enum ConstraintType
    {
        /// <summary>
        /// 唯一约束
        /// </summary>
        Unique,
        /// <summary>
        /// 主键约束
        /// </summary>
        PrimaryKey,
        /// <summary>
        /// 外键约束
        /// </summary>
        ForeignKey,

    }
    /// <summary>
    /// 外键引用
    /// </summary>
    public class ForeignReferences
    {
        /// <summary>
        /// 外键表
        /// </summary>
        public VariableExpression TableName { get; set; }
        /// <summary>
        /// 外键列
        /// </summary>
        public ListExpression Columns { get; set; }

        public override string ToString()
        {
            return $"REFERENCES {TableName}({Columns})";
        }

    }
}
