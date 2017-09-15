namespace SharpSoft.Data
{
    public class SQLCommandText
    {
        public SQLCommandText(string text)
        {
            Text = text;
        }
        public string Text { get; set; }

        public static implicit operator string(SQLCommandText sql)
        {
            return sql.Text;
        }
        public static explicit operator SQLCommandText(string text)
        {
            return new SQLCommandText(text);
        }
    }
}
