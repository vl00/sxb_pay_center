using System.IO;
using System.Text;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System;
using Newtonsoft.Json;

namespace iSchool.Infrastructure
{
    public static class AliyunBankcardHelper
    {
        private const String host = "https://b234bzxsv1.market.alicloudapi.com";
        private const String path = "/bank/4bzxsv1";
        private const String method = "POST";
        private const String appcode = "549d9d670d85496994f3f170f7be3154";

        public static BankCardCheckResponse Check(BankCardCheckReq req)
        {
            //var bodys=JsonConvert.SerializeObject(req);

            String bodys = $"bankcard={req.bankcard}&customername={req.customername}&idcard={req.idcard}&idcardtype={req.idcardtype}&mobile={req.mobile}&realname={req.realname}&scenecode={req.scenecode}";
            String querys = "";

            String url = host + path;
            HttpWebRequest httpRequest = null;
            HttpWebResponse httpResponse = null;

            if (0 < querys.Length)
            {
                url = url + "?" + querys;
            }

            if (host.Contains("https://"))
            {
                ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(CheckValidationResult);
                httpRequest = (HttpWebRequest)WebRequest.CreateDefault(new Uri(url));
            }
            else
            {
                httpRequest = (HttpWebRequest)WebRequest.Create(url);
            }
            httpRequest.Method = method;
            httpRequest.Headers.Add("Authorization", "APPCODE " + appcode);
            //需要给X-Ca-Nonce的值生成随机字符串，每次请求不能相同
            httpRequest.Headers.Add("X-Ca-Nonce", System.Guid.NewGuid().ToString());
            //根据API的要求，定义相对应的Content-Type
            httpRequest.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
            if (0 < bodys.Length)
            {
                byte[] data = Encoding.UTF8.GetBytes(bodys);
                using (Stream stream = httpRequest.GetRequestStream())
                {
                    stream.Write(data, 0, data.Length);
                }
            }
            try
            {
                httpResponse = (HttpWebResponse)httpRequest.GetResponse();
            }
            catch (WebException ex)
            {
                httpResponse = (HttpWebResponse)ex.Response;
            }


            Stream st = httpResponse.GetResponseStream();
            StreamReader reader = new StreamReader(st, Encoding.GetEncoding("utf-8"));
            var r = reader.ReadToEnd();
            return JsonConvert.DeserializeObject<BankCardCheckResponse> (r);

        }

        public static bool CheckValidationResult(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
        {
            return true;
        }

    }
    public class BankCardCheckReq
    {
        /// <summary>
        /// 银行卡号
        /// </summary>
        public string bankcard { get; set; }
        /// <summary>
        /// 商户名称
        /// </summary>
        public string customername { get; set; }
        /// <summary>
        /// 证件号
        /// </summary>
        public string idcard { get; set; }
        /// <summary>
        /// 证件类型（选填） 01：身份证（默认） 02：军官证 03：护照 04：回乡证 05：台胞证 06：警官证 07：士兵证 08：驾驶证 09：学⽣证 10：港澳证 99：其它证件
        /// </summary>
        public string idcardtype { get; set; } = "01";
        /// <summary>
        /// 银行卡预留手机号
        /// </summary>
        public string mobile { get; set; }
        /// <summary>
        /// 姓名
        /// </summary>
        public string realname { get; set; }
        /// <summary>
        /// 商户业务应用场景（01：直销银行；02：消费金融；03：银行二三类账户开户；04：征信；05：保险；06：基金；07：证券；08 租赁；09：海关申报；99：其他）
        /// </summary>
        public string scenecode { get; set; } = "01";

    }


    public class BankCardCheckResponse
    {
        public string errcode { get; set; }
        public BResult result { get; set; }
        public string jobid { get; set; }
        public string responsetime { get; set; }
        public string errmsg { get; set; }
    }

    public class BResult
    {
        public string areaname { get; set; }
        public string bankcardtype { get; set; }
        public string bankalias { get; set; }
        public string isluhn { get; set; }
        public string carddigits { get; set; }
        public string cardbin { get; set; }
        public string provincename { get; set; }
        public string weburl { get; set; }
        public string bindigits { get; set; }
        public string cardname { get; set; }
        public string logo { get; set; }
        public string tel { get; set; }
        public string bankname { get; set; }
    }

}
