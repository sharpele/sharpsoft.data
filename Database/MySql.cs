using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;
using MySql.Data.Common;
using MySql.Data.MySqlClient;
using SharpSoft.Data.GSQL;
using System.ComponentModel.DataAnnotations;

namespace SharpSoft.Data
{
    using System.ComponentModel;
    [ DisplayName("Mysql数据库")]
    public class MySql : Database
    {
        public MySql(string p_connstr) : base(MySqlClientFactory.Instance, p_connstr)
        {
            MySqlCommand cmd = new MySqlCommand();
        }

        public override Database Clone()
        {
            return new MySql(this.ConnectionString);
        }

        public static string CreateConnectionString(string server, int port, string defdb, string uid, string pwd)
        {
            return $"Server={server};Port={(port <= 0 ? 3306 : port)};Database={defdb};Uid={uid};Pwd={pwd};";
        }

        protected override SQLTextGenerator SQLTextGenerator()
        {
            return new MySqlTextGenerator();
        }
    }
}
