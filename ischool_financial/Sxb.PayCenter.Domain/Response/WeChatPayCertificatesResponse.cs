using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Sxb.PayCenter.WechatPay
{
    public class WeChatPayCertificatesResponse : WeChatPayResponse
    {
        [JsonPropertyName("data")]
        public List<Certificate> Certificates { get; set; }
    }
}
