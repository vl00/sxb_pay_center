using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Http;

namespace Sxb.PayCenter.WechatPay
{
    public static class ServiceCollectionExtensions
    {
        public static void AddWeChatPay(this IServiceCollection services)
        {
            services.AddHttpClient("Sxb.PayCenter.WechatPay");
            services.TryAddEnumerable(ServiceDescriptor.Singleton<IHttpMessageHandlerBuilderFilter, WeChatPayHttpMessageHandlerBuilderFilter>());
            services.AddSingleton<WeChatPayClientCertificateManager>();
            services.AddSingleton<WeChatPayPlatformCertificateManager>();
            services.AddSingleton<IWeChatPayClient,WeChatPayClient>();
            services.AddSingleton<IWeChatPayNotifyClient, WeChatPayNotifyClient>();

        }
    }
}
