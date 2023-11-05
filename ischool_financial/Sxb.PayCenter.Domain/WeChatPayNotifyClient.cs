using System;
using System.IO;
using System.Linq;

using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Sxb.PayCenter.WechatPay
{
    public class WeChatPayNotifyClient : IWeChatPayNotifyClient
    {
        #region WeChatPayNotifyClient Constructors

        private readonly IWeChatPayClient _client;
        private readonly WeChatPayPlatformCertificateManager _platformCertificateManager;

        public WeChatPayNotifyClient(IWeChatPayClient client, WeChatPayPlatformCertificateManager platformCertificateManager)
        {
            _client = client;
            _platformCertificateManager = platformCertificateManager;
        }

        #endregion

        #region IWeChatPayNotifyClient Members

#if NETCOREAPP3_1 || NET5_0
        public async Task<T> ExecuteAsync<T>(Microsoft.AspNetCore.Http.HttpRequest request, WeChatPayOptions options) where T : WeChatPayNotify
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            var headers = GetWeChatPayHeadersFromRequest(request);
            using (var reader = new StreamReader(request.Body, Encoding.UTF8, true, 1024, true))
            {
                var body = await reader.ReadToEndAsync();
                return await ExecuteAsync<T>(headers, body, options);
            }
        }
        public async Task<T> ExecuteRefundAsync<T>(Microsoft.AspNetCore.Http.HttpRequest request, WeChatPayOptions options) where T : WeChatPayRefundNotify
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            var headers = GetWeChatPayHeadersFromRequest(request);
            using (var reader = new StreamReader(request.Body, Encoding.UTF8, true, 1024, true))
            {
                var body = await reader.ReadToEndAsync();
                return await ExecuteRefundAsync<T>(headers, body, options);
            }
        }
        private static WeChatPayHeaders GetWeChatPayHeadersFromRequest(Microsoft.AspNetCore.Http.HttpRequest request)
        {
            var headers = new WeChatPayHeaders();

            if (request.Headers.TryGetValue(WeChatPayConsts.Wechatpay_Serial, out var serialValues))
            {
                headers.Serial = serialValues.First();
            }

            if (request.Headers.TryGetValue(WeChatPayConsts.Wechatpay_Timestamp, out var timestampValues))
            {
                headers.Timestamp = timestampValues.First();
            }

            if (request.Headers.TryGetValue(WeChatPayConsts.Wechatpay_Nonce, out var nonceValues))
            {
                headers.Nonce = nonceValues.First();
            }

            if (request.Headers.TryGetValue(WeChatPayConsts.Wechatpay_Signature, out var signatureValues))
            {
                headers.Signature = signatureValues.First();
            }

            return headers;
        }
#endif

        #endregion

        #region IWeChatPayNotifyClient Members

        public async Task<T> ExecuteAsync<T>(WeChatPayHeaders headers, string body, WeChatPayOptions options) where T : WeChatPayNotify
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            if (string.IsNullOrEmpty(options.V3Key))
            {
                throw new WeChatPayException("options.V3Key is Empty!");
            }
            //验证通知签名--重要，防止欺骗回调
          //  await CheckNotifySignAsync(headers, body, options);

            var parser = new WeChatPayNotifyJsonParser<T>();
            var notify = parser.Parse(body, options.V3Key);
            return notify;


        }

        #region old codes
        //public async Task<T> ExecuteRefundAsync<T>(WeChatPayHeaders headers, string body, WeChatPayOptions options, int type = 0) where T : WeChatPayRefundNotify
        //{
        //    if (string.IsNullOrEmpty(body))
        //    {
        //        throw new ArgumentNullException(nameof(body));
        //    }

        //    if (options == null)
        //    {
        //        throw new ArgumentNullException(nameof(options));
        //    }

        //    if (string.IsNullOrEmpty(options.Key))
        //    {
        //        throw new WeChatPayException("options.Key is Empty!");
        //    }

           

        //    var parser = new WeChatPayRefundNotifyJsonParser<T>();
        //    var notify = parser.Parse(body, options.V3Key);
        //    if (notify is WeChatPayRefundNotify)
        //    {
        //        var key = MD5.Compute(options.Key).ToLowerInvariant();
        //        var data = AES.Decrypt((notify as WeChatPayRefundNotify).ReqInfo, key, CipherMode.ECB, PaddingMode.PKCS7);
        //        notify = parser.Parse(body, data);
        //    }
        //    else
        //    {
        //        //验证通知签名
        //        await CheckNotifySignAsync(headers, body, options);
        //    }

        //    return notify;


        //}
        #endregion old codes

        public async Task<T> ExecuteRefundAsync<T>(WeChatPayHeaders headers, string body, WeChatPayOptions options, int type = 0) where T : WeChatPayRefundNotify
        {
            if (string.IsNullOrEmpty(body))
            {
                throw new ArgumentNullException(nameof(body));
            }
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }
            if (string.IsNullOrEmpty(options.Key))
            {
                throw new WeChatPayException("options.Key is Empty!");
            }

            var parser = new WeChatPayRefundNotifyXmlParser<WeChatPayRefundNotify>();
            var notify = parser.Parse(body, options.Key);
            if (notify is WeChatPayRefundNotify)
            {
                // notify is not null
            }
            else
            {
                //验证通知签名
                await CheckNotifySignAsync(headers, body, options);
            }

            return (T)notify;
        }

        #endregion

        #region Check Notify Method

        private async Task CheckNotifySignAsync(WeChatPayHeaders headers, string body, WeChatPayOptions options)
        {
            if (string.IsNullOrEmpty(headers.Serial))
            {
                throw new WeChatPayException($"sign check fail: {nameof(headers.Serial)} is empty!");
            }

            if (string.IsNullOrEmpty(headers.Signature))
            {
                throw new WeChatPayException($"sign check fail: {nameof(headers.Signature)} is empty!");
            }

            if (string.IsNullOrEmpty(body))
            {
                throw new WeChatPayException("sign check fail: body is empty!");
            }

            var cert = await LoadPlatformCertificateAsync(headers.Serial, options);
            var signatureSourceData = BuildSignatureSourceData(headers.Timestamp, headers.Nonce, body);

            if (!SHA256WithRSA.Verify(cert.GetRSAPublicKey(), signatureSourceData, headers.Signature))
            {
                throw new WeChatPayException("sign check fail: check Sign and Data Fail!");
            }
        }

        private async Task<X509Certificate2> LoadPlatformCertificateAsync(string serial, WeChatPayOptions options)
        {
            // 如果证书序列号已缓存，则直接使用缓存的
            if (_platformCertificateManager.TryGetValue(serial, out var certificate2))
            {
                return certificate2;
            }

            // 否则重新下载新的平台证书
            var request = new WeChatPayCertificatesRequest();
            var response = await _client.ExecuteAsync(request, options);
            foreach (var certificate in response.Certificates)
            {
                // 若证书序列号未被缓存，解密证书并加入缓存
                if (!_platformCertificateManager.ContainsKey(certificate.SerialNo))
                {
                    switch (certificate.EncryptCertificate.Algorithm)
                    {
                        case nameof(AEAD_AES_256_GCM):
                            {
                                var certStr = AEAD_AES_256_GCM.Decrypt(certificate.EncryptCertificate.Nonce, certificate.EncryptCertificate.Ciphertext, certificate.EncryptCertificate.AssociatedData, options.V3Key);
                                var cert = new X509Certificate2(Encoding.UTF8.GetBytes(certStr));
                                _platformCertificateManager.TryAdd(certificate.SerialNo, cert);
                            }
                            break;
                        default:
                            throw new WeChatPayException($"Unknown algorithm: {certificate.EncryptCertificate.Algorithm}");
                    }
                }
            }

            // 重新从缓存获取
            if (_platformCertificateManager.TryGetValue(serial, out certificate2))
            {
                return certificate2;
            }
            else
            {
                throw new WeChatPayException("Download certificates failed!");
            }
        }

        private static string BuildSignatureSourceData(string timestamp, string nonce, string body)
        {
            return $"{timestamp}\n{nonce}\n{body}\n";
        }

        #endregion
    }
}
