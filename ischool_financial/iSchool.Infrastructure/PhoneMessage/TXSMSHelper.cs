using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace iSchool.Infrastructure
{
    public class TXSMSHelper
    {
        //int sdkappid = 1400013556;
        //string appkey = "ba4604e9bba557e6792c876c5e609df2";
        string url = "https://yun.tim.qq.com/v5/tlssmssvr/sendsms";

        SmsSenderUtil util = new SmsSenderUtil();

        public TXSMSHelper() { }

        /**
         * 普通单发短信接口，明确指定内容，如果有多个签名，请在内容中以【】的方式添加到信息内容中，否则系统将使用默认签名
         * @param type 短信类型，0 为普通短信，1 营销短信
         * @param nationCode 国家码，如 86 为中国
         * @param phoneNumber 不带国家码的手机号
         * @param msg 信息内容，必须与申请的模板格式一致，否则将返回错误
         * @param extend 扩展码，可填空
         * @param ext 服务端原样返回的参数，可填空
         * @return SmsSingleSenderResult
         */
        public SmsSingleSenderResult Send(
            string sdkappid,
            string appkey,
            int type,
            string nationCode,
            string phoneNumber,
            string msg,
            string extend,
            string ext)
        {
            /*
            请求包体
            {
                "tel": {
                    "nationcode": "86", 
                    "mobile": "13788888888"
                },
                "type": 0, 
                "msg": "你的验证码是1234", 
                "sig": "fdba654e05bc0d15796713a1a1a2318c", 
                "time": 1479888540,
                "extend": "",
                "ext": ""
            }
            应答包体
            {
                "result": 0,
                "errmsg": "OK", 
                "ext": "", 
                "sid": "xxxxxxx", 
                "fee": 1
            }
            */
            if (0 != type && 1 != type)
            {
                throw new Exception("type " + type + " error");
            }
            if (null == extend)
            {
                extend = "";
            }
            if (null == ext)
            {
                ext = "";
            }

            long random = util.GetRandom();
            long curTime = util.GetCurTime();

            // 按照协议组织 post 请求包体
            JObject data = new JObject();

            JObject tel = new JObject();
            tel.Add("nationcode", nationCode);
            tel.Add("mobile", phoneNumber);

            data.Add("tel", tel);
            data.Add("msg", msg);
            data.Add("type", type);
            data.Add("sig", util.StrToHash(String.Format(
                "appkey={0}&random={1}&time={2}&mobile={3}",
                appkey, random, curTime, phoneNumber)));
            data.Add("time", curTime);
            data.Add("extend", extend);
            data.Add("ext", ext);

            string wholeUrl = url + "?sdkappid=" + sdkappid + "&random=" + random;
            HttpWebRequest request = util.GetPostHttpConn(wholeUrl);
            byte[] requestData = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(data));
            request.ContentLength = requestData.Length;
            Stream requestStream = request.GetRequestStream();
            requestStream.Write(requestData, 0, requestData.Length);
            requestStream.Close();

            // 接收返回包
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream responseStream = response.GetResponseStream();
            StreamReader streamReader = new StreamReader(responseStream, Encoding.GetEncoding("utf-8"));
            string responseStr = streamReader.ReadToEnd();
            streamReader.Close();
            responseStream.Close();
            SmsSingleSenderResult result;
            if (HttpStatusCode.OK == response.StatusCode)
            {
                result = util.ResponseStrToSingleSenderResult(responseStr);
            }
            else
            {
                result = new SmsSingleSenderResult();
                result.result = -1;
                result.errmsg = "http error " + response.StatusCode + " " + responseStr;
            }
            return result;
        }

        /**
         * 指定模板单发
         * @param nationCode 国家码，如 86 为中国
         * @param phoneNumber 不带国家码的手机号
         * @param templId 模板 id
         * @param templParams 模板参数列表，如模板 {1}...{2}...{3}，那么需要带三个参数
         * @param extend 扩展码，可填空
         * @param ext 服务端原样返回的参数，可填空
         * @return SmsSingleSenderResult
         */
        public SmsSingleSenderResult SendWithParam(
            int sdkappid,
            string appkey,
            string nationCode,
            string phoneNumber,
            int templId,
            List<string> templParams,
            string sign,
            string extend,
            string ext)
        {
            /*
            请求包体
            {
                "tel": {
                    "nationcode": "86",
                    "mobile": "13788888888"
                },
                "sign": "腾讯云",
                "tpl_id": 19,
                "params": [
                    "验证码", 
                    "1234",
                    "4"
                ],
                "sig": "fdba654e05bc0d15796713a1a1a2318c",
                "time": 1479888540,
                "extend": "",
                "ext": ""
            }
            应答包体
            {
                "result": 0,
                "errmsg": "OK", 
                "ext": "", 
                "sid": "xxxxxxx", 
                "fee": 1
            }
            */
            if (null == sign)
            {
                sign = "";
            }
            if (null == extend)
            {
                extend = "";
            }
            if (null == ext)
            {
                ext = "";
            }

            long random = util.GetRandom();
            long curTime = util.GetCurTime();

            // 按照协议组织 post 请求包体
            JObject data = new JObject();

            JObject tel = new JObject();
            tel.Add("nationcode", nationCode);
            tel.Add("mobile", phoneNumber);

            data.Add("tel", tel);
            data.Add("sig", util.CalculateSigForTempl(appkey, random, curTime, phoneNumber));
            data.Add("tpl_id", templId);
            data.Add("params", util.SmsParamsToJSONArray(templParams));
            data.Add("sign", sign);
            data.Add("time", curTime);
            data.Add("extend", extend);
            data.Add("ext", ext);

            string wholeUrl = url + "?sdkappid=" + sdkappid + "&random=" + random;
            HttpWebRequest request = util.GetPostHttpConn(wholeUrl);
            byte[] requestData = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(data));
            request.ContentLength = requestData.Length;
            Stream requestStream = request.GetRequestStream();
            requestStream.Write(requestData, 0, requestData.Length);
            requestStream.Close();

            // 接收返回包
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream responseStream = response.GetResponseStream();
            StreamReader streamReader = new StreamReader(responseStream, Encoding.GetEncoding("utf-8"));
            string responseStr = streamReader.ReadToEnd();
            streamReader.Close();
            responseStream.Close();
            SmsSingleSenderResult result;
            if (HttpStatusCode.OK == response.StatusCode)
            {
                result = util.ResponseStrToSingleSenderResult(responseStr);
            }
            else
            {
                result = new SmsSingleSenderResult();
                result.result = -1;
                result.errmsg = "http error " + response.StatusCode + " " + responseStr;
            }
            return result;
        }
    }
}
