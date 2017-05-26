using System;
using System.Collections.Generic;
using System.Text;

namespace SharpSoft.Data
{
    public class Delete:IStatement
    {
        public QueryFieldList Fields { get; set; }

        public List<IValue> From { get; set; }

        public List<JoinClause> Joins { get; set; }
        public WhereClause Where { get; set; }
    }
}
