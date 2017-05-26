using System;
using System.Collections.Generic;
using System.Text;

namespace SharpSoft.Data
{
    public class Update:IStatement
    {
        public Target Table { get; set; }
        public List<UpdatePair> Updates { get; set; }

        public WhereClause Where { get; set; }
    }

    public class UpdatePair
    {
        public Target Field { get; set; }

        public IValue Value { get; set; }
    }
}
