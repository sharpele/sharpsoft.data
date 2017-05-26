using System;
using System.Collections.Generic;
using System.Text;

namespace SharpSoft.Data
{
    /// <summary>
    /// 数据库中的对象基类
    /// </summary>
    public abstract class DataObject
    {
        public DataObject(string name, string alias = null)
        {
            Name = name;

        }
        /// <summary>
        /// 固定名称
        /// </summary>
        public string Name { get; set; }
        private string _Alias;
        /// <summary>
        /// 别名,如果未设置别名则默认别名与固定名称一致。
        /// </summary>
        public virtual string Alias
        {
            get
            {
                if (string.IsNullOrEmpty(_Alias))
                {
                    return Name;
                }
                return _Alias;
            }
            set { _Alias = value; }
        }

        public override string ToString()
        {
            return Alias;
        }

        public static implicit operator string(DataObject dobj)
        {
            if (dobj==null)
            {
                return null;
            }
            return dobj.ToString();
        }

    }
    /// <summary>
    /// 描述数据表的相关信息
    /// </summary>
    public class Table : DataObject
    {
        public Table(string name, string alias = null) : base(name, alias)
        {
        }
    }
    /// <summary>
    /// 描述字段的相关信息
    /// </summary>
    public class Field : DataObject
    {
        public Field(string name, string alias = null) : base(name, alias)
        {
        }
    }
    ///// <summary>
    ///// 描述函数的相关信息
    ///// </summary>
    //public class Function : DataObject
    //{
    //    public Function(string name) : base(name, null)
    //    {
    //    }
    //    public override string Alias { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    //    //TODO:
    //    public override string ToString()
    //    {
    //        return base.ToString();
    //    }
    //}
}
