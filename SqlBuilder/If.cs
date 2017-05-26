using System;
using System.Collections.Generic;
using System.Text;

namespace SharpSoft.Data
{
    public class If : IStatement
    {
        public List<ElseIfClause> Branchs { get; set; }
        public IStatement[] Else { get; set; }
    }

    public class ElseIfClause
    {
        public IValue Condition { get; set; }

        public IStatement[] Block { get; set; }
    }
}
