using System;
using System.Collections.Generic;
using System.Text;

namespace SharpSoft.Data
{
    /// <summary>
    /// 用于将SQL语句对象翻译到各数据库适应的SQL语句
    /// </summary>
    public abstract class SQLGenerater
    {
        /// <summary>
        /// 获取安全名称，避免与系统关键字重复
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public string GetSafeName(string target)
        {
            return safe(target);
        }
        /// <summary>
        /// 获取参数的占位符形式
        /// </summary>
        /// <param name="paraname"></param>
        /// <returns></returns>
        public string GetParameterPlaceholder(string paraname)
        {
            return placeholder(paraname);
        }
        /// <summary>
        /// 在派生类中重写，提供参数的占位符形式
        /// </summary>
        /// <param name="paraname"></param>
        /// <returns></returns>
        protected virtual string placeholder(string paraname)
        {
            return string.Concat("@", paraname);
        }
        /// <summary>
        /// 在派生类中重写，提供目标数据库系统的安全名称形式。
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        protected virtual string @safe(string target)
        {
            return string.Concat("[", target, "]");
        }
        /// <summary>
        /// 生成Select语句
        /// </summary>
        /// <param name="select"></param>
        /// <returns></returns>
        public abstract string GenerateSelect(Select select);
        /// <summary>
        /// 生成Update语句
        /// </summary>
        /// <param name="update"></param>
        /// <returns></returns>
        public abstract string GenerateUpdate(Update update);
        /// <summary>
        /// 色回暖过程Insert语句
        /// </summary>
        /// <param name="insert"></param>
        /// <returns></returns>
        public abstract string GenerateInsert(Insert insert);
        /// <summary>
        /// 生成Delete语句
        /// </summary>
        /// <param name="delete"></param>
        /// <returns></returns>
        public abstract string GenerateDelete(Delete delete);
    }
}
