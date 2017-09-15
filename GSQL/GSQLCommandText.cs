using System;
using System.Text;

namespace SharpSoft.Data.GSQL
{
    public class GSQLCommandText
    {
        internal SQLCommandText ToSql(SQLTextGenerator stg)
        {
            GSQLAnalyzer GLA = new GSQLAnalyzer(this.CommandText);
            var stams = GLA.ReadStatements();
            StringBuilder sb = new StringBuilder();
            foreach (var item in stams)
            {
                sb.Append(stg.ProcessStatement(item))
                    .AppendLine(";");
            }
            return new SQLCommandText(sb.ToString());
        }

        public string CommandText { get; set; }

        public static explicit operator string(GSQLCommandText gsql)
        {
            return gsql.CommandText;
        }
        public static explicit operator GSQLCommandText(string gsqltxt)
        {
            return new GSQLCommandText() { CommandText = gsqltxt };
        }
    }
}