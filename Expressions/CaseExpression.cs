using System;
using System.Collections.Generic;
using System.Text;

namespace SharpSoft.Data.Expressions
{
    /// <summary>
    /// case…when表达式
    /// </summary>
    public class CaseExpression : Expression
    {
        public CaseExpression()
        {
            Branches = new List<Branch>();
        }
        public IExpression Input { get; set; }

        public List<Branch> Branches { get; private set; }

        public IExpression ElseBrance { get; set; }
    }

    public class Branch
    {
        public IExpression Value { get; set; }

        public IExpression Result { get; set; }
    }
}
