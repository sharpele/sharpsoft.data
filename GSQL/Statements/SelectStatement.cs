using System.Text;

namespace SharpSoft.Data.GSQL
{
    using SharpSoft.Data.Expressions;
    /// <summary>
    /// 查询语句
    /// </summary>
    public class SelectStatement : Expression, IStatement
    {
        public ListExpression Columns { get; set; }

        public ListExpression Tables { get; set; }

        public IExpression Where { get; set; }

        public ListExpression GroupBy { get; set; }

        public IExpression Having { get; set; }

        public ListExpression OrderBy { get; set; }

        public Limit Limit { get; set; }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("SELECT ")
                .Append(Columns);
            sb.Append(" FROM ")
             .Append(Tables);
            if (Where != null)
            {
                sb.Append(" WHERE ").Append(Where);
            }
            if (GroupBy != null && GroupBy.HasItem)
            {
                sb.Append(" GROUP BY ").Append(GroupBy);
            }
            if (Having != null)
            {
                sb.Append(" HAVING ").Append(Having);
            }
            if (OrderBy != null && OrderBy.HasItem)
            {
                sb.Append(" ORDER BY ").Append(OrderBy);
            }
            if (Limit != null)
            {
                sb.Append(" LIMIT ").Append(Limit);
            }



            return sb.ToString();
        }
    }
}
