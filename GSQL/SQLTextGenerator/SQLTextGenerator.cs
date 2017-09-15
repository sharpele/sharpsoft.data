using System;
using System.Collections.Generic;
using System.Text;

namespace SharpSoft.Data.GSQL
{
    using Expressions;
    public abstract class SQLTextGenerator
    {
        #region BASE

        protected virtual string StringQuotation
        {
            get { return "'"; }
        }

        protected virtual string NullText
        {
            get { return "NULL"; }
        }

        protected string SafetiyName(string name)
        {
            return "`" + name + "`";
        }
        /// <summary>
        /// 处理字符串值，将字符串内部的引号替换为两个引号。
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        protected string ProcessStringValue(string value)
        {
            if (value == null)
            {
                return NullText;
            }
            string newvalue = value.Replace(StringQuotation, StringQuotation + StringQuotation);
            return StringQuotation + newvalue + StringQuotation;
        }
        protected string ProcessExpression(IExpression expression)
        {
            string value;
            if (expression == null)
            {
                value = "";
            }
            else if (expression is UnaryExpression)
            {
                value = ProcessExpression((UnaryExpression)expression);
            }
            else if (expression is BinaryExpression)
            {
                value = ProcessExpression((BinaryExpression)expression);
            }
            else if (expression is StringExpression)
            {
                value = ProcessExpression((StringExpression)expression);
            }
            else if (expression is NullExpression)
            {
                value = ProcessExpression((NullExpression)expression);
            }
            else if (expression is NumericExpression)
            {
                value = ProcessExpression((NumericExpression)expression);
            }
            else if (expression is VariableExpression)
            {
                value = ProcessExpression((VariableExpression)expression);
            }
            else if (expression is BracketExpression)
            {
                value = ProcessExpression((BracketExpression)expression);
            }
            else if (expression is FunctionExpression)
            {
                value = ProcessExpression((FunctionExpression)expression);
            }
            else if (expression is ListExpression)
            {
                value = ProcessExpression((ListExpression)expression);
            }
            else if (expression is WildcardExpression)
            {
                value = ProcessExpression((WildcardExpression)expression);
            }
            else if (expression is OrderExpression)
            {
                value = ProcessExpression((OrderExpression)expression);
            }
            else if (expression is CaseExpression)
            {
                value = ProcessExpression((CaseExpression)expression);
            }
            else if (expression is ConditionalExpression)
            {
                value = ProcessExpression((ConditionalExpression)expression);
            }
            else
            {
                return expression.ToString();
            }
            return value;
        }

        #endregion

        #region 表达式

        protected virtual string ProcessExpression(UnaryExpression unary)
        {
            return unary.Method + ProcessExpression(unary.Operand);
        }
        protected virtual string ProcessExpression(BinaryExpression bin)
        {
            string method;
            switch (bin.Method)
            {
                case "==":
                    method = "=";

                    break;
                default:
                    method = bin.Method;
                    break;
            }
            return $"{ProcessExpression(bin.Left)} {method} {bin.Right}";
        }
        protected virtual string ProcessExpression(StringExpression str)
        {
            return ProcessStringValue(str.Content);
        }
        protected virtual string ProcessExpression(NullExpression NU)
        {
            return NullText;
        }
        protected virtual string ProcessExpression(NumericExpression NUM)
        {
            return NUM.Content;
        }
        protected virtual string ProcessExpression(BooleanExpression BO)
        {
            return BO.Content ? "TRUE" : "FALSE";
        }
        protected virtual string ProcessExpression(VariableExpression VARI)
        {
            if (VARI.Name.StartsWith("@"))
            {
                return VARI.Name;
            }
            return SafetiyName(VARI.Name);
        }
        protected virtual string ProcessExpression(BracketExpression bracket)
        {
            string temp = "";
            switch (bracket.BeacketType)
            {
                case BeacketType.Unknow:
                    temp = "?{0}?";
                    break;
                case BeacketType.RoundBrackets:
                    temp = "({0})";
                    break;
                case BeacketType.SquareBrackets:
                    temp = "[{0}]";
                    break;
                case BeacketType.CurlyBrace:
                    temp = "{{{0}}}";
                    break;
                default:
                    break;
            }
            return string.Format(temp, bracket.Inner == null ? "" : ProcessExpression(bracket.Inner));
        }
        protected virtual string ProcessExpression(FunctionExpression func)
        {
            return $"{func.Name}({ProcessExpression(func.Arguments)})";
        }
        protected virtual string ProcessExpression(ListExpression list)
        {
            if (list == null)
            {
                return "";
            }
            List<string> l = new List<string>();
            foreach (var item in list)
            {
                l.Add(ProcessExpression(item));
            }
            return string.Join(",", l.ToArray());

        }
        protected virtual string ProcessExpression(WildcardExpression list)
        {
            return "*";

        }
        protected virtual string ProcessExpression(OrderExpression order)
        {
            return $"{ProcessExpression(order.Expression)} {order.OrderType}";
        }
        protected virtual string ProcessExpression(CaseExpression cas)
        {
            StringBuilder sb = new StringBuilder("CASE ");
            if (cas.Input == null)
            {
                sb.Append(ProcessExpression(cas.Input));
            }
            foreach (var item in cas.Branches)
            {
                sb.Append(" WHEN ")
                    .Append(ProcessExpression(item.Value))
                    .Append(" THEN ")
                    .Append(ProcessExpression(item.Result));

            }
            if (cas.ElseBrance != null)
            {
                sb.Append(" ELSE THEN ").Append(ProcessExpression(cas.ElseBrance));
            }
            sb.Append(" END");
            return sb.ToString();
        }
        protected virtual string ProcessExpression(ConditionalExpression con)
        {

            CaseExpression cas = new CaseExpression();
            cas.Input = con.Condition;
            cas.Branches.Add(
                    new Branch()
                    {
                        Value = new BooleanExpression() { Content = true },
                        Result = con.TrueValue
                    }
                );
            cas.ElseBrance = con.FalseValue;
            return ProcessExpression(cas);
        }
        #endregion

        #region 语句

        public string ProcessStatement(IStatement statement)
        {
            String text;
            if (statement == null)
            {
                text = "";
            }
            else if (statement is SelectStatement)
            {
                text = ProcessStatement((SelectStatement)statement);
            }
            else if (statement is UpdateStatement)
            {
                text = ProcessStatement((UpdateStatement)statement);
            }
            else if (statement is InsertStatement)
            {
                text = ProcessStatement((InsertStatement)statement);
            }
            else if (statement is DeleteStatement)
            {
                text = ProcessStatement((DeleteStatement)statement);
            }
            else if (statement is CreateTableStatement)
            {
                text = ProcessStatement((CreateTableStatement)statement);
            }
            else
            {
                throw new Exception("无法识别的SQL语句类型：" + statement.GetType().FullName);
            }
            return text;

        }

        protected virtual string ProcessStatement(SelectStatement statement)
        {
            StringBuilder sb = new StringBuilder("SELECT ")
                        .Append(ProcessExpression(statement.Columns)).Append(" ");
            if (statement.Tables != null && statement.Tables.HasItem)
            {
                sb.Append("FROM ");
                sb.Append(ProcessExpression(statement.Tables)).Append(" ");
            }
            if (statement.Where != null)
            {
                sb.Append("WHERE ");
                sb.Append(ProcessExpression(statement.Where)).Append(" ");
            }
            if (statement.GroupBy != null)
            {
                sb.Append("GROUP BY ");
                sb.Append(ProcessExpression(statement.GroupBy)).Append(" ");
            }
            if (statement.Having != null)
            {
                sb.Append("HAVING ");
                sb.Append(ProcessExpression(statement.Having)).Append(" ");
            }
            if (statement.OrderBy != null)
            {
                sb.Append("ORDER BY ");
                sb.Append(ProcessExpression(statement.OrderBy)).Append(" ");
            }
            if (statement.Limit != null)
            {
                sb.Append("LIMIT ");
                sb.Append(statement.Limit.ToString()).Append(" ");
            }
            return sb.ToString();
        }

        protected virtual string ProcessStatement(UpdateStatement statement)
        {
            StringBuilder sb = new StringBuilder("UPDATE ")
                           .Append(ProcessExpression(statement.Tables)).Append(" ");
            if (statement.Updates != null)
            {
                sb.Append("SET ");
                sb.Append(ProcessExpression(statement.Updates)).Append(" ");
            }
            if (statement.Where != null)
            {
                sb.Append("WHERE ");
                sb.Append(ProcessExpression(statement.Where)).Append(" ");
            }
            if (statement.OrderBy != null)
            {
                sb.Append("ORDER BY ");
                sb.Append(ProcessExpression(statement.OrderBy)).Append(" ");
            }
            if (statement.Limit != null)
            {
                sb.Append("LIMIT ");
                sb.Append(statement.Limit.ToString()).Append(" ");
            }
            return sb.ToString();

        }

        protected virtual string ProcessStatement(DeleteStatement statement)
        {
            StringBuilder sb = new StringBuilder("DELETE FROM ")
                            .Append(ProcessExpression(statement.Table)).Append(" ");
            if (statement.Where != null)
            {
                sb.Append("WHERE ");
                sb.Append(ProcessExpression(statement.Where)).Append(" ");
            }
            if (statement.OrderBy != null)
            {
                sb.Append("ORDER BY ");
                sb.Append(ProcessExpression(statement.OrderBy)).Append(" ");
            }
            if (statement.Limit != null)
            {
                sb.Append("LIMIT ");
                sb.Append(statement.Limit.ToString()).Append(" ");
            }
            return sb.ToString();

        }

        protected virtual string ProcessStatement(InsertStatement statement)
        {
            StringBuilder sb = new StringBuilder("INSERT INTO ")
                              .Append(ProcessExpression(statement.Table)).Append(" ");
            if (statement.Columns != null)
            {
                sb.Append(ProcessExpression(statement.Columns)).Append(" ");
            }
            if (statement.Values != null)
            {
                sb.Append("VALUES ");
                sb.Append(ProcessExpression(statement.Values)).Append(" ");
            }
            return sb.ToString();

        }
        protected virtual string ProcessStatement(CreateTableStatement statement)
        {


            StringBuilder sb = new StringBuilder("CREATE TABLE ");
            if (statement.IfNotExists)
            {
                sb.Append("IF NOT EXISTS ");
            }
            if (statement.Table != null)
            {
                sb.Append(ProcessExpression(statement.Table)).Append(" ");
            }
            sb.Append("(");
            List<string> list = new List<string>();
            foreach (var item in statement.ColumnDefines)
            {
                list.Add(ProcessColumnDefine(item));
            }
            sb.Append(string.Join(",", list.ToArray()));
            sb.Append(")");
            return sb.ToString();
        }


        protected virtual string ProcessColumnDefine(IColumnDefine columnDefine)
        {
            if (columnDefine is ColumnDefine cd1)
            {
                return ProcessColumnDefine(cd1);
            }
            else if (columnDefine is ConstraintDefine cd2)
            {
                return ProcessConstraintDefine(cd2);
            }
            else
            {
                throw new Exception($"未实现类型{columnDefine.GetType().FullName}的处理。");
            }

        }
        protected virtual string ProcessColumnDefine(ColumnDefine columnDefine)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(ProcessExpression(columnDefine.Name)).Append(" ")
                .Append(ProcessDataType(columnDefine.Type));
            if (columnDefine.TypeDescriptor != null && columnDefine.TypeDescriptor.HasItem)
            {
                sb.Append($"({ProcessExpression(columnDefine.TypeDescriptor)})");
            }
            sb.Append(" ");
            if (columnDefine.IsPrimaryKey)
            {
                sb.Append("PRIMARY KEY ");
            }
            if (columnDefine.Autoincrement)
            {
                sb.Append(ColumnAutoincrement()).Append(" ");
            }
            if (columnDefine.IsUnique)
            {
                sb.Append("UNIQUE").Append(" ");
            }
            if (columnDefine.Nullable)
            {
                sb.Append("NULL").Append(" ");
            }
            else
            {
                sb.Append("NOT NULL").Append(" ");
            }
            if (columnDefine.DefaultValue != null)
            {
                sb.Append("DEFAULT ")
                    .Append(ProcessExpression(columnDefine.DefaultValue));
            }
            if (columnDefine.Comment != null)
            {
                sb.Append("COMMENT ")
                    .Append(ProcessExpression(columnDefine.Comment));
            }


            return sb.ToString();
        }
        protected virtual string ProcessConstraintDefine(ConstraintDefine constraintDefine)
        {
            StringBuilder sb = new StringBuilder("CONSTRAINT ");
            if (constraintDefine.Name != null)
            {
                sb.Append(ProcessExpression(constraintDefine.Name))
                    .Append(" ");
            }
            sb.Append(ProcessConstraintType(constraintDefine.Type));
            sb.Append("(");
            List<string> list = new List<string>();
            foreach (var item in constraintDefine.Columns)
            {
                list.Add(ProcessExpression(item));
            }
            sb.Append(string.Join(",", list.ToArray()));
            sb.Append(")");
            if (constraintDefine.References != null)
            {
                sb.Append("References ");
                sb.Append(ProcessExpression(constraintDefine.References.TableName));
                sb.Append("(");
                list = new List<string>();
                foreach (var item in constraintDefine.References.Columns)
                {
                    list.Add(ProcessExpression(item));
                }
                sb.Append(string.Join(",", list.ToArray()));
                sb.Append(")");
            }

            return sb.ToString();
        }

        protected virtual string ProcessConstraintType(ConstraintType ct)
        {
            string type = "";
            switch (ct)
            {
                case ConstraintType.Unique:
                    type = "UNIQUE";
                    break;
                case ConstraintType.PrimaryKey:
                    type = "PRIMARY KEY";
                    break;
                case ConstraintType.ForeignKey:
                    type = "Foreign Key";
                    break;
                default:
                    break;
            }
            return type;
        }

        protected virtual string ProcessDataType(DataType dataType)
        {
            return Enum.GetName(typeof(DataType), dataType).ToLower();
        }

        protected virtual string ColumnAutoincrement()
        {
            return "AUTO_INCREMENT";
        }
        #endregion
    }
}
