using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Infrastructure.Common
{
    /// <summary>
    /// 时间类的通用方法
    /// </summary>
   public  class TimeHelp
   {
        /// <summary>
        /// 获取两个年份段之间的所有年份
        /// 前提：minyear<=maxyear
        /// </summary>
        /// <param name="minyear">最小年份</param>
        /// <param name="maxyear">最大年份</param>
        /// <returns></returns>
        public static List<string> GetYearist(string minyear, string maxyear)
        {
            List<string> yearList = new List<string>();
            DateTime time1 = Convert.ToDateTime(minyear + "-01-01");
            DateTime time2 = Convert.ToDateTime(maxyear + "-01-01");
            while (time1 <= time2)
            {
                yearList.Add(time1.ToString("yyyy"));
                time1 = time1.AddYears(1);
            }
            return yearList;
        }


        public static List<string> GetNewYearist()
        {
            var minYear = ConfigurationUtil.GetSection("AllowedYearRange:MinYear");
            var maxYear = ConfigurationUtil.GetSection("AllowedYearRange:MaxYear");
            List<string> yearList = new List<string>();
            DateTime time1 = Convert.ToDateTime(minYear + "-01-01");
            DateTime time2 = Convert.ToDateTime(maxYear + "-01-01");
            while (time1 <= time2)
            {
                yearList.Add(time1.ToString("yyyy"));
                time1 = time1.AddYears(1);
            }
            return yearList;
        }


        /// <summary>
        ///  时间转时间戳Unix-时间戳精确到毫秒
        /// </summary> 
        public static long ToUnixTimestampByMilliseconds(DateTime dt)
        {
            DateTimeOffset dto = new DateTimeOffset(dt);
            return dto.ToUnixTimeMilliseconds();
        }

    }
}
