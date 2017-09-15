namespace SharpSoft.Data.GSQL
{
    using Expressions;
    using System.Collections.Generic;
    using System.Text;

    /// <summary>
    /// 创建表语句
    /// </summary>
    public class CreateTableStatement : IStatement
    {
        /// <summary>
        /// 决定创建表之前检查表是否不存在
        /// </summary>
        public bool IfNotExists { get; set; }
        public VariableExpression Table { get; set; }
        public CreateTableStatement()
        {
            ColumnDefines = new List<IColumnDefine>();
        }
        public List<IColumnDefine> ColumnDefines { get; private set; }
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("CREATE TABLE ");
            if (IfNotExists)
            {
                sb.Append("IFNOTEXISTS ");
            }
            sb.Append(Table).Append(" (");
            foreach (var item in ColumnDefines)
            {
                sb.Append(item).Append(",");
            }

            sb.Append(")");
            return sb.ToString();
        }
    }
}
