using System;
using System.Collections.Generic;
using System.Text;
using SharpSoft.Data.Lexing;

namespace SharpSoft.Data
{
    public class ExpressionParser : ParserBase
    {
        public ExpressionParser(string sou)
        {
            LexerSetting setting = LexerSetting.Default;
            setting.IgnoreCase = true;//词法解析中忽略大小写
            setting.InlineCommentsStartSign = "/*";//行内注释起始符
            setting.InlineCommentsEndSign = "*/"; //行内注释终止符
            setting.OutlineCommentsSign = "//";//行外注释
            setting.LiteralFirstChars = new char[] { '@' };//使@开头的标识符可以识别
            setting.CustomOperators = new string[] { "not", "and", "or", "in", "like", "is" };
            setting.Keywords = new string[] { "true", "fasle" };

            Lexer lexer = new Lexer(sou, OnLexerSetting(setting));
            SetTokens(lexer.Reslove());
        }
        /// <summary>
        /// 在派生类中重写为当前使用的词法解析器提供配置
        /// </summary>
        /// <param name="basesetting"></param>
        /// <returns></returns>
        protected virtual LexerSetting OnLexerSetting(LexerSetting basesetting)
        {
            return basesetting;
        }
        public ExpressionParser(Lexer lexer) : base(lexer)
        {
        }

        public ExpressionParser(Token[] ts) : base(ts)
        {
        }
        private IValue _parse(int startindex, int endindex, bool containsExplist = true)
        {
            base.ContextIn(startindex, endindex);
            SetCursor(startindex);
            IValue last = null;
            while (cur != null && cur.Type != TokenType.End)
            {
                var value = ReadOnePart(last, containsExplist);
                if (value == null)
                {
                    break;
                }
                last = value;
            }
            ContextUp();
            return last;
        }
        /// <summary>
        /// 读取一个独立的表达式
        /// </summary> 
        /// <returns></returns>
        public IValue ReadExpression(bool containsExplist = true)
        {
            return _parse(GetCursor(), -1, containsExplist);
        }

        /// <summary>
        /// 读取常量
        /// </summary>
        /// <returns></returns>
        private Constant ReadConstant()
        {
            var t = cur;
            if (t.Type == TokenType.String)
            {
                Next();
                return new Constant() { Type = ConstantType.String, Content = t.Content };
            }
            else if (t.Type == TokenType.Numeric)
            {
                Next();
                return new Constant() { Type = ConstantType.Numeric, Content = t.Content };
            }
            else
            {
                ex("此处不是有效的常量。");
                return null;
            }
        }
        /// <summary>
        /// 读取变量
        /// </summary>
        /// <returns></returns>
        private Variable ReadVariable()
        {
            var t = cur;
            if (t.Type == TokenType.Literal)
            {
                Next();
                return new Variable() { Name = t.Content };
            }
            else
            {
                ex("此处不是有效的变量。");
                return null;
            }

        }
        /// <summary>
        /// 读取函数
        /// </summary>
        /// <returns></returns>
        private Function ReadFunction()
        {
            var t = cur;
            Function f = new Function();
            f.Name = t.Content;
            Next();
            var args = ReadOnePart(null, true);
            if (args is ExpressionList)
            {
                f.Arguments = (ExpressionList)args;
            }
            else if (args == null)
            {
                f.Arguments = new ExpressionList();
            }
            else
            {
                f.Arguments = new ExpressionList();
                f.Arguments.Push(args);
            }
            return f;
        }
        /// <summary>
        /// 在派生类中重写来处理自定义的内容
        /// </summary>
        /// <param name="lastpart"></param>
        /// <returns></returns>
        protected virtual IValue OnReadExpressionPart(IValue lastpart)
        {
            return null;
        }
        private int ParentheseLeftCount = 0;
        /// <summary>
        /// 读取一个完整的表达式
        /// </summary>
        /// <param name="lastpart">上下文，上一次读取到的部分表达式（可能不完整）</param> 
        /// <returns></returns>
        private IValue ReadOnePart(IValue lastpart, bool containsExplist)
        {
            SkipComments();
            Token t = cur;
            if (t == null)
            {
                return null;
            }
            IValue value = OnReadExpressionPart(lastpart);
            if (value != null)
            {
                return value;
            }
            switch (t.Type)
            {
                case TokenType.Keyword:
                    if (lastpart != null)
                    {
                        return null;
                    }
                    if (t.Content == "true")
                    {
                        Next();
                        return True.Value;
                    }
                    else if (t.Content == "fasle")
                    {
                        Next();
                        return False.Value;
                    }
                    else if (t.Content == "null")
                    {
                        Next();
                        return Null.Value;
                    }
                    return null;
                case TokenType.Literal://变量或函数
                    if (lastpart != null)
                    {
                        return null;
                    }
                    var p = Preview(1);
                    if (p != null && p.Type == TokenType.Parenthese && p.Content == "(")
                    {//函数
                        value = ReadFunction();
                    }
                    else
                    {
                        value = ReadVariable();
                    }

                    break;
                case TokenType.String:
                case TokenType.Numeric://常量
                    if (lastpart != null)
                    {
                        return null;
                    }
                    value = ReadConstant();
                    break;
                case TokenType.Operator://运算符 
                    var opr = getBinaryOperator(t);
                    if (opr != BinaryOperator.None && lastpart != null)
                    {//为二元操作符
                        return ReadBinaryExpression(lastpart, opr);
                    }
                    switch (t.Content)
                    {
                        case "-":
                        case "!":
                        case "not":
                        case "~":
                        case "+":
                            value = ReadUnaryExpression();
                            break;
                        default:
                            break;
                    }
                    break;
                case TokenType.Parenthese://圆括号
                    if (t.Content == "(")
                    {
                        ParentheseLeftCount++;
                        ReadTokensInBrackets(out int start, out int end);
                        value = _parse(start + 1, end - 1, true);
                        if (value is IExpression)
                        {//括号中的表达式在逻辑上是独立的，不受二元操作符的优先级的影响
                            ((IExpression)value).Alone = true;
                        }
                        if (matchToken(cur, TokenType.Parenthese, ")"))
                        {
                            return value;
                        }
                        else
                        {
                            ex("未匹配到闭括号。");
                        }
                    }
                    else if (t.Content == ")")
                    {
                        if (ParentheseLeftCount > 0)
                        {
                            ParentheseLeftCount--;
                            Next();
                            return lastpart;
                        }
                        else
                        {
                            ex("意外的闭括号。");
                        }
                    }
                    else
                    {
                        return null;
                    }
                    break;
                case TokenType.Comma://逗号，表达式列表
                    if (containsExplist)
                    {
                        value = ReadExpressionList(lastpart);
                    }
                    else
                    {
                        return null;
                    }
                    break;
                case TokenType.Comments://注释已经忽略，不应出现在这里。
                    ex("此处不应出现注释。");
                    break;
                case TokenType.Semicolon://分号，终止表达式
                    Next();
                    break;
                case TokenType.End://结束标记，终止
                    break;
                case TokenType.Question:  // 基类中未使用的标记类别
                case TokenType.Bracket:
                case TokenType.CurlyBracket:
                case TokenType.Colon:
                default:
                    break;
            }

            return value;
        }
        ///// <summary>
        ///// 读取条件表达式
        ///// </summary>
        ///// <param name="condition"></param>
        ///// <returns></returns>
        //private ConditionExpression ReadConditionExpression(IValue condition)
        //{
        //    //TODO:?:
        //    return null;
        //}

        /// <summary>
        /// 读取表达式列表
        /// </summary>
        /// <param name="lastvalue"></param>
        /// <returns></returns>
        private ExpressionList ReadExpressionList(IValue lastvalue)
        {
            ExpressionList l = new ExpressionList();
            l.Push(lastvalue);
            while (matchToken(cur, TokenType.Comma))
            {
                Next();
                var exp = ReadExpression(false);
                if (exp != null)
                {
                    l.Push(exp);
                }
                else
                {
                    break;
                }
            }
            return l;
        }
        /// <summary>
        /// 读取一元表达式
        /// </summary>
        /// <returns></returns>
        private UnaryExpression ReadUnaryExpression()
        {
            var t = cur;
            Next();
            UnaryOperator opr = UnaryOperator.Plus;
            switch (t.Content)
            {
                case "!":
                case "not":
                    opr = UnaryOperator.Not;
                    break;
                case "-":
                    opr = UnaryOperator.Minus;
                    break;
                case "+":
                    opr = UnaryOperator.Plus;
                    break;
                case "~":
                    opr = UnaryOperator.Tilde;
                    break;
                default:
                    break;
            }
            var value = ReadOnePart(null, false);
            return new UnaryExpression() { Operator = opr, Value = value };

        }
        private BinaryExpression combineBinaryExpression(IValue leftvalue, BinaryOperator opr, IValue next)
        {
            var exp = new BinaryExpression();
            if (leftvalue is BinaryExpression)
            {
                var left = (BinaryExpression)leftvalue;
                if (PrecedenceOf(left.Operator) < PrecedenceOf(opr) && !left.Alone)
                {//下一个串联二元表达式操作符的优先级高于此前表达式
                    //将下个表达式按优先规则与当前表达式合并
                    exp.Left = left.Left;
                    exp.Operator = left.Operator;
                    exp.Right = combineBinaryExpression(left.Right, opr, next);

                    //exp.Alone = true;
                    return exp;
                }
                else if (PrecedenceOf(left.Operator) == PrecedenceOf(opr))
                { //与下个串联表达式优先级相同 
                }
                else
                {//下一个串联表达式的优先级较低，独立此前表达式
                    left.Alone = true;
                }
            }
            exp.Left = leftvalue;
            exp.Operator = opr;
            exp.Right = next;
            return exp;
        }
        /// <summary>
        /// 读取一个二元表达式
        /// </summary>
        /// <param name="leftvalue">该表达式左边的值</param>
        /// <returns></returns>
        private BinaryExpression ReadBinaryExpression(IValue leftvalue, BinaryOperator opr)
        {
            Next();
            IValue value = ReadOnePart(null, false);
            if (value == null)
            {
                ex("表达式不完整");
            }
            var exp = combineBinaryExpression(leftvalue, opr, value);

            if (exp.Operator == BinaryOperator.Dot && exp.Right is Variable)
            {//访问属性/函数，该表达式在逻辑上独立
                exp.Alone = true;
            }
            return exp;
        }
        BinaryOperator getBinaryOperator(Token t)
        {
            if (t.Type != TokenType.Operator)
            {
                return BinaryOperator.None;
            }
            return BinaryExpression.GetOperator(t.Content);
        }
        /// <summary>
        /// 获取操作符的优先级（数字越大优先级越高）
        /// </summary>
        /// <param name="operater"></param>
        /// <returns></returns>
        int PrecedenceOf(BinaryOperator operater)
        {
            switch (operater)
            {
                case BinaryOperator.None:
                    return -99;
                case BinaryOperator.Dot:
                    return 99;
                case BinaryOperator.And:
                case BinaryOperator.AndAlso:
                case BinaryOperator.Or:
                case BinaryOperator.OrAlso:
                    return 0;
                case BinaryOperator.Equals:
                case BinaryOperator.NotEquals:
                case BinaryOperator.GreaterThan:
                case BinaryOperator.GreaterEquals:
                case BinaryOperator.LessThan:
                case BinaryOperator.LessEquals:

                case BinaryOperator.Like:
                case BinaryOperator.In:
                case BinaryOperator.Is:
                    return 1;
                case BinaryOperator.Plus:
                case BinaryOperator.Minus:
                    return 2;
                case BinaryOperator.Multiply:
                case BinaryOperator.Divide:
                case BinaryOperator.Mod:
                    return 3;

                default:
                    return -1;
            }

        }
    }
}
