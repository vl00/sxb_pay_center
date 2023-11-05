using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace iSchool
{
    public static partial class StringHelper
    {
        public static bool IsNullOrEmpty(this string str)
        {
            return string.IsNullOrEmpty(str);
        }

        public static bool IsNullOrWhiteSpace(this string str)
        {
            return string.IsNullOrEmpty(str) || string.IsNullOrWhiteSpace(str);
        }

        public static string FormatWith(this string str, params object[] args)
        {
            return string.Format(str, args);
        }

        public static string FormatWith(this string str, params (string k, object v)[] args)
        {
            if (string.IsNullOrEmpty(str) || string.IsNullOrWhiteSpace(str)) return str;
            foreach (var (k, v) in args)
            {
                str = str.Replace(!k.StartsWith('{') ? ("{" + k + "}") : k, v?.ToString());
            }
            return str;
        }

        public static StringBuilder AppendLine(this StringBuilder builder, string str, params object[] args)
        {
            return builder.AppendLine(str, args);
        }

        //eg: left join MaintenanceTemplates it on it.Id = m.MaintenanceTemplateId
        //    where m.IsDeleted = 0
        //    {" and m.Code = @KeyWord ".If(!string.IsNullOrWhiteSpace(input.KeyWord))}
        //    {" and m.ProjectId = @ProjectId ".If(input.ProjectId.HasValue)}
        //    {" and a.ProductId = @ProductId ".If(input.ProductId.HasValue)}
        public static string If(this string str, bool condition)
        {
            return condition ? str : string.Empty;
        }

        /// <summary>
        /// 获取html里面的text并且删除空格
        /// </summary>
        public static string GetHtmlText(this string htmlText)
        {
            string noStyle = htmlText.Replace("&quot;", "\"").Replace("&lt;", "<").Replace("&gt;", ">").Replace("&quot;", "\"").Replace("&nbsp;", "");
            noStyle = Regex.Replace(noStyle, @"<[\w\W]*?>", "", RegexOptions.IgnoreCase);
            noStyle = Regex.Replace(noStyle, @"\s", "", RegexOptions.IgnoreCase);
            return noStyle;
        }
        /// <summary>
        /// 身份证+***
        /// </summary>
        /// <param name="idcardno"></param>
        /// <returns></returns>
        public static string IdCardHide(string idCardNo)
        {
            if (string.IsNullOrEmpty(idCardNo)) return "";
            var len = idCardNo.Length;
            var temp = idCardNo.Substring(0, 4) + "********" + idCardNo.Substring(len - 6, 6);
            return temp;
        }
        /// <summary>
        /// 银行卡+***
        /// </summary>
        /// <param name="idcardno"></param>
        /// <returns></returns>
        public static string BankCardHide(string bankCardNo)
        {
            if (string.IsNullOrEmpty(bankCardNo)) return "";
            var len = bankCardNo.Length;
            var temp = bankCardNo.Substring(0, 4) + "********" + bankCardNo.Substring(len - 6, 6);
            return temp;
        }
        public static bool CheckIDCard18(string idNumber)
        {
            long n = 0;
            if (long.TryParse(idNumber.Remove(17), out n) == false
                || n < Math.Pow(10, 16) || long.TryParse(idNumber.Replace('x', '0').Replace('X', '0'), out n) == false)
            {
                return false;//数字验证  
            }
            string address = "11x22x35x44x53x12x23x36x45x54x13x31x37x46x61x14x32x41x50x62x15x33x42x51x63x21x34x43x52x64x65x71x81x82x91";
            if (address.IndexOf(idNumber.Remove(2)) == -1)
            {
                return false;//省份验证  
            }
            string birth = idNumber.Substring(6, 8).Insert(6, "-").Insert(4, "-");
            DateTime time = new DateTime();
            if (DateTime.TryParse(birth, out time) == false)
            {
                return false;//生日验证  
            }
            string[] arrVarifyCode = ("1,0,x,9,8,7,6,5,4,3,2").Split(',');
            string[] Wi = ("7,9,10,5,8,4,2,1,6,3,7,9,10,5,8,4,2").Split(',');
            char[] Ai = idNumber.Remove(17).ToCharArray();
            int sum = 0;
            for (int i = 0; i < 17; i++)
            {
                sum += int.Parse(Wi[i]) * int.Parse(Ai[i].ToString());
            }
            int y = -1;
            Math.DivRem(sum, 11, out y);
            if (arrVarifyCode[y] != idNumber.Substring(17, 1).ToLower())
            {
                return false;//校验码验证  
            }
            return true;//符合GB11643-1999标准  
        }

        public static bool CheckBankCardNo(string no)
        {
            //第一步：从右边第1个数字开始每隔一位乘以2；
            //第二步： 把在第一步中获得的乘积的各位数字相加，然后再与原号码中未乘2的各位数字相加；
            //第三步：对于第二步求和值中个位数求10的补数，如果个位数为0则该校验码为0。
            System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex(@"^\d{7,19}$");
            if (!regex.IsMatch(no)) return false;
            var temp = no;
            var temp0 = temp.ToCharArray().Reverse().ToArray();
            var sum = 0d;
            for (int i = 1, len = temp0.Length; i < len; i++)
            {
                var temp1 = Convert.ToInt16(temp0[i].ToString());
                if ((i + 1) % 2 == 0)
                {
                    temp1 *= 2;
                }
                sum += Math.Floor(temp1 / 10d) + (temp1 % 10);
            }
            var checkCode = 10 - sum % 10;

            return temp0[0].ToString() == checkCode.ToString("0");
        }

    }
       
}