namespace SharpSoft.Data.GSQL
{
    using SharpSoft.Data.Expressions;
    using System.Text;
    /// <summary>
    /// 更新语句
    /// </summary>
    public class UpdateStatement : IStatement
    {
        public ListExpression Tables { get; set; }

        public ListExpression Updates { get; set; }

        public IExpression Where { get; set; }

        public ListExpression OrderBy { get; set; }

        public Limit Limit { get; set; }
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("UPDATE ").Append(Tables).Append(" SET ")
                .Append(Updates);
            if (Where != null)
            {
                sb.Append(" WHERE ").Append(Where);
            }
            if (OrderBy != null)
            {
                sb.Append(" ORDER BY ").Append(OrderBy);
            }
            if (Limit != null)
            {
                sb.Append(" Limit ").Append(Limit);
            }
            return sb.ToString();
        }
    }
}
