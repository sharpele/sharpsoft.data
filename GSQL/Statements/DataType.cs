using System;
using System.Collections.Generic;
using System.Text;

namespace SharpSoft.Data.GSQL
{
    public enum DataType
    {
        Guid,
        /// <summary>
        /// 定长字符串
        /// </summary>
        Char,
        NChar,
        /// <summary>
        /// 变长字符串
        /// </summary>
        VarChar,
        NVarChar,
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
