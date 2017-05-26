using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SharpSoft.Data
{
    /// <summary>
    /// 用于管理数据库连接，以及提供一套同一的数据库操作方法
    /// </summary>
    public abstract class Database
    {
        #region Base
        private DbTransaction transaction = null;//当前正在执行的事务
        private DbConnection conn = null;
        private DbProviderFactory factory = null;
        public Database(DbProviderFactory p_factory, string p_connstr) : this(p_factory)
        {
            conn.ConnectionString = p_connstr;
        }
        public Database(DbProviderFactory p_factory)
        {
            factory = p_factory;
            conn = factory.CreateConnection();
        }
        public abstract TSqlSetting Setting();

        /// <summary>
        /// 获取或设置当前数据库的连接字符串
        /// </summary>
        public string ConnectionString
        {
            get { return conn.ConnectionString; }
            set { conn.ConnectionString = value; }
        }
        /// <summary>
        /// 是否在执行完毕后关闭数据连接
        /// </summary>
        public bool ClosWhenExecuted { get; set; } = false;

        /// <summary>
        /// 获取数据库工厂实例
        /// </summary>
        public virtual DbProviderFactory Factory
        {
            get { return factory; }
        }
        /// <summary>
        /// 获取数据库的实例
        /// </summary>
        /// <param name="p_invariantname">数据库类别的固定名称</param>
        /// <param name="p_connstr">连接字符串</param>
        /// <returns></returns>
        public static Database GetDatabase(string p_invariantname, string p_connstr)
        {
            if (p_invariantname == null)
            {
                throw new ArgumentNullException("p_invariantname");
            }
            switch (p_invariantname.ToLower())
            {
                case "mssql":
                case "System.Data.SqlClient":
                    return new MsSql(p_connstr);
                default:
                    throw new ArgumentException("无法识别的数据库类型名称：[" + p_invariantname + "]。");
            }
        }

        /// <summary>
        /// 打开数据连接（如果当前数据连接已关闭）
        /// </summary>
        public void Open()
        {
            if (conn.State == ConnectionState.Closed)
                conn.Open();
        }
        /// <summary>
        /// 关闭数据连接（如果当前数据连接不是关闭状态）
        /// </summary>
        public void Close()
        {
            if (conn.State != ConnectionState.Closed)
                conn.Close();
        }
        private DbCommand CreateCommand()
        {
            DbCommand cmd = conn.CreateCommand();
            if (transaction != null)
            {
                cmd.Transaction = transaction;
            }
            return cmd;
        }
        protected DbCommand CreateCommand(string p_cmdstr)
        {
            DbCommand cmd = CreateCommand();
            cmd.CommandText = p_cmdstr;
            return cmd;
        }
        protected DbCommand CreateCommand(string p_cmdstr, IDictionary<string, object> paras)
        {
            DbCommand cmd = CreateCommand(p_cmdstr);
            if (paras != null)
            {
                foreach (var item in paras)
                {
                    string pname = item.Key;
                    if (pname.StartsWith("@"))
                    {
                        pname = "@" + pname;
                    }
                    DbParameter para = cmd.CreateParameter();
                    para.ParameterName = pname;
                    para.Value = item.Value;
                    cmd.Parameters.Add(para);
                }
            }
            return cmd;
        }
        protected DbCommand CreateCommand(string p_cmdstr, object paras)
        {
            if (paras == null)
            {
                return CreateCommand(p_cmdstr);
            }
            else if (paras is IDictionary<string, object>)
            {
                return CreateCommand(p_cmdstr, (IDictionary<string, object>)paras);
            }
            else if (paras is JObject)
            {
                return CreateCommand(p_cmdstr, (JObject)paras);
            }
            DbCommand cmd = CreateCommand(p_cmdstr);
            if (paras != null)
            {
                Type t = paras.GetType();
                var pis = t.GetRuntimeProperties();
                foreach (var pi in pis)
                {
                    object value = pi.GetValue(paras);
                    string name = pi.Name;
                    DbParameter para = cmd.CreateParameter();
                    string pname = "@" + name;
                    para.ParameterName = pname;
                    para.Value = value;
                    cmd.Parameters.Add(para);
                }
            }
            return cmd;
        }
        protected DbCommand CreateCommand(string p_cmdstr, JObject paras)
        {
            DbCommand cmd = CreateCommand(p_cmdstr);
            if (paras != null)
            {

                var pis = paras.Properties();
                foreach (var pi in pis)
                {
                    var jvalue = pi.Value;
                    object value = null;
                    if (jvalue != null)
                    {
                        value = jvalue.ToObject(typeof(object));
                    }
                    string name = pi.Name;
                    DbParameter para = cmd.CreateParameter();
                    string pname = "@" + name;
                    para.ParameterName = pname;
                    para.Value = value;
                    cmd.Parameters.Add(para);
                }
            }
            return cmd;
        }
        /// <summary>
        /// 开始执行事务,事务提交/回滚后需要释放该对象。
        /// </summary>
        /// <param name="isolationLevel"></param>
        /// <returns></returns>
        public TransactionWarpper BeginTransaction(IsolationLevel isolationLevel = IsolationLevel.Unspecified)
        {
            DbTransaction tran = conn.BeginTransaction(isolationLevel);
            transaction = tran;
            TransactionWarpper warpper = new TransactionWarpper(tran, delegate { transaction = null; });
            return warpper;
        }

        /// <summary>
        /// 将SQL对象转换为CLI运行时对象
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        protected virtual object SqlObject2CLIObject(object obj)
        {
            if (obj is DBNull)
            {
                return null;
            }
            return obj;
        }
        #endregion

        #region 查询
        public int ExecuteNonQuery(string p_cmdstr, object paras = null)
        {
            var cmd = CreateCommand(p_cmdstr, paras);
            Open();
            int result = cmd.ExecuteNonQuery();

            if (ClosWhenExecuted)
            {
                Close();
            }
            return result;
        }

        public object ExecuteScalar(string p_cmdstr, object paras = null)
        {
            var cmd = CreateCommand(p_cmdstr, paras);
            Open();
            object result = cmd.ExecuteScalar();

            if (ClosWhenExecuted)
            {
                Close();
            }
            return SqlObject2CLIObject(result);
        }


        public DbDataReader ExecuteReader(string p_cmdstr, object paras = null)
        {
            var cmd = CreateCommand(p_cmdstr, paras);
            Open();
            DbDataReader reader = cmd.ExecuteReader();

            if (ClosWhenExecuted)
            {
                Close();
            }
            return reader;

        }
        public int ExcuteInt32(string p_cmdstr, object paras = null)
        {
            var obj = ExecuteScalar(p_cmdstr, paras);
            return Convert.ToInt32(obj);
        }
        public string ExcuteString(string p_cmdstr, object paras = null)
        {
            var obj = ExecuteScalar(p_cmdstr, paras);
            return Convert.ToString(obj);
        }
        /// <summary>
        /// 执行SQL语句，将查询结果作为JSON数组返回。
        /// </summary>
        /// <param name="p_cmdstr"></param>
        /// <param name="paras"></param>
        /// <returns></returns>
        public JArray ExecuteJArray(string p_cmdstr, object paras = null)
        {
            JArray array = new JArray();
            using (DbDataReader reader = ExecuteReader(p_cmdstr, paras))
            {
                var colcount = reader.FieldCount;
                while (reader.Read())
                {
                    JObject obj = new JObject();
                    for (int i = 0; i < colcount; i++)
                    {
                        string colname = reader.GetName(i);
                        object value = reader.GetValue(i);
                        value = SqlObject2CLIObject(value);
                        obj.Add(colname, new JValue(value));
                    }
                    array.Add(obj);
                }
                return array;
            }
        }
        /// <summary>
        /// 执行SQL语句，将查询结果作为JSON对象返回。
        /// </summary>
        /// <param name="p_cmdstr"></param>
        /// <param name="paras"></param>
        /// <returns></returns>
        public JObject ExecuteJObject(string p_cmdstr, object paras = null)
        {
            using (DbDataReader reader = ExecuteReader(p_cmdstr, paras))
            {
                var colcount = reader.FieldCount;
                while (reader.Read())
                {
                    JObject obj = new JObject();
                    for (int i = 0; i < colcount; i++)
                    {
                        string colname = reader.GetName(i);
                        object value = reader.GetValue(i);
                        value = SqlObject2CLIObject(value);
                        obj.Add(colname, new JValue(value));
                    }
                    return obj;
                }
                return null;
            }
        }
        #endregion
    }
}
