﻿using System;
using System.Text;

namespace Sxb.PayCenter.WechatPay
{
    public static class MD5
    {
        public static string Compute(string data)
        {
            if (string.IsNullOrEmpty(data))
            {
                throw new ArgumentNullException(nameof(data));
            }

            using (var md5 = System.Security.Cryptography.MD5.Create())
            {
                var hsah = md5.ComputeHash(Encoding.UTF8.GetBytes(data));
                return BitConverter.ToString(hsah).Replace("-", "");
            }
        }
    }
}
