using System;
using System.Collections.Generic;
using System.Text;

namespace SharpSoft.Data
{
    /// <summary>
    /// 查询
    /// </summary>
    public class Select : IStatement, IValue//查询语句在某些情况下可以出现在表达式中
    {
        public Select()
        {

        }
        public QueryFieldList Fields { get; set; }

        public List<IValue> From { get; set; }

        public List<JoinClause> Joins { get; set; }
        public WhereClause Where { get; set; }

        public GroupByClause GroupBy { get; set; }

        public HavingClause Having { get; set; }

        public OrderByClause OrderBy { get; set; }


        public LimitClause Limit { get; set; }

        /// <summary>
        /// 合并查询
        /// </summary>
        public Union Unions { get; set; }
    }
    /// <summary>
    /// 合并查询
    /// </summary>
    public class Union
    {
        /// <summary>
        /// 是否允许重复值
        /// </summary>
        public bool UnionAll { get; set; } = false;
        /// <summary>
        /// 需要合并的另一个查询
        /// </summary>
        public Select OtherSelect { get; set; }

    }
    /// <summary>
    /// 子查询，子查询可作为结果集使用。
    /// </summary>
    public class SubSelect : Select
    {
        /// <summary>
        /// 为子查询结果集指定一个别名
        /// </summary>
        /// <param name="alias"></param>
        public SubSelect(Select sel, string alias)
        {
            Alias = alias;
            this.Fields = sel.Fields;
            this.From = sel.From;
            this.Joins = sel.Joins;
            this.Where = sel.Where;
            this.GroupBy = sel.GroupBy;
            this.Having = sel.Having;
            this.OrderBy = sel.OrderBy;
            this.Limit = sel.Limit;
            this.Unions = sel.Unions;

        }
        /// <summary>
        /// 子查询结果集的别名
        /// </summary>
        public string Alias { get; set; }
    }
    /// <summary>
    /// 实现此接口表示该对象可用作结果集
    /// </summary>
    public interface IResultSets : IResult
    {
        /// <summary>
        /// 结果集别名
        /// </summary>
        string Alias { get; set; }
    }
    /// <summary>
    /// 查询语句可用的子句
    /// </summary>
    public interface ISelectClause
    {

    }
    /// <summary>
    /// 提供用于查询结果的字段列表
    /// </summary>
    public class QueryFieldList : List<QueryField>
    {

    }
    /// <summary>
    /// 表示一个用于查询的字段，可以是字段名，也可以常量值、表达式等
    /// </summary>
    public class QueryField
    {
        public IValue Value { get; set; }

        public string Alias { get; set; }
    }
    /// <summary>
    /// 连接类型
    /// </summary>
    public enum JoinType
    {
        Inner,
        Left,
        Right,
        Full
    }
    /// <summary>
    /// JOIN子句
    /// </summary>
    public class JoinClause : ISelectClause
    {
        public JoinType Type { get; set; }
        public IValue Table { get; set; }
        public OnClause @On { get; set; }
    }
    /// <summary>
    /// join子句的on子句
    /// </summary>
    public class OnClause
    {
        public IValue Condition { get; set; }
    }
    /// <summary>
    /// GroupBy子句
    /// </summary>
    public class GroupByClause : ISelectClause
    {
        public List<Target> GroupFields { get; set; }
    }
    public enum OrderByType
    {
        Asc, Desc
    }
    public class OrderByField
    {
        public Target Field { get; set; }
        public OrderByType Type { get; set; }
    }
    /// <summary>
    /// OrderBy子句
    /// </summary>
    public class OrderByClause : ISelectClause
    {
        public List<OrderByField> OrderFields { get; set; }
    }
    /// <summary>
    /// Where子句
    /// </summary>
    public class WhereClause : ISelectClause
    {
        public IValue Condition { get; set; }
    }
    /// <summary>
    /// Having子句
    /// </summary>
    public class HavingClause : ISelectClause
    {
        public IValue Condition { get; set; }
    }
    /// <summary>
    /// limit子句
    /// </summary>
    public class LimitClause : ISelectClause
    {
        /// <summary>
        /// 起始位置
        /// </summary>
        public int Offset { get; set; }
        /// <summary>
        /// 长度为-1则表示此后所有记录
        /// </summary>
        public int Length { get; set; }
    }
}
