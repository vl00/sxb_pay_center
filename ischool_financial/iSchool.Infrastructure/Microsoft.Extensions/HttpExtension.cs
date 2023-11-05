using System;
using System.Net.Http.Headers;
using System.Threading;

namespace System.Net.Http
{
    public static class HttpExtension
    {
        public static void Set(this HttpHeaders headers, string name, params string[] values)
        {
            headers.Remove(name);
            headers.TryAddWithoutValidation(name, values);
        }

        public static HttpRequestMessage SetHttpHeader(this HttpRequestMessage req, string name, params string[] values)
        {
            Set(req.Headers, name, values);
            return req;
        }

        public static HttpRequestMessage SetContent(this HttpRequestMessage req, HttpContent content)
        {
            req.Content = content;
            return req;
        }
    }
}