
using System;
using System.Collections.Generic;
using System.Text;

namespace SharpSoft.Data
{
    using SharpSoft.Data.Expressions;
    using SharpSoft.Data.GSQL;
    public class MySqlTextGenerator : SQLTextGenerator
    {
        protected override string ProcessExpression(BinaryExpression bin)
        {
            return base.ProcessExpression(bin);
        }
    }
}
