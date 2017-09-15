using System;
using System.Collections.Generic;
using System.Text;

namespace SharpSoft.Data
{
    public static class SqlUtility
    {
        /// <summary>
        /// 将数据库查询出的数据转换为指定的强类型
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static T ConvertToTargetType<T>(object obj)
        {
            Type t = typeof(T);
            if (obj == null || obj == DBNull.Value)
            {
                return default(T);
            }

            if ((t.IsConstructedGenericType && t.
      GetGenericTypeDefinition().Equals
      (typeof(Nullable<>))))
            {
                var types = t.GenericTypeArguments;
                if (types.Length == 1)
                {
                    t = types[0];
                }
                else
                {
                    throw new Exception("无法处理带有多个类型参数的泛型。");
                }
            }
            return (T)Convert.ChangeType(obj, t);
        }
    }
}
