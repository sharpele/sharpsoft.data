using SharpSoft.Data.Lexing;
using System;
using System.Collections.Generic;
using System.Text;

namespace SharpSoft.Data
{
    public class TSQLParser : ExpressionParser
    {
        public TSQLParser(string sql) : base(sql)
        {
        }

        protected override LexerSetting OnLexerSetting(LexerSetting basesetting)
        {
            LexerSetting setting = LexerSetting.Default;
            setting.IgnoreCase = true;//词法解析中忽略大小写
            setting.InlineCommentsStartSign = "/*";//行内注释起始符
            setting.InlineCommentsEndSign = "*/"; //行内注释终止符
            setting.OutlineCommentsSign = "--";//行外注释
            setting.LiteralFirstChars = new char[] { '@' };//使用@开头的为模板参数
            setting.CustomOperators = new string[] { "not", "and", "or", "in", "like", "is" };
            setting.Keywords = new string[] {"var","declare","if","else", "switch", "default", "true", "false" , "null",
                "select", "from", "where", "insert", "into", "values", "update", "set", "delete",
                "join","on","left","right","full","inner","having","asc","desc", "as", "group", "order","by", "union","all", "limit", "offset" };
            return setting;
        }
        #region 表达式扩展

        private AsExpression ReadAsExpression(IValue last)
        {
            var t = cur;
            if (matchToken(t, TokenType.Keyword, "as"))
            {//as表达式
                Next();
            }
            else
            {
                ex("as表达式语法错误。");
                return null;
            }
            t = cur;
            if (last != null && t.Type == TokenType.Literal || t.Type == TokenType.Keyword)
            {
                Next();
                return new AsExpression() { Value = last, As = t.Content };
            }
            else
            {
                ex("as表达式语法错误。");
                return null;
            }
        }
        private AssignExpression ReadAssignExpression(IValue last)
        {
            var t = cur;
            if (matchToken(t, TokenType.Operator, "="))
            {//赋值表达式
                Next();
            }
            else
            {
                ex("赋值表达式语法错误。");
                return null;
            }
            var value = ReadExpression(false);
            return new AssignExpression() { Left = last, Right = value };
        }
        private WildcardExpression ReadWildcardExpression(IValue last)
        {
            if (last is Variable vari)
            {
                if (matchToken(cur, TokenType.Operator, "."))
                {
                    Next();
                    if (matchToken(cur, TokenType.Operator, "*"))
                    {
                        Next();
                        return new WildcardExpression() { Table = vari };
                    }
                    else
                    {
                        ex("当前不是有效的通配符。");
                    }
                }
            }
            else
            {
                ex("在通配符*之前必须是一个有效的表名。");
            }
            return null;

        }
        private SwitchExpression ReadSwitchExpression()
        {
            Next();
            SwitchExpression sw = new SwitchExpression();
            var value = ReadExpression();
            if (value == null)
            {
                ex("必须为switch表达式指定一个条件。");
            }
            sw.Condition = value;
            IValue readvalue()
            {
                if (matchToken(cur, TokenType.Colon))
                {
                    Next();
                    return ReadExpression(false);//不考虑逗号读取一个表达式
                }
                else
                {
                    ex("switch表达式中必须用冒号来指定该条件下提供的值。");
                    return null;
                }
            }
            if (matchToken(cur, TokenType.CurlyBracket, "{"))
            {
                Next();
                sw.ValueResults = new Dictionary<IValue, IValue>();
                while (true)
                {
                    SkipComments();
                    if (matchToken(cur, TokenType.Keyword, "default"))
                    {//默认值
                        Next();
                        if (sw.Default != null)
                        {
                            ex("switch表达式指定了多个默认值。");
                        }
                        sw.Default = readvalue();
                    }
                    else
                    {
                        var con = ReadExpression();
                        var val = readvalue();
                        sw.ValueResults.Add(con, val);
                    }
                    SkipComments();
                    if (!matchToken(cur, TokenType.Comma))
                    {//读取完一条后没有出现逗号表示switch表达式已结束
                        break;
                    }
                    else
                    {
                        Next();//忽略用于分隔的逗号
                    }
                }
                if (!matchToken(cur, TokenType.CurlyBracket, "}"))
                {
                    ex("switch表达式没有指定块结束标记：'}'");
                }
                else
                {
                    Next();
                }
            }
            else
            {
                ex("switch表达式必须用花括弧来指定一组对应的值。");
            }
            return sw;
        }
        protected override IValue OnReadExpressionPart(IValue lastpart)
        {
            var t = cur;
            if (matchToken(t, TokenType.Keyword, "select") && matchToken( Preview(-1), TokenType.Parenthese,"("))
            {//允许表达式中的内容为select语句,表达式中的select语句之前必须是圆括弧
                return ReadSelect();
            }
            else if (matchToken(t, TokenType.Keyword, "switch"))
            {
                return ReadSwitchExpression();
            }
            else if (matchToken(t, TokenType.Keyword, "as"))
            {//as表达式，必须带有as关键字，不能像sqlserver那样省略as关键字，这会导致表达式词法解析异常。
                return ReadAsExpression(lastpart);
            }
            else if (matchToken(t, TokenType.Operator, "="))
            {//赋值表达式
                return ReadAssignExpression(lastpart);
            }
            else if (matchToken(t, TokenType.Operator, ".") && matchToken(Preview(1), TokenType.Operator, "*"))
            {//所有列通配符，指定表名
                return ReadWildcardExpression(lastpart);
            }
            else if (lastpart == null && (matchToken(Preview(-1), TokenType.Keyword, "select"))
                && matchToken(t, TokenType.Operator, "*") && matchToken(Preview(1), TokenType.Keyword, "from"))
            {//所有列通配符,单独的星号
                Next();
                return new WildcardExpression() { Table = null };
            }

            else
            {
                return base.OnReadExpressionPart(lastpart);
            }
        }

        public static Target ExpressionToTargetName(IValue exp)
        {
            if (exp is Variable vari)
            {
                return new Target() { NameParts = new List<string>() { vari.Name } };
            }
            else if (exp is BinaryExpression bina)
            {
                if (bina.Operator != BinaryOperator.Dot)
                {
                    throw new Exception("无效的对象名称。");
                }
                List<string> list = new List<string>();
                list.AddRange(ExpressionToTargetName(bina.Left).NameParts);
                list.AddRange(ExpressionToTargetName(bina.Right).NameParts);
                return new Target() { NameParts = list };
            }
            else
            {
                throw new Exception("无效的对象名称。");
            }
        }
        public static IValue ExpressionToResultSet(IValue value)
        {//将表达式转换为结果集
            string alias = null;
            if (value is AsExpression ase)
            {
                alias = ase.As;
                value = ase.Value;
            }
            if (value is BinaryExpression exp)
            {
                if (exp.Operator != BinaryOperator.Dot)
                {
                    throw new Exception("必须为小数点。");
                }
                if (!(exp.Left is Variable left && exp.Right is Variable right))
                {
                    throw new Exception("必须为标识符。");
                }
                List<string> l = new List<string>();
                l.Add(left.Name);
                l.Add(right.Name);
                return new TargetWithAlias() { Alias = alias, NameParts = l };
            }
            else if (value is Select sel)
            {
                return new SubSelect(sel, alias);
            }
            else
            {
                throw new Exception("该表达式不是有效的结果集对象。");
            }
        }
        #endregion
        #region 通用

        public List<IStatement> ReadStatements()
        {
            List<IStatement> l = new List<IStatement>();
            IStatement sta = null;
            do
            {
                sta = ReadStatement();
                if (sta != null)
                {
                    l.Add(sta);
                }
            } while (sta != null);
            return l;
        }
        /// <summary>
        /// 读取一条SQL语句
        /// </summary>
        /// <returns></returns>
        public IStatement ReadStatement()
        {
            SkipComments();
            Token t = cur;
            IStatement statement = null;
            if (matchToken(t, TokenType.Keyword, "select"))
            {
                statement = ReadSelect();
            }
            else if (matchToken(t, TokenType.Keyword, "insert"))
            {
                statement = ReadInsert();
            }
            else if (matchToken(t, TokenType.Keyword, "update"))
            {
                statement = ReadUpdate();
            }
            else if (matchToken(t, TokenType.Keyword, "delete"))
            {
                statement = ReadDelete();
            }
            else if (matchToken(t, TokenType.Keyword, "declare"))
            {
                statement = ReadDeclare();
            }
            else if (matchToken(t, TokenType.Keyword, "if"))
            {
                statement = ReadIf();
            }



            else if (t == null || matchToken(t, TokenType.End) || matchToken(t, TokenType.CurlyBracket, "}"))
            {
                return null;
            }
            else
            {
                ex("异常的标记。");
            }
            SkipComments();
            if (matchToken(cur, TokenType.Semicolon))
            {
                Next();//跳过语句末尾的分号
            }


            return statement;
        }

        /// <summary>
        /// 读取用小数点分隔的目标对象名称
        /// </summary>
        /// <returns></returns>
        protected Target ReadTargetName()
        {
            SkipComments();

            var t = cur;
            List<string> parts = new List<string>();
            while (t != null && t.Type == TokenType.Literal && !t.Content.StartsWith("@"))
            {
                parts.Add(t.Content);
                Next();
                t = cur;
                if (!matchToken(t, TokenType.Operator, "."))
                {
                    break;
                }
                else
                {
                    Next();//跳过名称分隔符"."
                    t = cur;
                }

            }
            if (parts.Count == 0)
            {
                ex("读取对象名称失败，错误的标记。");
            }
            return new Target() { NameParts = parts };
        }
        /// <summary>
        /// 读取目标名称列表，用于group by子句，insert插入的字段列表等不包含表达式和附加信息的地方。
        /// </summary>
        /// <returns></returns>
        protected List<Target> ReadTargetList()
        {
            SkipComments();
            Target target = null;
            List<Target> l = new List<Target>();
            while (true)
            {
                target = ReadTargetName();
                l.Add(target);
                if (!matchToken(cur, TokenType.Comma))
                {
                    break;
                }
                else
                {
                    Next();//跳过逗号
                }
            }
            return l;
        }

        /// <summary>
        /// 读取From子句的内容
        /// </summary>
        /// <returns></returns>+
        protected List<IValue> ReadFrom()
        {
            SkipComments();
            if (!matchToken(cur, TokenType.Keyword, "from"))
            {
                return null;
            }
            Next();//跳过from关键字
            SkipComments();
            List<IValue> list = new List<IValue>(1);
            var exp = ReadExpression();
            if (exp is ExpressionList)
            {
                list.AddRange(((ExpressionList)exp).GetList());
            }
            else
            {
                list.Add(exp);
            }
            return list;
        }
        /// <summary>
        /// 读取Where子句
        /// </summary>
        /// <returns></returns>
        protected WhereClause ReadWhere()
        {
            SkipComments();
            if (matchToken(cur, TokenType.Keyword, "where"))
            {
                Next();//跳过WHERE关键字
                var exp = ReadExpression();
                return new WhereClause() { Condition = exp };
            }
            else
            {
                return null;
            }
        }


        #endregion
        #region 声明
        public Declare ReadDeclare()
        {
            SkipComments();
            Declare dec = new Declare();
            Next();
            var exp = ReadExpression();
            if (exp is Variable vari1)
            {
                dec.VarName = vari1.Name;
            }
            else
            {
                ex("declare语法错误。");
            }
            exp = ReadExpression();
            SqlDataTyoe t = new SqlDataTyoe();
            if (exp is Variable vari)
            {
                t.DataType = SqlDataTyoe.GetDataType(vari.Name);
            }
            else if (exp is Function func)
            {
                t.DataType = SqlDataTyoe.GetDataType(func.Name);
                var paras = func.Arguments?.GetList();
                if (paras != null && paras.Count > 0)
                {
                    try
                    {
                        t.Length1 = int.Parse(((Constant)paras[0]).Content);
                        if (paras.Count > 1)
                        {
                            t.Length2 = int.Parse(((Constant)paras[1]).Content);
                            if (paras.Count > 2)
                            {
                                ex($"类型[{func.Name}]的长度/精度参数错误。");
                            }
                        }
                    }
                    catch (Exception)
                    {
                        ex($"类型[{func.Name}]的长度/精度参数错误。");
                    }

                }
            }
            dec.DataType = t;

            return dec;
        }

        #endregion
        #region 条件判断语句
        public If ReadIf()
        {
            If f = new If();
            f.Branchs = new List<ElseIfClause>();
            var branch = ReadBranch();
            f.Branchs.Add(branch);
            while (matchToken(cur, TokenType.Keyword, "else"))
            {
                Next();
                if (matchToken(cur, TokenType.Keyword, "if"))
                {
                    branch = ReadBranch();
                    f.Branchs.Add(branch);
                }
                else
                {//else block
                    f.Else = ReadOneBlock().ToArray();
                    break;//出现else块后if语句强制结束。
                }

            }
            return f;
        }
        /// <summary>
        /// 读取一个代码块，如果用花括弧包裹则读取花括弧中的多条语句，否则只读取一条语句。
        /// </summary>
        /// <returns></returns>
        protected List<IStatement> ReadOneBlock()
        {
            SkipComments();
            List<IStatement> l = new List<IStatement>();
            if (matchToken(cur, TokenType.CurlyBracket, "{"))
            {
                Next();
                l = ReadStatements();

                if (matchToken(cur, TokenType.CurlyBracket, "}"))
                {
                    Next();
                }
                else
                {
                    ex("未发现合适代码块结束标记:'}'");
                }
            }
            else
            {//没有花括号，只尝试读取一条语句
                var stat = ReadStatement();
                if (stat == null)
                {
                    ex("语法错误。");
                }
                l.Add(stat);
            }
            return l;
        }

        protected ElseIfClause ReadBranch()
        {

            if (matchToken(cur, TokenType.Keyword, "if"))
            {
                Next();
            }
            else
            {
                return null;
            }
            var value = ReadExpression();
            var branch = new ElseIfClause();
            if (value == null)
            {
                ex("语法错误，if语句之后应为逻辑表达式。");
            }
            branch.Condition = value;
            branch.Block = ReadOneBlock().ToArray();

            return branch;
        }
        #endregion
        #region 查询语句

        /// <summary>
        /// 从当前语句开始读取查询语句
        /// </summary>
        /// <returns></returns>
        public Select ReadSelect()
        {
            SkipComments();
            Select sel = new Select();
            Next();//跳过SELECT关键字
            sel.Fields = ReadQueryFieldList(false);
            sel.From = ReadFrom();

            sel.Joins = ReadJoins();
            sel.Where = ReadWhere();
            sel.GroupBy = ReadGroupBy();
            sel.Having = ReadHaving();
            sel.OrderBy = ReadOrderBy();
            sel.Unions = ReadUnion();
            sel.Limit = ReadLimit();
            return sel;
        }
        protected List<JoinClause> ReadJoins()
        {
            List<JoinClause> joins = new List<JoinClause>();
            JoinClause join = null;
            do
            {
                join = ReadJoin();
                if (join != null)
                {
                    joins.Add(join);
                }
            } while (join != null);
            if (joins.Count == 0)
            {
                return null;
            }
            return joins;
        }

        protected Union ReadUnion()
        {
            SkipComments();
            bool isunionall = false;
            if (matchToken(cur, TokenType.Keyword, "union"))
            {
                Next();
            }
            else
            {
                return null;
            }
            if (matchToken(cur, TokenType.Keyword, "all"))
            {
                isunionall = true;
                Next();
            }
            var othersel = ReadSelect();
            return new Union() { OtherSelect = othersel, UnionAll = isunionall };
        }
        private QueryField ExpressionAsQueryField(IValue exp)
        {
            if (exp is AsExpression)
            {
                var ase = (AsExpression)exp;
                return new QueryField() { Value = ase.Value, Alias = ase.As };
            }
            else
            {
                return new QueryField() { Value = exp, Alias = null };
            }
        }
        /// <summary>
        /// 从当前位置开始读取一个查询字段列表
        /// </summary>
        /// <returns></returns>
        protected QueryFieldList ReadQueryFieldList(bool caningro)
        {
            SkipComments();
            QueryFieldList fl = new QueryFieldList();
            if (matchToken(cur, TokenType.Keyword, "from"))
            {
                if (caningro)
                {
                    return null;
                }
                else
                {
                    ex("没有指定查询字段。");
                }
            }

            var value = ReadExpression();
            if (value is ExpressionList)
            {
                foreach (var item in ((ExpressionList)value).GetList())
                {
                    fl.Add(ExpressionAsQueryField(item));
                }
            }
            else
            {
                fl.Add(ExpressionAsQueryField(value));
            }
            return fl;
        }

        /// <summary>
        /// 读取join子句
        /// </summary>
        /// <returns></returns>
        protected JoinClause ReadJoin()
        {
            SkipComments();
            JoinType type = JoinType.Inner;
            foreach (var item in new string[] { "left", "right", "full", "inner" })
            {
                if (matchToken(cur, TokenType.Keyword, item))
                {
                    switch (item)
                    {
                        case "inner":
                            type = JoinType.Inner;
                            break;
                        case "left":
                            type = JoinType.Left;
                            break;
                        case "right":
                            type = JoinType.Right;
                            break;
                        case "full":
                            type = JoinType.Full;
                            break;
                        default:
                            break;
                    }
                    Next();
                    break;
                }
            }
            if (matchToken(cur, TokenType.Keyword, "join"))
            {
                Next();
                IValue value = ReadExpression();
                OnClause on = new OnClause();
                if (matchToken(cur, TokenType.Keyword, "on"))
                {
                    Next();
                    on.Condition = ReadExpression();
                }
                else
                {
                    ex("没有为join指定on子句");
                }
                return new JoinClause() { Type = type, Table = value, On = on };
            }
            else
            {
                return null;
            }
        }

        protected GroupByClause ReadGroupBy()
        {
            SkipComments();
            if (matchToken(cur, TokenType.Keyword, "group"))
            {
                Next();
            }
            else
            {
                return null;
            }
            if (matchToken(cur, TokenType.Keyword, "by"))
            {
                Next();
            }
            else
            {
                ex("group之后未见到关键字：by");
            }
            var tl = ReadTargetList();
            return new GroupByClause() { GroupFields = tl };
        }

        protected OrderByField ReadOrderByField()
        {
            SkipComments();
            var t = ReadTargetName();
            OrderByType type = OrderByType.Asc;
            SkipComments();
            if (matchToken(cur, TokenType.Keyword, "asc"))
            {
                type = OrderByType.Asc;
                Next();
            }
            else if (matchToken(cur, TokenType.Keyword, "desc"))
            {
                type = OrderByType.Desc;
                Next();
            }
            return new OrderByField() { Type = type, Field = t };
        }

        protected HavingClause ReadHaving()
        {
            SkipComments();
            if (matchToken(cur, TokenType.Keyword, "having"))
            {
                Next();
                return new HavingClause() { Condition = ReadExpression() };
            }
            else
            {
                return null;
            }
        }

        protected OrderByClause ReadOrderBy()
        {
            SkipComments();
            if (matchToken(cur, TokenType.Keyword, "order"))
            {
                Next();
            }
            else
            {
                return null;
            }
            if (matchToken(cur, TokenType.Keyword, "by"))
            {
                Next();
            }
            else
            {
                ex("order之后未见到关键字：by");
            }
            List<OrderByField> l = new List<OrderByField>();
            while (true)
            {
                var field = ReadOrderByField();
                l.Add(field);
                if (matchToken(cur, TokenType.Comma))
                {
                    Next();
                }
                else
                {
                    break;
                }
            }
            return new OrderByClause() { OrderFields = l };

        }
        /// <summary>
        /// 读取Limit子句，语法 LIMIT count/LIMIT index,count
        /// </summary>
        protected LimitClause ReadLimit()
        {
            SkipComments();
            if (matchToken(cur, TokenType.Keyword, "limit"))
            {
                Next();
                var value = ReadExpression();
                if (value is Constant con)
                {
                    return new LimitClause() { Length = con.ToInt32(), Offset = 0 };
                }
                else if (value is ExpressionList expl)
                {
                    var exps = expl.GetList();
                    if (exps.Count != 2)
                    {
                        ex("LIMIT子句的参数数目不正确。");
                    }
                    if (exps[0] is Constant con1 && exps[1] is Constant con2)
                    {
                        return new LimitClause() { Length = con2.ToInt32(), Offset = con1.ToInt32() };
                    }
                    else
                    {
                        ex("LIMIT子句的参数解析失败。");
                    }
                }
            }
            return null;
        }

        #endregion

        #region 插入语句
        public Insert ReadInsert()
        {
            SkipComments();
            if (matchToken(cur, TokenType.Keyword, "insert"))
            {
                Next();
            }
            else
            {
                ex("未发现INSERT关键字。");
            }
            if (matchToken(cur, TokenType.Keyword, "into"))
            {
                Next();
            }
            else
            {
                ex("未发现INTO关键字。");
            }
            Insert insert = new Insert();
            insert.Table = ReadTargetName();
            if (matchToken(cur, TokenType.Parenthese, "("))
            {//圆括号，读取字段列表。
                Next();
                insert.Fields = ReadTargetList();
                if (matchToken(cur, TokenType.Parenthese, ")"))
                {
                    Next();
                }
                else
                {
                    ex("在INSERT语句的字段列表处为找到正确的闭括号。");
                }
            }
            if (matchToken(cur, TokenType.Keyword, "values"))
            {
                Next();
                List<IValue> values = new List<IValue>();
                var value = ReadExpression();
                if (value is ExpressionList expl)
                {
                    foreach (var item in expl.GetList())
                    {
                        values.Add(item);
                    }
                }
                else
                {
                    values.Add(value);
                }
                insert.Values = values;
            }
            else
            {
                ex("在INSERT语句未在正确的位置找到关键字values。");
            }
            return insert;
        }
        #endregion

        #region 更新语句

        public Update ReadUpdate()
        {
            SkipComments();
            if (!matchToken(cur, TokenType.Keyword, "update"))
            {
                ex("未找到关键字UPDATE.");
            }
            else
            {
                Next();
            }
            Update update = new Update();
            update.Table = ReadTargetName();
            if (!matchToken(cur, TokenType.Keyword, "set"))
            {
                ex("UPDATE语句中未找到关键字set.");
            }
            else
            {
                Next();
                update.Updates = ReadUpdatePairs();
                update.Where = ReadWhere();
            }

            return update;
        }
        protected static UpdatePair AssignExpressionToUpdatePair(AssignExpression exp)
        {
            UpdatePair up = new UpdatePair();
            up.Field = ExpressionToTargetName(exp.Left);
            up.Value = exp.Right;
            return up;
        }

        public List<UpdatePair> ReadUpdatePairs()
        {
            List<UpdatePair> ps = new List<UpdatePair>();
            var value = ReadExpression();
            if (value is AssignExpression ass)
            {
                ps.Add(AssignExpressionToUpdatePair(ass));
                return ps;
            }
            else if (value is ExpressionList expl)
            {
                foreach (var item in expl.GetList())
                {
                    if (item is AssignExpression assitem)
                    {
                        ps.Add(AssignExpressionToUpdatePair(assitem));
                    }
                    else
                    {
                        break;
                    }
                }
                return ps;
            }
            ex("Update语句的更新列表出现语法错误。");
            return null;
        }
        #endregion

        #region 删除语句

        public Delete ReadDelete()
        {
            SkipComments();
            Delete del = new Delete();
            if (!matchToken(cur, TokenType.Keyword, "delete"))
            {
                ex("不是有效的DELETE语句。");
            }
            else
            {
                Next();
                del.Fields = ReadQueryFieldList(true);//DELETE语句允许不指定字段列表
                del.From = ReadFrom();
                del.Joins = ReadJoins();
                del.Where = ReadWhere();
            }


            return del;
        }

        #endregion
    }
}
