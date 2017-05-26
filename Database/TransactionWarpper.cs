using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;

namespace SharpSoft.Data
{
    /// <summary>
    /// 事务包装
    /// </summary>
    public class TransactionWarpper : IDisposable
    {
        DbTransaction tran = null;
        Action whendispose = null;
        internal TransactionWarpper(DbTransaction p_tran, Action p_whendispose)
        {
            tran = p_tran;
            whendispose = p_whendispose;
        }
        /// <summary>
        /// 提交事务
        /// </summary>
        public void Commit()
        {
            tran.Commit();
        }
        /// <summary>
        /// 回滚事务
        /// </summary>
        public void Rollback()
        {
            tran.Rollback();
        }
        ~TransactionWarpper()
        {
            if (tran != null)
            {
                tran.Dispose();
                tran = null;
            }
        }
        public void Dispose()
        {
            whendispose?.Invoke();
            tran.Dispose();
            tran = null;
        }
    }
}
