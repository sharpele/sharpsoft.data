using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;

namespace SharpSoft.Data
{
    /// <summary>
    /// SqlServer数据库
    /// </summary>
    public class MsSql : Database
    {
        public MsSql(string connstr) : base(SqlClientFactory.Instance, connstr)
        {
        }

        public override TSqlSetting Setting()
        {
            return new MsSqlSetting();
        }
    }
}
