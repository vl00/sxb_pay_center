using iSchool.Infrastructure.SxbCommonResponse;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ProductManagement.API.Http.HttpExtend
{
    public static class HttpClientExt
    {
        /// <summary>
        /// 内部敏感接口请求
        /// </summary>
        /// <param name="client"></param>
        /// <param name="requestUri"></param>
        /// <param name="requestJson"></param>
        /// <returns></returns>
        public static async Task<SxbResponse> SxbPostAsync(this HttpClient client, string requestUri, string requestJson)
        {
            try
            {
                var timespan = DateTime.Now.ToString("yyyyMMddHHmmss");
                var nonce = Guid.NewGuid().ToString("N");
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Add("sxb.timespan", timespan);
                client.DefaultRequestHeaders.Add("sxb.nonce", nonce);
                client.DefaultRequestHeaders.Add("sxb.key", "AskSystem");
                StringBuilder query = new StringBuilder("woaisxb2021");//私钥，不能泄露
                query.Append($"{timespan}\n{nonce}\n{requestJson}\n");
                MD5 md5 = MD5.Create();
                byte[] bytes = md5.ComputeHash(Encoding.UTF8.GetBytes(query.ToString()));
                StringBuilder result = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    result.Append(bytes[i].ToString("X2"));
                }
                var sign = result.ToString();
                client.DefaultRequestHeaders.Add("sxb.sign", sign);
                using (var reqContent = new StringContent(requestJson, Encoding.UTF8, "application/json"))
                using (var resp = await client.PostAsync(requestUri, reqContent))
                using (var respContent = resp.Content)
                {
                 
                    var body = await respContent.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<SxbResponse>(body);
                  
                }
               

            }
            catch (Exception ex)
            {
               return new SxbResponse() { succeed=false,msg= $"请求未成功{ex.Message}" };
               
            }

        }


        public static async Task<SxbResponse> SxbPost2Async(this HttpClient client, string requestUri, string requestJson)
        {
            try
            {
                var timespan = DateTime.Now.ToString("yyyyMMddHHmmss");
                var nonce = Guid.NewGuid().ToString("N");
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Add("sxb-timespan", timespan);
                client.DefaultRequestHeaders.Add("sxb-nonce", nonce);
                client.DefaultRequestHeaders.Add("sxb-key", "AskSystem");
                StringBuilder query = new StringBuilder("woaisxb2021");//私钥，不能泄露
                query.Append($"{timespan}\n{nonce}\n{requestJson}\n");
                MD5 md5 = MD5.Create();
                byte[] bytes = md5.ComputeHash(Encoding.UTF8.GetBytes(query.ToString()));
                StringBuilder result = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    result.Append(bytes[i].ToString("X2"));
                }
                var sign = result.ToString();
                client.DefaultRequestHeaders.Add("sxb-sign", sign);
                using (var reqContent = new StringContent(requestJson, Encoding.UTF8, "application/json"))
                using (var resp = await client.PostAsync(requestUri, reqContent))
                using (var respContent = resp.Content)
                {

                    var body = await respContent.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<SxbResponse>(body);

                }


            }
            catch (Exception ex)
            {
                return new SxbResponse() { succeed = false, msg = $"请求未成功{ex.Message}" };

            }

        }
    }
}
