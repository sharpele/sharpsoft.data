using System;
using System.Collections.Generic;
using System.Text;

namespace SharpSoft.Data
{
    public abstract class SqlBuilderSetting
    {
        /// <summary>
        /// 提供数据库语句构造器需要转义的字符
        /// </summary>
        /// <returns></returns>
        public abstract Dictionary<char, string> GetCharTransferreds();
    }
    public class MsSqlBuilderSetting : SqlBuilderSetting
    {
        public override Dictionary<char, string> GetCharTransferreds()
        {
            return new Dictionary<char, string>() {
                { '\'', "''" }//单引号转换为双引号
            };
        }
    }
    public class MySqlBuilderSetting : SqlBuilderSetting
    {
        public override Dictionary<char, string> GetCharTransferreds()
        {
            return new Dictionary<char, string>() {
                { '\n', "\\n" },
                { '\r',"\\r"},
                { '\t',"\\t"},
                { '\0',"\\0"},
                { '\b',"\\b"},
                { '\'',"\\'"},
                { '"',"\\\""},
                { '%',"\\%"},
                { '_',"\\_"},
                { '\\',"\\\\"}
            };
        }
    }
    public class SqliteBuilderSetting : SqlBuilderSetting
    {
        public override Dictionary<char, string> GetCharTransferreds()
        {
            return new Dictionary<char, string>() {
                { '/', "//" },
                { '\'',"''"},
                { '[',"/["},
                { ']',"/]"},
                { '%',"/%"},
                { '_',"/_"},
                { '&',"/&"},
                { '(',"/("},
                { ')',"/)"}
            };
        }
    }
}
