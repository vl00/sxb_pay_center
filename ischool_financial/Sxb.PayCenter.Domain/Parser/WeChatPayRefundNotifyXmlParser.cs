using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace Sxb.PayCenter.WechatPay
{
    public class WeChatPayRefundNotifyXmlParser<T> where T : WeChatPayRefundNotify
    {

        public T Parse(string body, string key)
        {
            var result = Parse<T>(body);
            if (result?.ReqInfo == null) throw new WeChatPayException("退款通知v2格式出错了");

            var k = MD5.Compute(key).ToLowerInvariant();
            var data = AES.Decrypt(result.ReqInfo, k, CipherMode.ECB, PaddingMode.PKCS7);

            var r = Parse<WeChatPayRefundNotify_ReqInfo>(data, "root");             
            result.OutRefundNo = r.OutRefundNo;
            result.OutTradeNo = r.OutTradeNo;
            result.RefundAccount = r.RefundAccount;
            result.RefundFee = r.RefundFee;
            result.RefundId = r.RefundId;
            result.RefundRecvAccout = r.RefundRecvAccout;
            result.RefundRequestSource = r.RefundRequestSource;
            result.RefundStatus = r.RefundStatus;
            result.SettlementRefundFee = r.SettlementRefundFee;
            result.SettlementTotalFee = r.SettlementTotalFee;
            result.SuccessTime = DateTime.TryParse(r.SuccessTime, out var _st) ? _st : default;
            result.TotalFee = r.TotalFee;
            result.TransactionId = r.TransactionId;

            return result;
        }

        static TT Parse<TT>(string body, string root = "xml") where TT : class
        {
            TT result = null;
            var parameters = new WeChatPayDictionary();

            try
            {
                var bodyDoc = XDocument.Parse(body).Element(root);
                foreach (var element in bodyDoc.Elements())
                {
                    parameters.Add(element.Name.LocalName, element.Value);
                }

                using (var sr = new StringReader(body))
                {
                    var xmldes = new XmlSerializer(typeof(TT));
                    result = (TT)xmldes.Deserialize(sr);
                }
            }
            catch { }

            if (result == null)
            {
                result = Activator.CreateInstance<TT>();
            }

            return result;
        }
    }
}
