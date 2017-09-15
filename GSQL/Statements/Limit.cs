namespace SharpSoft.Data.GSQL
{
    using SharpSoft.Data.Expressions;
    public class Limit
    {
        public IExpression Offset { get; set; }

        public IExpression Rows { get; set; }

        public override string ToString()
        {
            if (Offset == null)
            {
                return Rows.ToString();
            }
            else
            {
                return $"{Offset},{Rows}";
            }
        }
    }
}
