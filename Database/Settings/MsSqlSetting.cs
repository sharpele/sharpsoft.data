using System;
using System.Collections.Generic;
using System.Text;

namespace SharpSoft.Data
{
    public class MsSqlSetting : TSqlSetting
    {
        public override string GetDataType(SqlDataTyoe type)
        {
            string t = "";
            switch (type.DataType)
            {
                case DataType.Char:
                    t = "char";
                    break;
                case DataType.VarChar:
                    t = "varchar";
                    break;
                case DataType.Text:
                    t = "text";
                    break;
                case DataType.Int:
                    t = "int";
                    break;
                case DataType.Long:
                    t = "bigint";
                    break;
                case DataType.Decimal:
                    t = "decimal";
                    break;
                case DataType.Boolean:
                    t = "bit";
                    break;
                case DataType.Binary:
                    t = "binary";
                    break;
                case DataType.DateTime:
                    t = "datetime";
                    break;
                case DataType.Time:
                    t = "time";
                    break;
                default:
                    throw new NotSupportedException("不支持的类型：" + type.DataType.ToString());
            }
            string getlength(int len)
            {
                if (len >= 0)
                {
                    return len.ToString(); ;
                }
                else
                {//为负表示使用最大值
                    return "max";
                }
            }
            if (type.Length1.HasValue && type.Length2.HasValue)
            {
                return $"{t}({getlength(type.Length1.Value)},{getlength(type.Length2.Value)})";
            }
            else if (type.Length1.HasValue)
            {
                return $"{t}({getlength(type.Length1.Value)})";
            }
            else
            {
                return t;
            }
        }

        public override string getdate()
        {
            return "getdate()";
        }

        public override string get_last_insert_id()
        {
            return "@@IDENTITY";
        }
    }
}
