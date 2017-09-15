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
    using GSQL;

    /// <summary>
    /// 用于管理数据库连接，以及提供一套同一的数据库操作方法
    /// </summary>
    public abstract class Database : IDisposable
    {
        #region Base
        private DbTransaction transaction = null;//当前正在执行的事务
        private readonly
            DbConnection conn = null;
        private DbProviderFactory factory = null;
        protected Database(DbProviderFactory p_factory, string p_connstr) : this(p_factory)
        {
            conn.ConnectionString = p_connstr;
        }
        protected Database(DbProviderFactory p_factory)
        {
            factory = p_factory;
            conn = factory.CreateConnection();

            sqlTextGenerator = SQLTextGenerator();
            if (sqlTextGenerator==null)
            {
                throw new Exception("当前数据库对象未指定SQLTextGenerator。");
            }
        }
        protected readonly SQLTextGenerator sqlTextGenerator;
        /// <summary>
        /// 在派生类中实现为当前数据库生成SQL脚本
        /// </summary>
        /// <returns></returns>
        protected abstract SQLTextGenerator SQLTextGenerator(); 
        /// <summary>
        /// 根据指定的数据库类型名称和连接字符串创建数据库实例。
        /// </summary>
        /// <param name="dbtype"></param>
        /// <param name="connstr"></param>
        /// <returns></returns>
        public static Database Create(string dbtype, string connstr)
        {
            var t = Type.GetType(dbtype);
            if (t == null)
            {
                throw new NotSupportedException($"database type \"{dbtype}\" not founded.please check the typename.");
            }
            var ci = t.GetConstructor(new Type[] { typeof(string) });
            var obj = ci.Invoke(new object[] { connstr });
            return (Database)obj;
        }

        /// <summary>
        /// 获取或设置当前数据库的连接字符串
        /// </summary>
        public string ConnectionString
        {
            get { return conn.ConnectionString; }
            set { conn.ConnectionString = value; }
        }

        public int ConnectionTimeout { get { return conn.ConnectionTimeout; } }
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
        protected DbCommand CreateCommand(string sql)
        {
            DbCommand cmd = CreateCommand();
            cmd.CommandText = sql;
            return cmd;
        }
        protected virtual DbParameter processParameter(DbCommand cmd, string name, object value)
        {
            if (!name.StartsWith("@"))
            {
                name = "@" + name;
            }

            DbParameter para = cmd.CreateParameter();
            para.ParameterName = name;
            if (value is LikeParameter lp)
            {
                string temp = "{0}";
                switch (lp.Mode)
                {
                    case LikeMode.Contains:
                        temp = "%{0}%";
                        break;
                    case LikeMode.StartWith:
                        temp = "{0}%";
                        break;
                    case LikeMode.EndWith:
                        temp = "%{0}";
                        break;
                    default:
                        break;
                }

                para.Value = string.Format(temp, lp.Value);
            }
            else if (value is InParameter inp)
            {
                List<string> l = new List<string>();
                //var setting = Setting();
                //foreach (var item in inp.Paras)
                //{
                //    switch (item)
                //    {
                //        case SByte sbyt:
                //        case Byte byt:
                //        case ushort usho:
                //        case short sho:
                //        case int i:
                //        case uint ui:
                //        case long lon:
                //        case ulong ulon:
                //        case float f:
                //        case double dou:
                //        case decimal dec:
                //            l.Add(setting.Numeric(item.ToString()));
                //            break;
                //        case bool boo:
                //            l.Add(setting.Boolean(boo ? (IBoolean)True.Value : (IBoolean)False.Value));
                //            break;
                //        case string str:
                //            l.Add(setting.String(str));
                //            break;
                //        default:
                //            l.Add(setting.String(item.ToString()));
                //            break;
                //    }
                //}
                para.Value = $"({string.Join(",", l.ToArray())})";
            }
            else
            {
                para.Value = value;
            }
            return para;
        }
        protected DbCommand CreateCommand(string sql, IDictionary<string, object> paras)
        {
            DbCommand cmd = CreateCommand(sql);
            if (paras != null)
            {
                foreach (var item in paras)
                {
                    string pname = item.Key;
                    cmd.Parameters.Add(processParameter(cmd, pname, item.Value));
                }
            }
            return cmd;
        }
        protected DbCommand CreateCommand(string sql, object paras)
        {
            if (paras == null)
            {
                return CreateCommand(sql);
            }
            else if (paras is IDictionary<string, object>)
            {
                return CreateCommand(sql, (IDictionary<string, object>)paras);
            }
            else if (paras is JObject)
            {
                return CreateCommand(sql, (JObject)paras);
            }
            DbCommand cmd = CreateCommand(sql);
            if (paras != null)
            {
                Type t = paras.GetType();
                var pis = t.GetRuntimeProperties();
                foreach (var pi in pis)
                {
                    object value = pi.GetValue(paras);
                    string pname = pi.Name;
                    cmd.Parameters.Add(processParameter(cmd, pname, value));
                }
            }
            return cmd;
        }
        protected DbCommand CreateCommand(string sql, JObject paras)
        {
            DbCommand cmd = CreateCommand(sql);
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
                    string pname = pi.Name;
                    cmd.Parameters.Add(processParameter(cmd, pname, value));
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
            if (conn.State != ConnectionState.Open)
            {
                conn.Open();
            }
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
        public int ExecuteNonQuery(GSQLCommandText gsql, object paras = null)
        {
            return ExecuteNonQuery(gsql.ToSql(sqlTextGenerator), paras);
        }
        [System.Diagnostics.DebuggerStepThrough]
        public int ExecuteNonQuery(SQLCommandText sql, object paras = null)
        {
            var cmd = CreateCommand(sql, paras);
            Open();
            int result = cmd.ExecuteNonQuery();

            if (ClosWhenExecuted)
            {
                Close();
            }
            return result;
        }

        public T ExecuteScalar<T>(GSQLCommandText gsql, object paras = null)
        {
            var obj = ExecuteScalar(gsql.ToSql(sqlTextGenerator), paras);
            return SqlUtility.ConvertToTargetType<T>(obj);
        }
        public T ExecuteScalar<T>(SQLCommandText sql, object paras = null)
        {
            var cmd = CreateCommand(sql, paras);
            Open();
            object result = cmd.ExecuteScalar();

            if (ClosWhenExecuted)
            {
                Close();
            }
            var obj = SqlObject2CLIObject(result);
            return SqlUtility.ConvertToTargetType<T>(obj);
        }
        public object ExecuteScalar(GSQLCommandText gsql, object paras = null)
        {
            return ExecuteScalar(gsql.ToSql(sqlTextGenerator), paras);
        }
        [System.Diagnostics.DebuggerStepThrough]
        public object ExecuteScalar(SQLCommandText sql, object paras = null)
        {
            var cmd = CreateCommand(sql, paras);
            Open();
            object result = cmd.ExecuteScalar();

            if (ClosWhenExecuted)
            {
                Close();
            }
            return SqlObject2CLIObject(result);
        }


        public DbDataReader ExecuteReader(GSQLCommandText gsql, object paras = null)
        {
            return ExecuteReader(gsql.ToSql(sqlTextGenerator), paras);
        }
        [System.Diagnostics.DebuggerStepThrough]
        public DbDataReader ExecuteReader(SQLCommandText sql, object paras = null)
        {
            var cmd = CreateCommand(sql, paras);
            Open();
            DbDataReader reader = cmd.ExecuteReader();

            if (ClosWhenExecuted)
            {
                Close();
            }
            return reader;

        }
        public int ExecuteInt32(GSQLCommandText gsql, object paras = null)
        {
            return ExecuteInt32(gsql.ToSql(sqlTextGenerator), paras);
        }
        public int ExecuteInt32(SQLCommandText sql, object paras = null)
        {
            var obj = ExecuteScalar(sql, paras);
            return Convert.ToInt32(obj);
        }
        public string ExecuteString(GSQLCommandText gsql, object paras = null)
        {
            return ExecuteString(gsql.ToSql(sqlTextGenerator), paras);
        }
        public string ExecuteString(SQLCommandText sql, object paras = null)
        {
            var obj = ExecuteScalar(sql, paras);
            return Convert.ToString(obj);
        }
        public JArray ExecuteJArray(GSQLCommandText gsql, object paras = null)
        {
            return ExecuteJArray(gsql.ToSql(sqlTextGenerator), paras);
        }

        public T[] ExecuteArray<T>(GSQLCommandText gsql, object paras = null)
        {
            using (DbDataReader reader = ExecuteReader(gsql, paras))
            {
                var colcount = reader.FieldCount;
                List<T> l = new List<T>(colcount);
                while (reader.Read())
                {

                    object value = reader.GetValue(0);
                    value = SqlObject2CLIObject(value);
                    var tval = SqlUtility.ConvertToTargetType<T>(value);
                    l.Add(tval);
                }
                return l.ToArray();
            }
        }
        public Dictionary<TKey, TValue> ExecuteDictionary<TKey, TValue>(GSQLCommandText gsql, string key, string value, object paras = null)
        {
            using (DbDataReader reader = ExecuteReader(gsql, paras))
            {
                var colcount = reader.FieldCount;
                Dictionary<TKey, TValue> dic = new Dictionary<TKey, TValue>(colcount);
                int keyindex = 0;
                int valueindex = 0;
                for (int i = 0; i < colcount; i++)
                {
                    string colname = reader.GetName(i);
                    if (key == colname)
                    {
                        keyindex = i;
                    }
                    else if (value == colname)
                    {
                        valueindex = i;
                    }
                }
                while (reader.Read())
                {

                    object keyv = reader.GetValue(keyindex);
                    object valuev = reader.GetValue(valueindex);
                    keyv = SqlObject2CLIObject(keyv);
                    valuev = SqlObject2CLIObject(valuev);
                    TKey tkeyv = SqlUtility.ConvertToTargetType<TKey>(keyv);
                    TValue tvaluev = SqlUtility.ConvertToTargetType<TValue>(valuev);
                    dic.Add(tkeyv, tvaluev);
                }
                return dic;
            }
        }
        /// <summary>
        /// 执行SQL语句，将查询结果作为JSON数组返回。
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="paras"></param>
        /// <returns></returns>
        public JArray ExecuteJArray(SQLCommandText sql, object paras = null)
        {
            JArray array = new JArray();
            using (DbDataReader reader = ExecuteReader(sql, paras))
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
        public JObject ExecuteJObject(GSQLCommandText gsql, object paras = null)
        {
            return ExecuteJObject(gsql.ToSql(sqlTextGenerator), paras);
        }
        public T ExecuteObject<T>(GSQLCommandText gsql, object paras = null)
        {
            var jobj = ExecuteJObject(gsql, paras);
            string sertext = JsonConvert.SerializeObject(jobj);
            return JsonConvert.DeserializeObject<T>(sertext);
        }
        /// <summary>
        /// 执行SQL语句，将查询结果作为JSON对象返回。
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="paras"></param>
        /// <returns></returns>
        public JObject ExecuteJObject(SQLCommandText sql, object paras = null)
        {
            using (DbDataReader reader = ExecuteReader(sql, paras))
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

        #region 查询扩展
        /// <summary>
        /// 分页查询
        /// </summary>
        /// <param name="select"></param>
        /// <returns></returns>
        public virtual SqlPagination ExecutePagination(GSQLCommandText select, int onepagerowscount, int curpageindex, object paras = null)
        {
            //var gs = GSqlSetting.Default;
            //GSQLParser p1 = new GSQLParser((string)select);
            //var sel = p1.ReadSelect();
            //var selcount = $"select count(1) from ({GSqlSetting.Default.Select(sel)}) AS TEMP;";
            //var count = ExecuteInt32((GSQLCommandText)selcount, paras);
            //var pagecount = (int)Math.Ceiling((decimal)count / (decimal)onepagerowscount);
            //int limitstart = curpageindex * onepagerowscount;
            //sel.Limit = new LimitClause() { Offset = limitstart, Length = onepagerowscount };
            //SqlPagination pagi = new SqlPagination() { CurrentPage = curpageindex, PageCount = pagecount };
            //pagi.PageData = ExecuteJArray((GSQLCommandText)GSqlSetting.Default.Select(sel), paras);
            //return pagi;
            return null;
        }
        /// <summary>
        /// 查询结果是否存在
        /// </summary>
        /// <param name="select"></param>
        /// <param name="paras"></param>
        /// <returns></returns>
        public virtual bool ExecuteExists(GSQLCommandText select, object paras = null)
        {
            //GSQLParser p1 = new GSQLParser((string)select);
            //var sel = p1.ReadSelect();
            //sel.Fields = new QueryFieldList();
            //var field = new Function() { Name = "COUNT", Arguments = new ExpressionList() { Args = new List<IValue>() { new Constant() { Type = ConstantType.Numeric, Content = "1" } } } };
            //sel.Fields.Add(field);
            //int count = ExecuteInt32((GSQLCommandText)GSqlSetting.Default.Select(sel), paras);
            //return count >= 1;
            return false;
        }

        #endregion

        public abstract Database Clone();
        public void Dispose()
        {
            this.Close();
            this.conn?.Dispose();
        }
    }
}
