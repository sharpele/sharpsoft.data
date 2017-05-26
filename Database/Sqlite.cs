using System;
using System.Collections.Generic;
using System.Data.Common;

using System.Text;

namespace SharpSoft.Data
{
    public class Sqlite : Database
    {
        public Sqlite(string p_connstr) : base(null, p_connstr)
        {
            //System.Data.sql
        }

        public override TSqlSetting Setting()
        {
            throw new NotImplementedException();
        }
    }
}
