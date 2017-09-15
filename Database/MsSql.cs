using System;
using System.Collections.Generic;
using System.Text;

namespace SharpSoft.Data
{
    using System.Data.SqlClient;
    using SharpSoft.Data.GSQL;
    using System.ComponentModel;
    /// <summary>
    /// SqlServer数据库
    /// </summary>
    [DisplayName("Sql Server数据库")]
    public class MsSql : Database
    {
        public MsSql(string connstr) : base(SqlClientFactory.Instance, connstr)
        {
        }

        public override Database Clone()
        {
            return new MsSql(this.ConnectionString);
        }

        public static string CreateConnectionString(string server, int port, string defdb, string uid, string pwd)
        {
            return $"Data Source={server}{(port <= 0 ? "" : "," + port.ToString())};Initial Catalog={defdb};User ID={uid};Password={pwd};";
        }

        protected override SQLTextGenerator SQLTextGenerator()
        {
            return new MySqlTextGenerator();
        }

        //public override SqlSettingBase Setting()
        //{
        //    return   MsSqlSetting.Default;
        //}
    }
}
