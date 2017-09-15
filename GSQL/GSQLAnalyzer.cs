using System;
using System.Collections.Generic;
using System.Text;

namespace SharpSoft.Data.GSQL
{
    using Expressions;
    using Lexing;
    public class GSQLAnalyzer : ExpressionAnalyzer
    {
        static GSQLAnalyzer()
        {
            datatypes = Enum.GetNames(typeof(DataType));
        }
        static string[] datatypes;
        public GSQLAnalyzer(string sou) : base(sou)
        {
        }
        public GSQLAnalyzer(Token[] p_tokens) : base(p_tokens)
        {

        }
        protected override string[] Keywords()
        {
            return new string[] {"create","table","constraint","primary","foreign","key","unique","references",
                "default","auto", "var","declare","ifnotexists",
                "if","else", "default", "true", "false" , "null","comment",
                "select", "from", "where", "insert", "into", "values", "update", "set", "delete",
                "join","on","left","right","full","inner","having","asc","desc",  "group", "order","by", "union","all", "limit", "offset",
               "case","when","end"
            };
        }



        public override IExpression ReadPrimitiveExpression()
        {
            if (MatchToken(TokenType.Bracket, "["))
            {//方括号中的变量直接当作变量处理。
                var exp0 = base.ReadPrimitiveExpression();
                if (exp0 is BracketExpression be)
                {
                    if (be.Inner is VariableExpression vari)
                    {
                        return vari;
                    }
                }
                return exp0;
            }

            var exp = base.ReadPrimitiveExpression();
            var cur = Current;
            if (exp == null)
            {//基类未处理的表达式
                if (cur.MatchToken(TokenType.Operator, "*"))
                {//乘号单独出现当作通配符
                    exp = new WildcardExpression() { };

                }
                else if (cur.MatchToken(TokenType.Keyword, "select"))
                {//查询语句
                    exp = ReadSelect();
                }



                if (exp is Expression expb)
                {
                    expb.StartToken = cur;
                    expb.EndToken = Current;
                }
                if (exp != null)
                {
                    Next();
                }
            }
            return exp;
        }


        #region SELECT

        private ListExpression ReadFrom()
        {
            if (MatchToken(TokenType.Keyword, "from"))
            {
                Next();
                var tables = ReadListExpression();
                if (tables == null || !tables.HasItem)
                {
                    error("SELECT语句定义了from关键字，但是却未找到有效的表定义。");
                }
                return tables;
            }
            return null;
        }
        private IExpression ReadWhere()
        {
            if (MatchToken(TokenType.Keyword, "where"))
            {
                Next();
                var where = ReadExpressionTree();
                if (where == null)
                {
                    error("SELECT语句定义了where关键字，但是却未找到有效的where条件定义。");
                }
                return where;
            }
            return null;
        }

        private ListExpression ReadGroupBy()
        {
            if (MatchToken(TokenType.Keyword, "group"))
            {
                Next();
                if (MatchToken(TokenType.Keyword, "by"))
                {
                    Next();
                    var cols = ReadListExpression();
                    if (cols == null)
                    {
                        error("SELECT语句定义了group by关键字，但是却未找到有效的group by列表。");
                    }
                    return cols;
                }
                else
                {
                    error("关键字group后必须为关键字by。");
                }
            }
            return null;
        }

        private IExpression ReadHaving()
        {
            if (MatchToken(TokenType.Keyword, "having"))
            {
                Next();
                var having = ReadExpressionTree();
                if (having == null)
                {
                    error("定义了having关键字但是找不到having之后的表达式。");
                }
                return having;
            }
            return null;
        }

        private ListExpression ReadOrderBy()
        {
            if (MatchToken(TokenType.Keyword, "order"))
            {
                Next();
                if (MatchToken(TokenType.Keyword, "by"))
                {
                    Next();
                    var cols = ReadListExpression();
                    if (cols == null)
                    {
                        error("SELECT语句定义了group by关键字，但是却未找到有效的group by列表。");
                    }
                    return cols;
                }
                else
                {
                    error("关键字group后必须为关键字by。");
                }
            }
            return null;

        }

        private Limit ReadLimit()
        {
            if (MatchToken(TokenType.Keyword, "limit"))
            {
                Next();
                var el = ReadListExpression();
                if (el == null || !el.HasItem)
                {
                    error("limit关键字之后未发现表达式。");
                }
                Limit l = new Limit();
                if (el.ItemsCount == 1)
                {
                    l.Rows = el[0];
                    l.Offset = null;
                }
                else if (el.ItemsCount == 2)
                {
                    l.Offset = el[0];
                    l.Rows = el[1];
                }
                else
                {
                    error("limit子句的参数数目不正确。");
                }
                return l;
            }
            return null;
        }

        protected SelectStatement ReadSelect()
        {
            SelectStatement select = new SelectStatement();
            if (MatchToken(TokenType.Keyword, "select"))
            {
                Next();
                var columns = ReadListExpression();
                if (columns == null || !columns.HasItem)
                {
                    error("未找到有效的列定义。");
                }
                select.Columns = columns;
                select.Tables = ReadFrom();
                select.Where = ReadWhere();
                select.GroupBy = ReadGroupBy();
                select.Having = ReadHaving();
                select.OrderBy = ReadOrderBy();
                select.Limit = ReadLimit();
            }
            else
            {
                error("select语句必须以关键字[select]开头。");
            }
            return select;
        }

        #endregion

        #region UPDATE

        private ListExpression ReadSet()
        {
            if (MatchToken(TokenType.Keyword, "set"))
            {
                Next();
                var sets = ReadListExpression();
                if (sets == null || !sets.HasItem)
                {
                    error("update语句的set关键字后未找到有效的修改值列表。");
                }
                return sets;
            }
            return null;
        }

        protected UpdateStatement ReadUpdate()
        {
            UpdateStatement update = new UpdateStatement();
            if (MatchToken(TokenType.Keyword, "update"))
            {
                Next();
                var tables = ReadListExpression();

                if (tables == null || !tables.HasItem)
                {
                    error("未找到有效的列定义。");
                }
                update.Tables = tables;
                update.Updates = ReadSet();
                update.Where = ReadWhere();
                update.OrderBy = ReadOrderBy();
                update.Limit = ReadLimit();
            }
            else
            {
                error("update语句必须以关键字[update]开头。");
            }
            return update;
        }
        #endregion

        #region DELETE
        protected DeleteStatement ReadDelete()
        {
            DeleteStatement delete = new DeleteStatement();
            if (MatchToken(TokenType.Keyword, "delete"))
            {
                Next();
                if (MatchToken(TokenType.Keyword, "from"))
                {
                    Next();
                    delete.Table = ReadExpressionTree();
                    delete.Where = ReadWhere();
                    delete.OrderBy = ReadOrderBy();
                    delete.Limit = ReadLimit();
                    return delete;
                }
                else
                {
                    error("delete语句中未发现关键字[from]。");
                }
            }
            else
            {
                error("delete语句必须以关键字[delete]开头。");
            }
            return null;
        }
        #endregion

        #region INSERT

        protected IStatement ReadInsert()
        {
            if (MatchToken(TokenType.Keyword, "insert"))
            {
                Next();
                if (MatchToken(TokenType.Keyword, "into"))
                {
                    Next();
                    InsertStatement insert = new InsertStatement();
                    insert.Table = ReadPrimitiveExpression();
                    if (insert.Table == null)
                    {
                        error("insert语句未定义有效的目标表。");
                    }
                    insert.Columns = ReadExpressionTree();
                    if (MatchToken(TokenType.Keyword, "values"))
                    {
                        Next();
                        insert.Values = ReadExpressionTree();
                    }
                    return insert;
                }
                else
                {
                    error("insert关键字之后必须为[into]关键字。");
                }
            }
            else
            {
                error("insert语句必须以关键字[insert]开头。");
            }
            return null;
        }

        #endregion

        #region CREATE_TABLE

        private List<IColumnDefine> ReadColumnDefines()
        {
            List<IColumnDefine> list = new List<IColumnDefine>();
            while (!MatchToken(TokenType.End))
            {
                IColumnDefine cd = ReadConstraintDefine();
                if (cd == null)
                {
                    cd = ReadColumnDefine();
                }
                list.Add(cd);
                if (MatchToken(TokenType.Comma))
                {
                    Next();//忽略之后分割的逗号
                }
                else
                {
                    if (MatchToken(TokenType.End))
                    {
                        break;
                    }
                    else
                    {
                        error("此处应为逗号。");
                    }
                }
            }

            return list;
        }
        private DataType ReadDataType()
        {
            var type = ReadVariable();
            if (type == null)
            {
                error("未找到有效的数据类型定义。");
            }
            foreach (var item in datatypes)
            {
                if (type.Name.ToLower() == item.ToLower())
                {
                    return (DataType)Enum.Parse(typeof(DataType), item);
                }
            }
            error($"无效的数据类型[{type.Name}]");
            return DataType.VarChar;
        }
        /// <summary>
        /// 读取数据列定义
        /// </summary>
        /// <returns></returns>
        private ColumnDefine ReadColumnDefine()
        {
            var col = ReadVariable();
            if (col == null)
            {
                error("未找到有效的列名称定义。");
            }
            var type = ReadDataType();
            ColumnDefine cd = new ColumnDefine() { Name = col, Type = type };
            if (MatchToken(TokenType.Bracket, "("))
            {//类型有长度和精度定义
                var ada = (GSQLAnalyzer)ProcessBracket();
                cd.TypeDescriptor = ada.ReadListExpression();
            }
            while (true)
            {
                if (MatchKeyword("primary", "key"))
                {
                    cd.IsPrimaryKey = true;
                }
                else if (MatchKeyword("auto"))
                {
                    cd.Autoincrement = true;
                }
                else if (MatchKeyword("not", "null"))
                {
                    cd.Nullable = false;
                }
                else if (MatchKeyword("null"))
                {
                    cd.Nullable = true;
                }
                else if (MatchKeyword("unique"))
                {
                    cd.IsUnique = true;
                }
                else if (MatchKeyword("default"))
                {
                    var defval = ReadPrimitiveExpression();
                    cd.DefaultValue = defval;
                }
                else if (MatchKeyword("comment"))
                {
                    var com = ReadPrimitiveExpression();
                    cd.Comment = com;
                }
                else
                {
                    break;
                }
            }



            return cd;
        }
        /// <summary>
        /// 读取约束定义
        /// </summary>
        /// <returns></returns>
        private ConstraintDefine ReadConstraintDefine()
        {
            ConstraintDefine cons = new ConstraintDefine();
            if (MatchKeyword("constraint"))
            {
                var consname = ReadVariable();//尝试读取约束名称
                cons.Name = consname;
                ConstraintType ct = ConstraintType.Unique;
                if (MatchKeyword("primary", "key"))//主键约束
                {
                    ct = ConstraintType.PrimaryKey;
                }
                else if (MatchKeyword("foreign", "key"))
                {
                    ct = ConstraintType.ForeignKey;
                }
                else if (MatchKeyword("unique"))
                {
                    ct = ConstraintType.Unique;
                }
                else
                {
                    error("无法找到有效的约束类型。");
                }
                cons.Type = ct;
                if (MatchToken(TokenType.Bracket, "("))
                {
                    var ada = (GSQLAnalyzer)ProcessBracket();
                    var cols = ada.ReadListExpression();
                    cons.Columns = cols;
                    if (cols == null || !cols.HasItem)
                    {
                        error("约束中必须指定至少一个列名。");
                    }
                }
                else
                {
                    error("此处应为圆括弧'('。");
                }
                if (MatchKeyword("references"))
                {
                    if (ct != ConstraintType.ForeignKey)
                    {
                        error("只有外键约束才允许指定外键表引用。");
                    }
                    var fgref = new ForeignReferences();
                    var fgtable = ReadVariable();
                    if (fgtable == null)
                    {
                        error("必须为外键约束指定一个引用表名称。");
                    }
                    fgref.TableName = fgtable;
                    if (MatchToken(TokenType.Bracket, "("))
                    {
                        var ada = (GSQLAnalyzer)ProcessBracket();
                        var cols = ada.ReadListExpression();
                        fgref.Columns = cols;
                        if (cols == null || !cols.HasItem)
                        {
                            error("外键引用表必须指定至少一个列名。");
                        }
                        if (cols.ItemsCount != cons.Columns.ItemsCount)
                        {
                            error("外键引用表列数目必须与外键列数目一致。");
                        }
                    }
                    else
                    {
                        error("必须为外键表指定对应的外键列，此处应为圆括弧'('。");
                    }
                    cons.References = fgref;
                }
            }
            else
            {
                return null;
                //error("约束定义必须以关键字[constraint]开头。");
            }
            return cons;
        }

        protected CreateTableStatement ReadCreateTable()
        {
            if (MatchKeyword("create", "table"))
            {
                CreateTableStatement createTable = new CreateTableStatement();
                if (MatchKeyword("ifnotexists"))
                {
                    createTable.IfNotExists = true;
                }
                var tbname = ReadVariable();
                if (tbname == null)
                {
                    error("CREATE TABLE 语句未定义有效的数据表名称。");
                }
                createTable.Table = tbname;
                if (MatchToken(TokenType.Bracket, "("))
                {
                    var ada = (GSQLAnalyzer)ProcessBracket();
                    var cds = ada.ReadColumnDefines();
                    createTable.ColumnDefines.AddRange(cds);
                    return createTable;
                }
                else
                {
                    error("此处应为圆括弧'('。");
                }
            }

            return null;
        }

        #endregion

        public List<IStatement> ReadStatements()
        {
            List<IStatement> list = new List<IStatement>();
            while (true)
            {
                var sta = ReadStatement();
                if (sta != null)
                {
                    list.Add(sta);
                }
                else
                {
                    break;
                }
            }
            return list;
        }
        public IStatement ReadStatement()
        {
            IStatement statement = null;
            if (MatchToken(TokenType.Keyword, "select"))
            {
                statement = ReadSelect();
            }
            else if (MatchToken(TokenType.Keyword, "update"))
            {
                statement = ReadUpdate();
            }
            else if (MatchToken(TokenType.Keyword, "delete"))
            {
                statement = ReadDelete();
            }
            else if (MatchToken(TokenType.Keyword, "insert"))
            {
                statement = ReadInsert();
            }
            else if (MatchToken(TokenType.Keyword, "create"))
            {
                statement = ReadCreateTable();
            }

            else if (MatchToken(TokenType.End))
            {//遇到结尾
                return null;
            }
            else
            {
                error("无法识别的起始内容，不能从此处开始读取一条语句。");
            }

            if (MatchToken(TokenType.Semicolon))
            {//忽略语句末尾的分号。
                Next();
            }
            return statement;
        }
    }
}
