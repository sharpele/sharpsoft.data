
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;

namespace SharpSoft.Data
{
#if NETSTANDARD2_0
    using Microsoft.Data.Sqlite;
    using SharpSoft.Data.GSQL;
    using System.ComponentModel;

    [DisplayName("Sqlite数据库")]
    public class Sqlite : Database
    {
        public Sqlite(string p_connstr) : base(SqliteFactory.Instance, p_connstr)
        {

        }

        public override Database Clone()
        {
            return new Sqlite(this.ConnectionString);
        }

        public static string CreateConnectionString(string server, int port, string defdb, string uid, string pwd)
        {
            return $"Data Source={server}{(port <= 0 ? "" : "," + port.ToString())};Initial Catalog={defdb};User ID={uid};Password={pwd};";
        }

        protected override SQLTextGenerator SQLTextGenerator()
        {
            throw new NotImplementedException();
        }
    }
#endif 
}
