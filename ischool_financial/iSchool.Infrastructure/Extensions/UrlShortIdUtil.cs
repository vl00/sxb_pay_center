using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace iSchool.Infrastructure.Extensions
{

    public static class UrlShortIdUtil
    {
        public static string ToShortId(long no)
        {
            var b = Encoding.Default.GetBytes(no.ToString());
            string shortId = Base32Util.ToBase32String(b);
            return shortId?.ToLower();
        }

        public static long ToNumber(string shortId)
        {
            var b = Base32Util.FromBase32String(shortId);
            var no = Encoding.Default.GetString(b);
            return no == null ? 0 : Convert.ToInt64(no);
        }

        public static bool IsNumber(string shortId)
        {
            var b = Base32Util.FromBase32String(shortId);
            var no = Encoding.Default.GetString(b);
            return IsNumeric(no);
        }

        public static bool IsNumeric(string strNumber)
        {
            Regex objNotNumberPattern = new Regex("[^0-9.-]");
            Regex objTwoDotPattern = new Regex("[0-9]*[.][0-9]*[.][0-9]*");
            Regex objTwoMinusPattern = new Regex("[0-9]*[-][0-9]*[-][0-9]*");
            string strValidRealPattern = "^([-]|[.]|[-.]|[0-9])[0-9]*[.]*[0-9]+$";
            string strValidIntegerPattern = "^([-]|[0-9])[0-9]*$";
            Regex objNumberPattern = new Regex("(" + strValidRealPattern + ")|(" + strValidIntegerPattern + ")");

            return !objNotNumberPattern.IsMatch(strNumber) &&
                !objTwoDotPattern.IsMatch(strNumber) &&
                !objTwoMinusPattern.IsMatch(strNumber) &&
                objNumberPattern.IsMatch(strNumber);
        }

        public static string Long2Base32(long number)
        {
            byte[] bytes = BitConverter.GetBytes(number);
            List<byte> buffer = new List<byte>();
            List<byte> sbytes = new List<byte>();
            foreach (byte b in bytes)
            {
                buffer.Add(b);
                if (b != 0)
                {
                    sbytes.AddRange(buffer);
                    buffer.Clear();
                }
            }
            if (sbytes.Count == 0)
            {
                sbytes.Add(0);
            }
            return Base32Util.ToBase32String(sbytes.ToArray())?.ToLower();
        }
        public static long Base322Long(string base32string)
        {
            byte[] bytes = Base32Util.FromBase32String(base32string);
            List<byte> result = new List<byte>();
            result.AddRange(bytes);
            if (result.Count < 8)
            {
                result.AddRange(new byte[8 - bytes.Length]);
            }
            return BitConverter.ToInt64(result.ToArray(), 0);
        }

        public static string ToBase32String(this string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return "";
            var b = Encoding.Default.GetBytes(input);
            return Base32Util.ToBase32String(b)?.ToLower();
        }
    }
}
