using System.Text;

namespace SharpSoft.Data.GSQL
{
    using SharpSoft.Data.Expressions;
    /// <summary>
    /// 插入语句
    /// </summary>
    public class InsertStatement : IStatement
    {
        public IExpression Table { get; set; }

        public IExpression Columns { get; set; }

        public IExpression Values { get; set; }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder("INSERT INTO ");
            sb.Append(Table)
            .Append(" ")
            .Append(Columns);
            if (Values != null)
            {
                sb.Append(" VALUES ")
                            .Append(Values);
            }

            return sb.ToString();
        }
    }
}
