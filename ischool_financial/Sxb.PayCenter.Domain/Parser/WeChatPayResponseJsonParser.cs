using System;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace Sxb.PayCenter.WechatPay
{
    public class WeChatPayResponseJsonParser<T> where T : WeChatPayResponse
    {
        private static readonly JsonSerializerOptions jsonSerializerOptions = new JsonSerializerOptions { IgnoreNullValues = true, Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping };

        public T Parse(string body, int statusCode)
        {
            ...
        }
    }
}
