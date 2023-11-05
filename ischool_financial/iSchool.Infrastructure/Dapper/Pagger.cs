using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace iSchool.Infrastructure.Dapper
{
    /// <summary>
    /// sql分页帮助类
    /// </summary>
    public class Pagger
    {
        private string _sql = string.Empty;
        private string _sqlorder = string.Empty;
        private string _sqlcount = string.Empty;
        private bool _distinct = false;
        /// <summary>
        /// 查询语句
        /// </summary>
        public string Sql
        {
            set
            {
                if (string.IsNullOrEmpty(value))
                    throw new Exception("查询语句不能为空。");
                if (value.ToLower().IndexOf("select") != 0)
                    throw new Exception("查询语句必须以SELECT开头。");
                //找到最后一个ORDER BY子句
                int pos = value.ToLower().LastIndexOf("order by");
                if (pos < 0)
                    throw new Exception("查询语句没有指定ORDER BY子句。");
                pos += 8;
                //判断ORDER BY是否处在一个子查询中
                if (GetSubstringCount(value, "(", pos) < GetSubstringCount(value, ")", pos))
                    throw new Exception("查询语句没有指定ORDER BY子句。");
                //提取ORDER BY子句的内容
                _sqlorder = value.Substring(pos).Trim();
                //生成统计语句
                string countStr = string.Empty;
                countStr += @"(?<=^select\s)[^()]*"; //匹配查询字段开始
                countStr += @"(";                       //匹配子查询语句开始，开始查找括号
                countStr += @"\(((?<sub1>\()|(?<-sub1>\))|[^()])*(?(sub1)(?!))\)";          //匹配一个闭合的括号
                countStr += @"([^()]*\(((?<sub2>\()|(?<-sub2>\))|[^()])*(?(sub2)(?!))\))*"; //如果在闭合括号之后还有括号，则继续匹配
                countStr += @")?";                      //匹配子查询语句结束
                countStr += @"[^()]*(?=\sfrom)";    //匹配查询字符结束
                _sqlcount = Regex.Replace(value, countStr, "count(*)", RegexOptions.IgnoreCase | RegexOptions.Singleline);
                _sqlcount = Regex.Replace(_sqlcount, @" order by .+?$", "", RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.RightToLeft);
                //DISTINCT
                var dispos = value.ToLower().IndexOf("distinct");
                _distinct = dispos > 0;
                if (_distinct) value = value.Remove(dispos, 8);
                _sql = value;
            }
            get
            {
                return _sql;
            }
        }

        public Pagger(string strsql)
        {
            Sql = strsql;
        }

        #region 分页sql
        /// <summary>
        /// 得到分页sql语句
        /// 配合SqlBuilder使用 添加@pageindex，@pagesize 参数
        /// </summary>
        /// <returns></returns>
        public string GetPaggerSql()
        {
            string strsql = $"SELECT * FROM (SELECT " + (_distinct ? "DISTINCT" : "") + " TOP 100 PERCENT ROW_NUMBER() OVER (ORDER BY " + _sqlorder + ") Row," + _sql.Substring(7).Replace(" ORDER BY " + _sqlorder, string.Empty) + ") TMP WHERE Row> @pagesize * (@pageindex - 1)  AND Row<= @pagesize * @pageindex  ORDER BY Row";
            return strsql;
        }
        /// <summary>
        /// 得到sql2012分页语句
        /// 配合SqlBuilder使用 添加@pageindex，@pagesize 参数
        /// </summary>
        /// <returns></returns>
        public string GetPaggerSql2012()
        {
            string strsql = Sql + "OFFSET (@pageindex-1 * @pagesize) ROW FETCH NEXT @pagesize ROWS only; "; ;
            return strsql;
        }
        /// <summary>
        /// 得到分页sql
        /// </summary>
        /// <param name="pagesize">每页显示数量</param>
        /// <param name="pageindex">当前页 从1开始</param>
        /// <returns></returns>
        public string GetPaggerSql(int pagesize, int pageindex)
        {
            string strsql = "SELECT * FROM (SELECT TOP 100 PERCENT ROW_NUMBER() OVER (ORDER BY " + _sqlorder + ") Row," + _sql.Substring(7).Replace(" ORDER BY " + _sqlorder, string.Empty) + ") TMP WHERE Row>" + pagesize * (pageindex - 1) + " AND Row<=" + pagesize * pageindex + " ORDER BY Row";
            return strsql;
        }
        /// <summary>
        /// 得到总条数
        /// </summary>
        /// <returns></returns>
        public string GetCountSql()
        {
            return _sqlcount;
        }
        #endregion

        public static int GetSubstringCount(string strSource, string strSub, int startIndex)
        {
            //判断起始位置是否越界
            if (string.IsNullOrEmpty(strSource) || string.IsNullOrEmpty(strSub))
                return 0;
            if (startIndex >= strSource.Length)
                return 0;
            //得到子字符串出现的次数
            strSource = strSource.Substring(startIndex);
            return (strSource.Length - strSource.Replace(strSub, string.Empty).Length) / strSub.Length;
        }

    }
}
