
using System.Text;

namespace SharpSoft.Data.Expressions
{
    using SharpSoft.Data.Lexing;
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    public class ExpressionAnalyzer
    {
        public ExpressionAnalyzer(string sou)
        {
            LexerSetting setting = LexerSetting.Default;
            setting.IgnoreCase = true;//词法解析中忽略大小写
            setting.InlineCommentsStartSign = "/*";//行内注释起始符
            setting.InlineCommentsEndSign = "*/"; //行内注释终止符
            setting.OutlineCommentsSign = "//";//行外注释
            setting.LiteralFirstChars = new char[] { '@' };//使@开头的标识符可以识别 
            setting.CustomOperators = new string[] { ":=", "not", "and", "or", "in", "like", "is", "as" };
            List<string> keys = new List<string>();
            var ks = Keywords();
            if (ks != null)
            {
                keys.AddRange(ks);
            }
            setting.Keywords = keys.ToArray();
            Lexer lexer = new Lexer(sou, setting);
            tokens = lexer.Reslove();
        }

        private string[] UnaryOperators = { "not", "!", "-", "+" };

        private readonly Token[] tokens;
        private int cursor = 0;
        public ExpressionAnalyzer(Token[] p_tokens)
        {
            tokens = p_tokens;
        }
        /// <summary>
        /// 在派生类中重写，提供分析器保留关键字（均为小写）。
        /// </summary>
        /// <returns></returns>
        protected virtual string[] Keywords()
        {
            return new string[] {
                "if","else", "default", "true", "false" , "null",
               "case","when","end"
            };
        }

        #region 游标

        protected Token Current
        {
            get
            {
                return Get(cursor);
            }
        }
        /// <summary>
        /// 转到下一个非注释标记
        /// </summary>
        protected void Next()
        {
            do
            {
                cursor++;
            } while (MatchToken(TokenType.Comments));
        }
        /// <summary>
        /// 转到上一个非注释标记
        /// </summary>
        protected void Previous()
        {
            do
            {
                cursor--;
            } while (MatchToken(TokenType.Comments));
        }

        protected Token Get(int index)
        {
            if (index < 0 || index >= tokens.Length)
            {
                return new Token(TokenType.End, null);
            }
            return tokens[index];
        }

        protected Token Preview(int offset = 1)
        {
            return Get(cursor + offset);
        }
        /// <summary>
        /// 匹配当前标记
        /// </summary>
        /// <param name="type"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        protected bool MatchToken(TokenType type, string content = null)
        {
            return Current.MatchToken(type, content);
        }
        /// <summary>
        /// 从当前位置开始匹配多个关键字,匹配成功后将游标置于这些关键字之后。
        /// </summary>
        /// <param name="keys"></param>
        /// <returns></returns>
        protected bool MatchKeyword(params string[] keys)
        {
            if (keys == null || keys.Length == 0)
            {
                return true;
            }
            bool allmatch = true;
            foreach (var item in keys)
            {
                if (MatchToken(TokenType.Keyword, item))
                {
                    Next();
                }
                else
                {
                    allmatch = false;
                    break;
                }
            }
            return allmatch;
        }
        /// <summary>
        /// 匹配当前位置括号中的内容，并将游标置于匹配的闭括弧上。
        /// </summary>
        /// <param name="bt">起始开括号</param>
        /// <returns></returns>
        protected Token[] MatchBracket()
        {
            Token bt = Current;
            if (bt.Type == TokenType.Bracket)
            {
                int startIndex = cursor;
                string startBracket = bt.Content;
                string endBracket = "\0";
                switch (bt.Content)
                {
                    case "(":
                        endBracket = ")";
                        break;
                    case "[":
                        endBracket = "]";
                        break;
                    case "{":
                        endBracket = "}";
                        break;
                }
                int bkscount = 1;//记录开始括号的数量
                int bkecount = 0;//记录结束括号的数量

                Next();

                while (true)
                {
                    if (MatchToken(TokenType.Bracket, endBracket))
                    {
                        bkecount++;
                    }
                    else if (MatchToken(TokenType.Bracket, startBracket))
                    {
                        bkscount++;
                    }
                    if (bkscount == bkecount)
                    {//数量匹配，找到结束括号位置,返回括号间的内容
                        int endIndex = cursor;
                        Token[] tks = new Token[endIndex - startIndex - 1];
                        for (int i = 0; i < tks.Length; i++)
                        {
                            tks[i] = Get(startIndex + i + 1);
                        }
                        Next();//跳过闭括弧
                        return tks;
                    }
                    if (MatchToken(TokenType.End))
                    {//直到源末尾，尚未匹配到结束括号。
                        error($"此处应为闭括号'{endBracket}'。");
                        break;
                    }
                    Next();
                }
            }
            else
            {
                error("此处无法开始匹配括号。");
            }
            return null;
        }

        [System.Diagnostics.DebuggerStepThrough]
        protected void error(string msg)
        {
            throw new SyntaxException(msg) { Token = Current };
        }
        /// <summary>
        /// 提交警告
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="tk"></param>
        /// <param name="exp"></param>
        protected void waring(string msg, Token tk, IExpression exp = null)
        {
            var war = new SyntaxWaring() { Message = msg, Expression = exp, WaringToken = tk };
        }
        /// <summary>
        /// 返回一个新的处理器实例用于处理括号中的内容,调用此方法后游标位于闭括弧上。
        /// </summary>
        protected ExpressionAnalyzer ProcessBracket()
        {
            var cur = Current;
            if (cur.Content == "(" || cur.Content == "[" || cur.Content == "{")
            {
                var tks = MatchBracket();
                ExpressionAnalyzer ana;
                var ctor = this.GetType().GetConstructor(new Type[] { typeof(Token[]) });
                if (ctor == null)
                {
                    ana = new ExpressionAnalyzer(tks);
                }
                else
                {
                    ana = (ExpressionAnalyzer)ctor.Invoke(new object[] { tks });
                }

                return ana;
            }
            else if (cur.Content == ")" || cur.Content == "]" || cur.Content == "}")
            {
                error($"意外的闭括弧'{cur.Content}'。");
            }
            return null;
        }

        #endregion
        public IExpression ReadExpressionTree()
        {
            var exp = ReadExpressionTree(null, null);
            if (exp is BinaryExpression binexp)
            {
                if (binexp.Right == null)
                {//构不成二元表达式则直接返回该表达式
                    return binexp.Left;
                }
            }
            return exp;

        }
        /// <summary>
        /// 从当前位置读取一个表达式树
        /// </summary>
        /// <returns></returns>
        protected virtual IExpression ReadExpressionTree(BinaryExpression context, string lastmethod = null)
        {
            var first = Current;
            BinaryExpression binexp = null;
            if (context != null)
            {
                binexp = context;
                if (string.IsNullOrEmpty(lastmethod))
                {
                    return context;
                }
            }

            IExpression exp = ReadPrimitiveExpression();
            string method = null;
            if (MatchToken(TokenType.Operator))
            {//匹配到操作符。 
                method = Current.Content;
                Next();
            }
            else if (MatchToken(TokenType.Question))//表达式后为问号，读取条件表达式，条件表达式的优先级高于任何其他同级操作符。
            {
                Next();
                var trueexp = ReadExpressionTree();
                if (MatchToken(TokenType.Colon))
                {//冒号
                    Next();
                    var falseexp = ReadExpressionTree();
                    exp = new ConditionalExpression() { StartToken = first, EndToken = Preview(-1), Condition = exp, TrueValue = trueexp, FalseValue = falseexp };
                    if (MatchToken(TokenType.Operator))
                    {
                        method = Current.Content;
                        Next();
                    }
                }
                else
                {
                    error("在bool?trueexp:falseexp表达式中未发现冒号[:]。");
                }
            }
            else if (MatchToken(TokenType.Keyword))
            {
                var tk = Current;
                if (tk.Content == "asc" || tk.Content == "desc")
                {
                    OrderType ot;
                    if (tk.Content == "desc")
                    {
                        ot = OrderType.Desc;
                    }
                    else
                    {
                        ot = OrderType.Asc;
                    }
                    exp = new OrderExpression() { Expression = exp, OrderType = ot };
                    Next();
                    if (MatchToken(TokenType.Operator))
                    {
                        method = Current.Content;
                        Next();
                    }
                }

            }
            else if (MatchToken(TokenType.Literal) || MatchToken(TokenType.Confine))
            {//无操作符之后的对象为变量名则当作别名表达式
                var vari = ReadVariable();
                exp = new BinaryExpression() { Left = exp, Right = vari, Method = "as" };
                if (MatchToken(TokenType.Operator))
                {
                    method = Current.Content;
                    Next();
                }
            }



            if (binexp == null)
            {//第一个二元表达式节点
                if (exp == null)
                {//未解析任何表达式
                    return null;
                }
                if (string.IsNullOrEmpty(method))
                {//表达式树已终止终结
                    return exp;
                }
                else
                {
                    binexp = new BinaryExpression() { StartToken = first, Left = exp, Right = null, Method = null };
                    binexp = (BinaryExpression)ReadExpressionTree(binexp, method);//递归读取其他节点
                }

            }
            else
            {
                if (lastmethod != null && exp == null)
                {//尾部发现了操作符，但是下一节点读取不到任何表达式
                    Previous();
                    error("表达式树尾部缺失。");
                }
                binexp.Append(lastmethod, exp);
                if (!string.IsNullOrEmpty(method))
                {
                    return ReadExpressionTree(binexp, method);//递归读取其他节点
                }
                else
                {
                    return binexp;
                }
            }
            binexp.EndToken = Preview(-1);
            return binexp;
        }
        protected virtual VariableExpression ReadVariable()
        {
            var cur = Current;
            if (cur.MatchToken(TokenType.Literal))
            {
                Next();
                return new VariableExpression() { Name = cur.Content };
            }
            else if (cur.MatchToken(TokenType.Confine))
            {
                Next();
                cur = Current;
                if (cur.Type == TokenType.Literal || cur.Type == TokenType.Keyword)
                {
                    Next();
                    if (MatchToken(TokenType.Confine))
                    {
                        Next();
                        return new VariableExpression() { Name = cur.Content };
                    }
                    else
                    {
                        error("安全名称符号[`]必须成对存在。");
                    }
                }
                else
                {
                    error("安全名称符号[`]之内必须为关键字或变量名。");
                }

            }
            return null;
        }
        /// <summary>
        /// 从当前位置获取一个基元表达式（一元表达式，变量，常量，函数，括号中的表达式）
        /// </summary>
        /// 
        /// <returns></returns>
        public virtual IExpression ReadPrimitiveExpression()
        {
            /*
             如果在内部调用了让游标置后的方法则应执行 Previous() 方法将游标提升，此方法末尾会对游标作统一处理。
             ******后续添加处理时切忌在中途直接返回表达式对象，应将其赋予exp变量，交由本函数末尾统一处理。
             */
            var cur = Current;
            IExpression exp = null;
            switch (cur.Type)
            {
                case TokenType.Literal:
                    var prev = Preview();
                    if (prev.MatchToken(TokenType.Bracket, "("))
                    {//变量后圆括号视为函数 
                        exp = ReadFunctionExpression();
                        Previous();//将游标提升一步。
                    }
                    else
                    {
                        exp = new VariableExpression() { Name = cur.Content };
                    }
                    break;
                case TokenType.String:
                    exp = new StringExpression() { Content = cur.Content };
                    break;
                case TokenType.Numeric:
                    exp = new NumericExpression() { Content = cur.Content };
                    break;
                case TokenType.Keyword:
                    if (cur.MatchToken(TokenType.Keyword, "true"))
                    {
                        exp = new BooleanExpression() { Content = true };
                    }
                    else if (cur.MatchToken(TokenType.Keyword, "false"))
                    {
                        exp = new BooleanExpression() { Content = false };
                    }
                    else if (cur.MatchToken(TokenType.Keyword, "null"))
                    {
                        exp = new NullExpression() { };
                    }
                    else if (cur.MatchToken(TokenType.Keyword, "case"))
                    {
                        exp = ReadCaseExpression();
                        Previous();//将游标提升一步。
                    }
                    break;
                case TokenType.Bracket://处理括号

                    var ana = ProcessBracket();
                    BracketExpression bracket = new BracketExpression() { };
                    var inner = ana.ReadListExpression();
                    if (inner != null && inner.ItemsCount == 1)
                    {//列表只有一个子项
                        bracket.Inner = inner[0];
                    }
                    else
                    {
                        bracket.Inner = inner;
                    }

                    switch (cur.Content)
                    {
                        case "(":
                            bracket.BeacketType = BeacketType.RoundBrackets;
                            break;
                        case "[":
                            bracket.BeacketType = BeacketType.SquareBrackets;
                            break;
                        case "{":
                            bracket.BeacketType = BeacketType.CurlyBrace;
                            break;
                        default:
                            bracket.BeacketType = BeacketType.Unknow;
                            break;
                    }
                    exp = bracket;
                    Previous();//将游标提升一步。
                    break;
                case TokenType.Confine:
                    exp = ReadVariable();
                    Previous();//将游标提升一步。
                    break;
                case TokenType.Operator:
                    foreach (var item in UnaryOperators)
                    {
                        if (item == cur.Content)
                        {
                            Next();
                            UnaryExpression unary = new UnaryExpression() { };
                            unary.Method = cur.Content;
                            var exp1 = ReadPrimitiveExpression();
                            Previous();//将游标提升一步。
                            if (exp1 == null)
                            {
                                error("一元操作符没有合适的操作数对象。");
                            }
                            unary.Operand = exp1;
                            exp = unary;
                            break;
                        }
                    }
                    break;//除一元操作符外，其他操作符不在此方法中处理 
                case TokenType.Colon: //冒号
                case TokenType.Question://问号 
                case TokenType.Semicolon: //分号
                case TokenType.Comments:
                case TokenType.Comma://逗号只在表达式列表中处理
                case TokenType.End:
                    exp = null;
                    break;
            }
            if (exp != null)
            {
                if (exp is Expression exppp)
                {
                    exppp.StartToken = cur;
                    exppp.EndToken = Current;
                }

                Next();
            }

            return exp;
        }

        protected virtual CaseExpression ReadCaseExpression()
        {
            Token tk = Current;
            Next();
            var input = ReadExpressionTree(null);
            CaseExpression caseexp = new CaseExpression() { Input = input };

            while (true)
            {
                if (MatchToken(TokenType.Keyword, "when"))
                {
                    Next();
                    var value = ReadExpressionTree(null);
                    if (value == null)
                    {
                        error("关键字when之后应为表达式。");
                    }
                    if (MatchToken(TokenType.Keyword, "then"))
                    {
                        Next();
                        var result = ReadExpressionTree(null);
                        if (result == null)
                        {
                            error("关键字then之后应为表达式。");
                        }
                        caseexp.Branches.Add(new Branch() { Value = value, Result = result });
                    }
                }
                else
                {
                    break;
                }

            }
            if (MatchToken(TokenType.Keyword, "else"))
            {
                Next();
                var elseb = ReadExpressionTree(null);
                caseexp.ElseBrance = elseb;
            }
            if (MatchToken(TokenType.Keyword, "end"))
            {
                Next();
            }
            else
            {
                error("case表达式缺少结束标记[end]。");
            }
            return caseexp;
        }
        /// <summary>
        /// 读取一个函数
        /// </summary>
        /// <param name="tk"></param>
        /// <returns></returns>
        protected virtual FunctionExpression ReadFunctionExpression()
        {
            var tk = Current;
            if (tk.Type != TokenType.Literal)
            {
                error("标记类型错误。");
            }
            if (MatchToken(TokenType.Bracket, "("))
            {
                error("标记类型错误。");
            }
            FunctionExpression fun = new FunctionExpression() { Name = tk.Content };
            Next();
            var ana = ProcessBracket();
            fun.Arguments = ana.ReadListExpression(); 
            return fun;
        }

        /// <summary>
        /// 从当前位置读取一个表达式列表
        /// </summary>
        /// <returns></returns>
        protected virtual ListExpression ReadListExpression()
        {
            ListExpression list = new ListExpression() { StartToken = Current };
            IExpression expression = null;
            while (true)
            {
                expression = ReadExpressionTree();
                if (expression != null)
                {
                    list.Add(expression);
                }
                else
                {
                    waring("表达式列表中发现Null项。", Current, list);
                }
                if (MatchToken(TokenType.Comma))
                {
                    Next();
                }
                else
                {//未匹配到逗号，列表终结
                    break;
                }

            }
            list.EndToken = Preview(-1);
            return list;
        }

    }
}
