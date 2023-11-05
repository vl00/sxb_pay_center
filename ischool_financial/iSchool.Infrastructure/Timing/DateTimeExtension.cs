using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Infrastructure.Timing
{
    public static class DateTimeExtension
    {
        /// <summary>
        ///  时间转时间戳Unix-时间戳精确到毫秒
        /// </summary> 
        public static long ToUnixTimestampByMilliseconds(this DateTime dt)
        {
            DateTimeOffset dto = new DateTimeOffset(dt);
            return dto.ToUnixTimeMilliseconds();
        }


        public static DateTime Min(this DateTime dt, DateTime compareDt)
        {
            return dt > compareDt ? compareDt : dt;
        }
    }
}
