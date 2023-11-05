using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;

namespace Dapper
{
    public static class DynamicParameterExtension
    {
        /// <summary>
        /// sql:"@0,@1"
        /// new DynamicParameters().Set("p0", "p1");
        /// </summary>
        /// <param name="ps"></param>
        /// <returns></returns>
        public static DynamicParameters Set(this DynamicParameters d, params object[] ps)
        {
            for (int i = 0, len = ps.Length; i < len; i++)
                d.Add($"{i}", ps[i]);
            return d;
        }

        /// <summary>
        /// new DynamicParameters().Set(new Dicitory<string, object>{ ["k"] = 0 })
        /// </summary>
        /// <param name="kvs"></param>
        /// <returns></returns>
        public static DynamicParameters Set(this DynamicParameters d, IEnumerable<KeyValuePair<string, object>> kvs)
        {
            foreach (var kv in kvs)
                d.Add(kv.Key, kv.Value);
            return d;
        }

        /// <summary>
        /// new DynamicParameters().Set(("k", 0), ("id", 1))
        /// </summary>
        /// <param name="kvs"></param>
        /// <returns></returns>
        public static DynamicParameters Set(this DynamicParameters d, params (string Key, object Value)[] kvs)
        {
            foreach (var kv in kvs)
                d.Add(kv.Key, kv.Value);
            return d;
        }

        /// <summary>
        /// var dyp = new DynamicParameters().Set("0", null).Set("a", 1)
        ///     .Set("i2", direction: ParameterDirection.Output, dbType: DbType.String, size: 100);
        /// </summary>
        public static DynamicParameters Set(this DynamicParameters parameters, string name, object value = null, DbType? dbType = null, ParameterDirection? direction = null, int? size = null, byte? precision = null, byte? scale = null)
        {
            parameters.Add(name, value, dbType, direction, size, precision, scale);
            return parameters;
        }
        
        /// <summary>
        /// dp = new DynamicParameters(XXX); dp在执行sql时才会从里面templete复制出来
        /// </summary>
        public static DynamicParameters Set(this DynamicParameters dp, object parameters)
        {
            switch (parameters)
            {
                case IEnumerable<KeyValuePair<string, object>> kvs:
                    foreach (var kv in kvs)
                        dp.Add(kv.Key, kv.Value);
                    break;
                case IEnumerable<(string Key, object Value)> kvs:
                    foreach (var kv in kvs)
                        dp.Add(kv.Key, kv.Value);
                    break;
                default:
                    {
                        var pis = parameters.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
                        foreach (var pi in pis)
                            dp.Add(pi.Name, pi.GetValue(parameters, null));
                    }
                    break;
            }
            return dp;
        }

        public static object Get(this DynamicParameters d, string name)
        {
            return d.Get<object>(name); //d.Get<T>(...)是直接强制转换
        }
    }
}
