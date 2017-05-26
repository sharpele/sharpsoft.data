using System;
using System.Collections.Generic;
using System.Text;

namespace SharpSoft.Data
{
    /// <summary>
    /// 声明语句
    /// </summary>
    public class Declare : IStatement
    {
        /// <summary>
        /// 变量名称
        /// </summary>
        public string VarName { get; set; }

        public SqlDataTyoe DataType { get; set; }
    }
    public class SqlDataTyoe
    {
        public DataType DataType { get; set; }
        public int? Length1 { get; set; } = null;
        public int? Length2 { get; set; } = null;
        public static DataType GetDataType(string type)
        {
            if (string.IsNullOrEmpty(type))
            {
                throw new Exception();
            }
            switch (type.ToLower())
            {
                case "char":
                    return DataType.Char;
                case "varchar":
                    return DataType.VarChar;
                case "text":
                    return DataType.Text;
                case "int":
                    return DataType.Int;
                case "long":
                    return DataType.Long;
                case "decimal":
                    return DataType.Decimal;
                case "boolean":
                    return DataType.Boolean;
                case "binary":
                    return DataType.Binary;
                case "datetime":
                    return DataType.DateTime;
                case "time":
                    return DataType.Time;
                default:
                    throw new Exception("未定义该数据类型：[" + type + "]");
            }
        }

    }


    public enum DataType
    {
        /// <summary>
        /// 定长字符串
        /// </summary>
        Char,
        /// <summary>
        /// 变长字符串
        /// </summary>
        VarChar,
        /// <summary>
        /// 长文本
        /// </summary>
        Text,
        /// <summary>
        /// 整型
        /// </summary>
        Int,
        /// <summary>
        /// 长整型
        /// </summary>
        Long,
        /// <summary>
        /// 固定精度和比例的数字
        /// </summary>
        Decimal,
        /// <summary>
        /// 布尔型
        /// </summary>
        Boolean,
        /// <summary>
        /// 二进制数据
        /// </summary>
        Binary,
        /// <summary>
        /// 日期时间
        /// </summary>
        DateTime,
        /// <summary>
        /// 时间
        /// </summary>
        Time
    }
}
