using System;
using System.Collections.Generic;
using System.Text;

namespace SharpSoft.Data
{
    /// <summary>
    /// 为目标数据库提供一组通用的方式来生成TSQL指令
    /// </summary>
    public abstract class TSqlSetting
    {
        #region 函数统一
        public string function(Function func)
        {
            switch (func.Name.ToLower())
            {
                case "get_last_insert_id":
                    return get_last_insert_id();
                case "getdate":
                    return getdate();
                default:
                    break;
            }

            return null;
        }
        /// <summary>
        /// 在派生类中实现，提供数据库获取最近一次插入的自增id
        /// </summary>
        /// <returns></returns>
        public abstract string get_last_insert_id();
        /// <summary>
        /// 在派生类中实现，提供目标数据库获取时间的方法
        /// </summary>
        /// <returns></returns>
        public abstract string getdate();

        #endregion
        /// <summary>
        /// 提供避免与保留字冲突的安全名称
        /// </summary>
        /// <param name="targetname"></param>
        /// <returns></returns>
        public virtual string SafeName(string targetname)
        {
            return string.Concat("[", targetname, "]");
        }
        /// <summary>
        /// 获取对象名称
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public virtual string Target(Target target)
        {
            List<string> l = new List<string>();
            foreach (var item in target.NameParts)
            {
                l.Add(SafeName(item));
            }
            return string.Join(".", l.ToArray());
        }
        /// <summary>
        /// 获取带别名的对象名称
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public virtual string TargetWithAlias(TargetWithAlias target)
        {
            return $"{Target(target)}{(string.IsNullOrEmpty(target.Alias) ? "" : " AS " + target.Alias)}";
        }

        public virtual string QueryField(QueryField queryfield)
        {
            return Expression(queryfield.Value) + (string.IsNullOrEmpty(queryfield.Alias) ? "" : " AS " + queryfield.Alias);
        }

        protected virtual string Join(JoinClause join)
        {
            string jt = "";
            switch (join.Type)
            {
                case JoinType.Inner:
                    jt = "INNER JOIN";
                    break;
                case JoinType.Left:
                    jt = "LEFT JOIN";
                    break;
                case JoinType.Right:
                    jt = "RIGHT JOIN";
                    break;
                case JoinType.Full:
                    jt = "FULL JOIN";
                    break;
                default:
                    break;
            }
            return jt + " " + Expression(join.Table) + " ON " + Expression(join.On.Condition);
        }
        public virtual string Generate(IStatement[] statements)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var item in statements)
            {
                if (item is Select sel)
                {
                    sb.Append(Select(sel));
                }
                else if (item is Insert insert)
                {
                    sb.Append(Insert(insert));
                }
                else if (item is Update update)
                {
                    sb.Append(Update(update));
                }
                else if (item is Delete delete)
                {
                    sb.Append(Delete(delete));
                }
                else if (item is Declare dec)
                {
                    sb.Append(Declare(dec));
                }
                else if (item is If f)
                {
                    sb.Append(If(f));
                }

                sb.AppendLine(";");
            }
            return sb.ToString();
        }
        public virtual string Select(Select sel)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("SELECT ");
            List<string> l = new List<string>();
            foreach (var item in sel.Fields)
            {
                l.Add(QueryField(item));
            }
            sb.Append(string.Join(",", l.ToArray()));
            if (sel.From != null)
            {
                sb.Append(" FROM ");
                l.Clear();
                foreach (var item in sel.From)
                {
                    l.Add(Expression(item));
                }
                sb.Append(string.Join(",", l.ToArray()));
            }
            l.Clear();
            if (sel.Joins != null && sel.Joins.Count > 0)
            {
                sb.Append(" ");
                foreach (var item in sel.Joins)
                {
                    l.Add(Join(item));
                }
                sb.Append(string.Join(" ", l.ToArray()));
            }
            if (sel.Where != null)
            {
                sb.Append(" WHERE ");
                sb.Append(Expression(sel.Where.Condition));
            }

            l.Clear();
            if (sel.GroupBy != null && sel.GroupBy.GroupFields.Count > 0)
            {
                sb.Append(" GROUP BY ");
                foreach (var item in sel.GroupBy.GroupFields)
                {
                    l.Add(Target(item));
                }
                sb.Append(string.Join(",", l.ToArray()));
            }
            l.Clear();
            if (sel.OrderBy != null && sel.OrderBy.OrderFields.Count > 0)
            {
                sb.Append(" ORDER BY ");
                foreach (var item in sel.OrderBy.OrderFields)
                {
                    string orderbytype = "";
                    switch (item.Type)
                    {
                        case OrderByType.Asc:
                            orderbytype = "asc";
                            break;
                        case OrderByType.Desc:
                            orderbytype = "desc";
                            break;
                    }
                    l.Add(Target(item.Field) + " " + orderbytype);
                }
                sb.Append(string.Join(",", l.ToArray()));
            }
            l.Clear();
            if (sel.Having != null)
            {
                sb.Append(" HAVING ");
                sb.Append(Expression(sel.Having.Condition));
            }
            if (sel.Limit != null)
            {

            }
            if (sel.Unions != null)
            {
                sb.Append(" UNION ");
                if (sel.Unions.UnionAll)
                {
                    sb.Append("ALL ");
                }
                sb.Append(Select(sel.Unions.OtherSelect));
            }


            return sb.ToString();
        }
        /// <summary>
        /// 获取limit子句
        /// </summary>
        /// <param name="limit"></param>
        /// <returns></returns>
        public virtual string Limit(LimitClause limit)
        {
            if (limit == null)
            {
                return "";
            }
            return $"LIMIT {limit.Length} OFFSET {limit.Offset}";
        }
        public virtual string Insert(Insert insert)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("INSERT INTO ");
            sb.Append(Target(insert.Table));
            sb.Append(" ");
            List<string> l = new List<string>();
            if (insert.Fields != null && insert.Fields.Count > 0)
            {
                foreach (var item in insert.Fields)
                {
                    l.Add(Target(item));
                }
                sb.Append("(").Append(string.Join(",", l.ToArray())).Append(")");
            }
            l.Clear();
            sb.Append("VALUES(");
            foreach (var item in insert.Values)
            {
                l.Add(Expression(item));
            }
            sb.Append(string.Join(",", l.ToArray()))
                .Append(")");
            return sb.ToString();
        }
        protected virtual string UpdatePair(UpdatePair pair)
        {
            return $"{Target(pair.Field)} = {Expression(pair.Value)}";
        }
        public virtual string Update(Update update)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("UPDATE ");
            sb.Append(Target(update.Table));
            sb.Append(" SET ");
            List<string> l = new List<string>();
            foreach (var item in update.Updates)
            {
                l.Add(UpdatePair(item));
            }
            sb.Append(string.Join(",", l.ToArray()));
            if (update.Where != null)
            {
                sb.Append(" WHERE ");
                sb.Append(Expression(update.Where.Condition));
            }
            return sb.ToString();
        }

        public virtual string Delete(Delete delete)
        {

            StringBuilder sb = new StringBuilder();
            sb.Append("DELETE ");
            List<string> l = new List<string>();
            foreach (var item in delete.Fields)
            {
                l.Add(QueryField(item));
            }
            sb.Append(string.Join(",", l.ToArray()));
            sb.Append(" FROM ");
            l.Clear();
            foreach (var item in delete.From)
            {
                l.Add(Expression(item));
            }
            sb.Append(string.Join(",", l.ToArray()));
            l.Clear();
            if (delete.Joins != null && delete.Joins.Count > 0)
            {
                sb.Append(" ");
                foreach (var item in delete.Joins)
                {
                    l.Add(Join(item));
                }
                sb.Append(string.Join(" ", l.ToArray()));
            }
            if (delete.Where != null)
            {
                sb.Append(" WHERE ");
                sb.Append(Expression(delete.Where.Condition));
            }

            l.Clear();
            return sb.ToString();
        }

        public virtual string Declare(Declare dec)
        {
            return $"DECLARE {dec.VarName} {GetDataType(dec.DataType)}";
        }
        public virtual string If(If f)
        {
            StringBuilder sb = new StringBuilder();
            bool first = true;
            foreach (var item in f.Branchs)
            {
                sb.AppendLine($"{(first ? "" : " ELSE ")}IF({Expression(item.Condition)})");
                sb.AppendLine("BEGIN");
                sb.AppendLine(Generate(item.Block));
                sb.AppendLine("END");
            }
            if (f.Else != null)
            {
                sb.AppendLine("ELSE");
                sb.AppendLine("BEGIN");
                sb.AppendLine(Generate(f.Else));
                sb.AppendLine("END");
            }
            return sb.ToString();
        }
        #region 类型
        /// <summary>
        /// 在派生类中实现，获取通用数据类型在目标数据库中的实现
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public abstract string GetDataType(SqlDataTyoe t);
        #endregion
        #region 常量

        protected virtual string String(string str)
        {
            return string.Concat("'", str, "'");
        }
        protected virtual string Numeric(string numeric)
        {
            return numeric;
        }
        protected virtual string Boolean(IBoolean boolean)
        {
            if (boolean is True)
            {
                return "1";
            }
            else if (boolean is False)
            {
                return "0";
            }
            else
            {
                throw new Exception("不可识别的bool类型。");
            }

        }

        protected virtual string Null()
        {
            return "NULL";
        }

        public virtual string Constant(Constant con)
        {
            if (con.Type == ConstantType.String)
            {
                return String(con.Content);
            }
            else if (con.Type == ConstantType.Numeric)
            {
                return Numeric(con.Content);
            }
            else
            {
                throw new Exception("不是有效的常量。");
            }
        }
        #endregion 
        #region 操作符

        protected virtual string IS()
        {
            return "is";
        }

        protected virtual string UnaryOperator(UnaryOperator opr)
        {
            switch (opr)
            {
                case Data.UnaryOperator.Not:
                    return "NOT";
                case Data.UnaryOperator.Tilde:
                    throw new Exception("SQL不支持按位取反");
                case Data.UnaryOperator.Minus:
                    return "-";
                case Data.UnaryOperator.Plus:
                    return "";
                default:
                    throw new Exception("未知的一元操作符。");
            }
        }
        protected virtual string BinaryOperator(BinaryOperator opr)
        {
            switch (opr)
            {
                case Data.BinaryOperator.Plus:
                    return "+";
                case Data.BinaryOperator.Minus:
                    return "-";
                case Data.BinaryOperator.Multiply:
                    return "*";
                case Data.BinaryOperator.Divide:
                    return "/";
                case Data.BinaryOperator.Mod:
                    return "%";
                case Data.BinaryOperator.Equals:
                    return "=";
                case Data.BinaryOperator.NotEquals:
                    return "!=";
                case Data.BinaryOperator.GreaterThan:
                    return ">";
                case Data.BinaryOperator.GreaterEquals:
                    return ">=";
                case Data.BinaryOperator.LessThan:
                    return "<";
                case Data.BinaryOperator.LessEquals:
                    return "<=";
                case Data.BinaryOperator.And:
                    return "AND";
                case Data.BinaryOperator.Or:
                    return "OR";
                case Data.BinaryOperator.AndAlso:
                    return "AND";
                case Data.BinaryOperator.OrAlso:
                    return "OR";
                case Data.BinaryOperator.In:
                    return "IN";
                case Data.BinaryOperator.Like:
                    return "LIKE";
                case Data.BinaryOperator.Is:
                    return "IS";
                case Data.BinaryOperator.Dot:
                    return ".";
                case Data.BinaryOperator.None:
                default:
                    throw new Exception("不支持的二元操作符。");
            }
        }

        #endregion
        #region 表达式
        protected virtual string UnaryExpression(UnaryExpression unexp)
        {
            var s = !false;
            string exp = "";
            exp = Expression(unexp.Value);
            return UnaryOperator(unexp.Operator) + exp;
        }
        protected virtual string BinaryExpression(BinaryExpression binexp)
        {
            string str = string.Concat(Expression(binexp.Left), "", BinaryOperator(binexp.Operator), "", Expression(binexp.Right));
            if (binexp.Alone && !(binexp.Operator == Data.BinaryOperator.Dot))
            {
                return string.Concat("(", str, ")");
            }
            else
            {
                return str;
            }
        }

        protected virtual string SwitchExpression(SwitchExpression swexp)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("CASE ");
            sb.Append(Expression(swexp.Condition));
            foreach (var item in swexp.ValueResults)
            {
                sb.Append($" WHEN {Expression(item.Key)} THEN {Expression(item.Value)}");
            }
            if (swexp.Default != null)
            {
                sb.Append(" ELSE ").Append(Expression(swexp.Default));
            }
            sb.Append(" END");
            return sb.ToString();
        }


        protected virtual string ExpressionList(ExpressionList explst)
        {
            List<string> l = new List<string>();
            foreach (var item in explst.GetList())
            {
                l.Add(Expression(item));
            }
            string str = string.Join(",", l.ToArray());
            return string.Concat("(", str, ")");
        }

        protected virtual string AsExpression(AsExpression asexp)
        {
            var exp = Expression(asexp.Value);
            if (string.IsNullOrEmpty(asexp.As))
            {
                return exp;
            }
            else
            {
                return exp + " AS " + asexp.As;
            }
        }
        protected virtual string Function(Function func)
        {
            return $"{func.Name}{ExpressionList(func.Arguments)}";
        }
        public virtual string Expression(IValue value)
        {
            if (value is IExpression exp)
            {
                if (value is AssignExpression assexp)
                {
                    return Expression(assexp.Left) + " = " + Expression(assexp.Right);
                }
                else if (value is UnaryExpression unexp)
                {
                    return UnaryExpression(unexp);
                }
                else if (value is BinaryExpression binexp)
                {
                    return BinaryExpression(binexp);
                }
                else if (value is SwitchExpression swexp)
                {
                    return SwitchExpression(swexp);
                }
                else if (value is ExpressionList explst)
                {
                    return ExpressionList(explst);
                }
                else if (value is AsExpression asexp)
                {
                    return AsExpression(asexp);
                }
                else if (value is WildcardExpression wild)
                {
                    if (wild.Table == null)
                    {
                        return "*";
                    }
                    return $"{SafeName(wild.Table.Name)}.*";

                }
                else
                {
                    throw new Exception("不支持的表达式类型。" + exp.GetType().ToString());
                }
            }
            else if (value is Constant con)
            {
                return Constant(con);
            }
            else if (value is IBoolean boolean)
            {
                return Boolean(boolean);
            }
            else if (value is Null)
            {
                return Null();
            }
            else if (value is Variable vari)
            {
                if (vari.Name.StartsWith("@"))
                {
                    return vari.Name;
                }
                else
                {
                    return SafeName(vari.Name);
                }
            }
            else if (value is TargetWithAlias twa)
            {
                return TargetWithAlias(twa);
            }
            else if (value is Select sel)
            {//表达式内部的子查询
                return "(" + Select(sel) + ")";
            }
            else if (value is Function func)
            {
                return Function(func);
            }
            else
            {
                throw new Exception("不支持的表达式类型。" + value.GetType().ToString());
            }
        }
        #endregion
    }
}
