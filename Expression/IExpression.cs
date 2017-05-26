namespace SharpSoft.Data
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// 该接口表示一个可以求值的对象
    /// </summary>
    public interface IValue
    {

    }
    /// <summary>
    /// 该接口表示一个对象的结果为布尔值
    /// </summary>
    public interface IBoolean : IValue
    {

    }
    /// <summary>
    /// 表示空对象null
    /// </summary>
    public struct Null : IValue
    {
        public static Null Value { get; }
        public override bool Equals(object obj)
        {
            return obj is Null || obj==null;
        }
        public override int GetHashCode()
        {
            return -1;
        }
    }

    public struct True : IBoolean
    {
        public static True Value { get; }
        public override bool Equals(object obj)
        {
            return obj is True;
        }
        public override int GetHashCode()
        {
            return 1;
        }
        public override string ToString()
        {
            return "true";
        }
    }
    public struct False : IBoolean
    {
        public static False Value { get; }
        public override bool Equals(object obj)
        {
            return obj is False;
        }
        public override int GetHashCode()
        {
            return 0;
        }
        public override string ToString()
        {
            return "false";
        }

    }
    /// <summary>
    /// 常量类型
    /// </summary>
    public enum ConstantType
    {
        /// <summary>
        /// 字符串
        /// </summary>
        String,
        /// <summary>
        /// 数字
        /// </summary>
        Numeric
    }
    /// <summary>
    /// 常量
    /// </summary>
    public class Constant : IValue
    {
        public string Content { get; set; }

        public ConstantType Type { get; set; }
        public override string ToString()
        {
            switch (Type)
            {
                case ConstantType.String:
                    return $"\"{Content}\"";
                case ConstantType.Numeric:
                    return Content;
                default:
                    return "<unknow>";
            }
        }

        public int ToInt32()
        {
            return int.Parse(Content);
        }
        public Decimal ToDecimal()
        {
            return Decimal.Parse(Content);
        }
    }
    /// <summary>
    /// 变量
    /// </summary>
    public class Variable : IValue
    {
        public string Name { get; set; }
        public override string ToString()
        {
            return Name;
        }
    }
    /// <summary>
    /// 函数
    /// </summary>
    public class Function : IValue
    {
        public string Name { get; set; }
        /// <summary>
        /// 函数的参数列表
        /// </summary>
        public ExpressionList Arguments { get; set; }
        public override string ToString()
        {
            return $"函数:{Name} {Arguments.ToString()}";
        }
    }
    /// <summary>
    /// 实现该接口表示一个表达式
    /// </summary>
    public interface IExpression : IValue
    {
        bool Alone { get; set; }
    }
    /// <summary>
    /// 一元操作符
    /// </summary>
    public enum UnaryOperator
    {
        /// <summary>
        /// 逻辑非
        /// </summary>
        Not,
        /// <summary>
        /// 波浪符，按位取反
        /// </summary>
        Tilde,
        /// <summary>
        /// 负数
        /// </summary>
        Minus,
        /// <summary>
        /// 正数
        /// </summary>
        Plus
    }
    /// <summary>
    /// 一元表达式
    /// </summary>
    public class UnaryExpression : IExpression
    {
        public UnaryOperator Operator { get; set; }
        public IValue Value { get; set; }
        public bool Alone { get => true; set { } }

        public override string ToString()
        {
            string opr;
            switch (Operator)
            {
                case UnaryOperator.Not:
                    opr = "!";
                    break;
                case UnaryOperator.Tilde:
                    opr = "~";
                    break;
                case UnaryOperator.Minus:
                    opr = "-";
                    break;
                case UnaryOperator.Plus:
                    opr = "+";
                    break;
                default:
                    opr = "[unknow]";
                    break;
            }
            return $"{opr}{Value.ToString()}";
        }
    }
    /// <summary>
    /// 二元运算符 
    /// </summary>
    public enum BinaryOperator
    {
        /// <summary>
        /// 表示该操作符不是二元运算符
        /// </summary>
        None,
        /// <summary>
        /// 小数点.属性/函数运算符
        /// </summary>
        Dot,
        /// <summary>
        /// 加+
        /// </summary>
        Plus,
        /// <summary>
        /// 减-（注意，求负运算符也当作是二元运算符，在二元表达式中，如果操作符为减号，左操作数为null则该表达式为求负表达式）
        /// </summary>
        Minus,
        /// <summary>
        /// 乘*
        /// </summary>
        Multiply,
        /// <summary>
        /// 除/
        /// </summary>
        Divide,
        /// <summary>
        /// 取模%
        /// </summary>
        Mod,
        /// <summary>
        /// 等于
        /// </summary>
        Equals,
        /// <summary>
        /// 不等于
        /// </summary>
        NotEquals,
        /// <summary>
        /// 大于
        /// </summary>
        GreaterThan,
        /// <summary>
        /// 大于等于
        /// </summary>
        GreaterEquals,
        /// <summary>
        /// 小于
        /// </summary>
        LessThan,
        /// <summary>
        /// 小于等于
        /// </summary>
        LessEquals,
        /// <summary>
        /// &
        /// </summary>
        And,
        /// <summary>
        /// |
        /// </summary>
        Or,
        /// <summary>
        /// &&
        /// </summary>
        AndAlso,
        /// <summary>
        /// ||
        /// </summary>
        OrAlso,
        /// <summary>
        /// IN(PARAS)
        /// </summary>
        In,
        /// <summary>
        /// like模糊查询
        /// </summary>
        Like,
        /// <summary>
        /// IS类型断言
        /// </summary>
        Is
    }
    /// <summary>
    /// 二元表达式
    /// </summary>
    public class BinaryExpression
        : IExpression
    {
        /// <summary>
        /// 表达式左边的结果
        /// </summary>
        public IValue Left { get; set; }
        /// <summary>
        /// 表达式右边的结果
        /// </summary>
        public IValue Right { get; set; }
        /// <summary>
        /// 运算符
        /// </summary>
        public BinaryOperator Operator { get; set; }
        /// <summary>
        /// 该表达式在逻辑上是否是独立的
        /// </summary>
        public bool Alone { get; set; }

        public override string ToString()
        {
            return $"{(Alone ? "(" : "(")}{Left.ToString()} {GetOperatorString(Operator)} {Right}{(Alone ? ")" : ")")}";

        }

        public static string GetOperatorString(BinaryOperator opr)
        {
            switch (opr)
            {
                case BinaryOperator.None:
                    return "[None]";
                case BinaryOperator.Dot:
                    return ".";
                case BinaryOperator.Plus:
                    return "+";
                case BinaryOperator.Minus:
                    return "-";
                case BinaryOperator.Multiply:
                    return "*";
                case BinaryOperator.Divide:
                    return "/";
                case BinaryOperator.Mod:
                    return "%";
                case BinaryOperator.Equals:
                    return "==";
                case BinaryOperator.NotEquals:
                    return "!=";
                case BinaryOperator.GreaterThan:
                    return ">";
                case BinaryOperator.GreaterEquals:
                    return ">=";
                case BinaryOperator.LessThan:
                    return "<";
                case BinaryOperator.LessEquals:
                    return "<=";
                case BinaryOperator.And:
                    return "&";
                case BinaryOperator.Or:
                    return "|";
                case BinaryOperator.AndAlso:
                    return "&&";
                case BinaryOperator.OrAlso:
                    return "||";
                case BinaryOperator.In:
                    return "in";
                case BinaryOperator.Like:
                    return "like";
                case BinaryOperator.Is:
                    return "is";
                default:
                    return "[unknow]";
            }
        }

        public static BinaryOperator GetOperator(string sign)
        {
            switch (sign)
            {
                case ".":
                    return BinaryOperator.Dot;
                case "+":
                    return BinaryOperator.Plus;
                case "-":
                    return BinaryOperator.Minus;
                case "*":
                    return BinaryOperator.Multiply;
                case "/":
                    return BinaryOperator.Divide;
                case "%":
                    return BinaryOperator.Mod;
                case "==":
                    return BinaryOperator.Equals;
                case "!=":
                case "<>":
                    return BinaryOperator.NotEquals;
                case ">":
                    return BinaryOperator.GreaterThan;
                case ">=":
                    return BinaryOperator.GreaterEquals;
                case "<":
                    return BinaryOperator.LessThan;
                case "<=":
                    return BinaryOperator.LessEquals;
                case "&":
                case "and":
                    return BinaryOperator.And;
                case "|":
                case "or":
                    return BinaryOperator.Or;
                case "&&":
                    return BinaryOperator.AndAlso;
                case "||":
                    return BinaryOperator.OrAlso;
                case "in":
                    return BinaryOperator.In;
                case "like":
                    return BinaryOperator.Like;
                case "is":
                    return BinaryOperator.Is;
                default:
                    //ex("暂不支持操作符：[" + t.Content + "]。");
                    return BinaryOperator.None;
            }
        }
    }
    ///// <summary>
    ///// 条件表达式
    ///// </summary>
    //public class ConditionExpression : IExpression
    //{
    //    private readonly Dictionary<IValue, IValue> dic;
    //    public ConditionExpression()
    //    {
    //        dic = new Dictionary<IValue, IValue>();
    //    }
    //    /// <summary>
    //    /// 判断条件
    //    /// </summary>
    //    public IValue Condition { get; set; }
    //    public bool Alone { get => true; set { } }

    //    public Dictionary<IValue, IValue>.Enumerator GetConditionParts()
    //    {
    //        return dic.GetEnumerator();
    //    }

    //    public void PushConditionPart(IValue key, IValue value)
    //    {
    //        dic.Add(key, value);
    //    }


    //}
    /// <summary>
    /// 表达式列表
    /// </summary>
    public class ExpressionList : IExpression
    {
        private readonly List<IValue> list;

        public bool Alone { get; set; }

        public ExpressionList()
        {
            list = new List<IValue>();
        }
        public void Push(IValue value)
        {
            list.Add(value);
        }

        public List<IValue> GetList()
        {
            return list;
        }

        public override string ToString()
        {
            var strs = new List<string>(list.Count);
            foreach (var item in list)
            {
                strs.Add(item.ToString());
            }
            return $"({string.Join(",", strs.ToArray())})";
        }
    }

}
