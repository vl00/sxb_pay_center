using System.Text.Json.Serialization;

namespace Sxb.PayCenter.WechatPay
{
    /// <summary>
    /// 错误详情
    /// </summary>    
    public class ErrorDetail : WeChatPayObject
    {
        [JsonPropertyName("location")]
        public string Location { get; set; }

        [JsonPropertyName("value")]
        public int Value { get; set; }
    }
}
