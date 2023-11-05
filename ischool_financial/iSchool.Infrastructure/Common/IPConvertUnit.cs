using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Infrastructure.Common
{
    public static class IPConvertUnit
    {
        public static long Ip2Long(string strIP)
        {
            strIP = strIP == "::1" ? "127.0.0.1" : strIP;
            long[] ip = new long[4];
            string[] s = strIP.Split('.');
            ip[0] = long.Parse(s[3]);
            ip[1] = long.Parse(s[2]);
            ip[2] = long.Parse(s[1]);
            ip[3] = long.Parse(s[0]);
            return (ip[0] << 24) + (ip[1] << 16) + (ip[2] << 8) + ip[3];
        }


        public static string Long2IP(long longIP)
        {
            StringBuilder sb = new StringBuilder("");
            sb.Append((longIP & 0x000000FF));
            sb.Append(".");
            sb.Append((longIP & 0x0000FFFF) >> 8);
            sb.Append(".");
            sb.Append((longIP & 0x00FFFFFF) >> 16);
            sb.Append(".");
            sb.Append(longIP >> 24);
            return sb.ToString();
        }
    }
}
