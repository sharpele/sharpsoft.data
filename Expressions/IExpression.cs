
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace SharpSoft.Data.Expressions
{
    using SharpSoft.Data.Lexing;
    public interface IExpression
    {
        /// <summary>
        /// 获取表达式的起始标记
        /// </summary>
        Token StartToken { get; }
        /// <summary>
        /// 获取表达式的结束标记
        /// </summary>
        Token EndToken { get; }
    }
    public class Expression : IExpression
    {
        public Token StartToken { get; protected internal set; }
        public Token EndToken { get; protected internal set; }
    }
    /// <summary>
    /// 字符串
    /// </summary>
    public class StringExpression : Expression
    {
        public string Content { get; set; }
        public override string ToString()
        {
            return $"\"{Content}\"";
        }
    }

    public class NullExpression : Expression
    {
        public override string ToString()
        {
            return "NULL";
        }
    }
    /// <summary>
    /// 数值
    /// </summary>
    public class NumericExpression : Expression
    {
        public string Content { get; set; }
        public override string ToString()
        {
            return Content;
        }
    }

    public class BooleanExpression : Expression
    {
        public bool Content { get; set; }

        public override string ToString()
        {
            return $"{(Content ? "True" : "False")}";
        }
    }
    /// <summary>
    /// 变量
    /// </summary>
    public class VariableExpression : Expression
    {
        public string Name { get; set; }
        public override string ToString()
        {
            return $"`{Name}`";
        }
    }
    /// <summary>
    /// 括号表达式
    /// </summary>
    public class BracketExpression : Expression
    {
        /// <summary>
        /// 括号内的表达式
        /// </summary>
        public IExpression Inner { get; set; }
        /// <summary>
        /// 括号类别
        /// </summary>
        public BeacketType BeacketType { get; set; }

        public override string ToString()
        {
            string temp = "";
            switch (BeacketType)
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
            return string.Format(temp, Inner == null ? "" : Inner.ToString());
        }
    }
    /// <summary>
    /// 函数
    /// </summary>
    public class FunctionExpression : Expression
    {
        public string Name { get; set; }

        public ListExpression Arguments { get; set; }

        public override string ToString()
        {
            return $"{Name}({Arguments})";
        }
    }
    /// <summary>
    /// 表达式列表
    /// </summary>
    public class ListExpression : Expression, IEnumerable<IExpression>
    {
        private readonly List<IExpression> list;
        public ListExpression()
        {
            list = new List<IExpression>();
        }

        public int ItemsCount { get { return list.Count; } }

        public IExpression this[int index]
        {
            get { return list[index]; }
            set { list[index] = value; }
        }
        public bool HasItem { get { return list.Count > 0; } }

        public void Add(IExpression exp)
        {
            list.Add(exp);
        }

        public override string ToString()
        {
            List<string> strs = new List<string>();
            foreach (var item in list)
            {
                if (item == null)
                {
                    strs.Add("");
                }
                else
                {
                    strs.Add(item.ToString());
                }
            }
            return string.Join(",", strs.ToArray());
        }
        public IEnumerator<IExpression> GetEnumerator()
        {
            return list.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return list.GetEnumerator();
        }
    }
    /// <summary>
    /// 类型
    /// </summary>
    public class TypeExpression : Expression
    {
        public string TypeName { get; set; }
        public override string ToString()
        {
            return TypeName;
        }
    }

    /// <summary>
    /// 赋值表达式
    /// </summary>
    public class AssignExpression : Expression
    {
        public VariableExpression Left { get; set; }

        public IExpression Right { get; set; }
    }

    /// <summary>
    /// 二元表达式
    /// </summary>
    public class BinaryExpression : Expression
    {
        //二元操作符优先级
        private static Dictionary<string, int> BinaryOperatorsLevel = new Dictionary<string, int> {
            {".",0 },
            { "*",1},{ "/",1},{ "%",1},
            { "+",2},{ "-",2},
            { "<<",3},{ ">>",3},
            { "<",4},{ ">",4},{ "<=",4},{ ">=",4},{ "is",4},{ "as",4},
            { "==",5},{ "!=",5},{ "<>",5},{"like",5 },{ "in",5},
            { "&",6},{ "and",6},
            { "^",7},
            { "|",8},{ "or",8},
            { "&&",9},
            { "||",10},
            
            { "=",15},{ ":=",15},
        };
        public IExpression Left { get; set; }

        public IExpression Right { get; set; }
        /// <summary>
        /// 操作方法
        /// </summary>
        public string Method { get; set; }

        public override string ToString()
        {
            return $"{Left} {Method} {Right}";
        }

        /// <summary>
        /// 向二元表达式末尾附加一个节点并自动处理操作符的优先级
        /// </summary>
        /// <param name="addmethod"></param>
        /// <param name="addexp"></param>
        public void Append(string addmethod, IExpression addexp)
        {
            if (string.IsNullOrEmpty(addmethod))
            {
                if (Left == null)
                {
                    Left = addexp;
                    return;
                }
                if (Right == null)
                {
                    Right = addexp;
                    return;
                }
                else
                {
                    throw new Exception("当前表达式已完整，在未指定下个操作符的情况下无法向当前二元表达式附加表达式。");
                }
            }
            if (string.IsNullOrEmpty(Method))
            {
                Method = addmethod;
                if (Right != null)
                {
                    throw new Exception("二元表达式附加逻辑错误。");
                }
                else
                {
                    Right = addexp;
                    return;
                }
            }


            BinaryExpression be = this;
            while (be != null)
            {
                if (be.Right is BinaryExpression right)
                {
                    be = right;
                }
                else
                {
                    break;
                }
            }
            var l1 = BinaryOperatorsLevel[be.Method];//原操作符优先级
            var l2 = BinaryOperatorsLevel[addmethod];//新操作符优先级
            var lroot = BinaryOperatorsLevel[this.Method];//根操作符优先级
            if (lroot <= l2)//与根表达式操作符的优先级比较，若低于根表达式则将新表达式提升为根表达式
            {
                var newleft = new BinaryExpression() { Left = this.Left, Right = this.Right, Method = this.Method };
                var newright = addexp;
                this.Left = newleft;
                this.Right = newright;
                this.Method = addmethod;
            }
            else
            {
                if (l1 <= l2)//与上一节点表达式操作符的优先级比较，若低于上一节点表达式则将新表达式置于节点末尾。
                {
                    var newleft = new BinaryExpression() { Left = be.Left, Right = be.Right, Method = be.Method };
                    var newright = addexp;
                    be.Left = newleft;
                    be.Right = newright;
                    be.Method = addmethod;
                }
                else//若高于上一节点表达式则将新表达式植入最后一个表达式
                {
                    var newright = new BinaryExpression() { Left = be.Right, Right = addexp, Method = addmethod };
                    be.Right = newright;

                }

            }
        }
    }
    /// <summary>
    /// 一元表达式
    /// </summary>
    public class UnaryExpression : Expression
    {
        /// <summary>
        /// 操作数
        /// </summary>
        public IExpression Operand { get; set; }
        /// <summary>
        /// 操作方法
        /// </summary>
        public string Method { get; set; }

        public override string ToString()
        {
            return $"{Method}{Operand}";
        }
    } 
    /// <summary>
    /// 索引访问器表达式
    /// </summary>
    public class IndexExpression : Expression
    {
        /// <summary>
        /// 要访问其成员的对象
        /// </summary>
        public Expression Expression { get; set; }
        /// <summary>
        /// 索引器参数
        /// </summary>
        public ListExpression Arguments { get; set; }
    }
    /// <summary>
    /// 通配符
    /// </summary>
    public class WildcardExpression : Expression
    {
        public override string ToString()
        {
            return "*";
        }
    }

}
