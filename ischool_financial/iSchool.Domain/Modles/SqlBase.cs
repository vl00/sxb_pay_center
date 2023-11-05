using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Domain.Modles
{
    /// <summary>
    /// sql 基础类
    /// </summary>
    public class SqlBase
    {

        /// <summary>
        /// sql语句集合
        /// </summary>
        public List<string> Sqls { get; set; }

        /// <summary>
        /// sql参数集合
        /// </summary>
        public List<object> SqlParams { get; set; }
    }

    public class SqlSingle
    {

        /// <summary>
        /// sql语句
        /// </summary>
        public string Sql { get; set; }

        /// <summary>
        /// sql参数
        /// </summary>
        public object SqlParam { get; set; }
    }
}
