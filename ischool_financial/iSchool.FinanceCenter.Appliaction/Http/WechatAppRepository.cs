using iSchool.FinanceCenter.Appliaction.HttpDto;
using iSchool.FinanceCenter.Appliaction.RequestDto;
using iSchool.Infrastructure;
using Microsoft.Extensions.Logging;
using ProductManagement.Tool.HttpRequest;
using System.Net.Http;
using System.Threading.Tasks;

namespace iSchool.FinanceCenter.Appliaction.Http
{

    public class WechatAppRepository : HttpBaseClient<RequestConfig>, IWechatAppRepository
    {
        public WechatAppRepository(HttpClient client, HttpApiConfigs configs, ILoggerFactory log)
            : base(client, configs.WeChatAppConfig, log)
        {
        }

        public async Task<GetAccessTokenResult> GetAccessToken(WeChatGetAccessTokenRequest request)
        {
            var response = await GetAsync<WeChatBaseResponseResult<GetAccessTokenResult>, WeChatAppGetAccessTokenOption>(new WeChatAppGetAccessTokenOption(request.App));
            if (response != null && response.success)
            {
                return response.data;
            }
            else
            {
                throw new CustomResponseException(response.msg);
               
            }
        }

       
    }
}
