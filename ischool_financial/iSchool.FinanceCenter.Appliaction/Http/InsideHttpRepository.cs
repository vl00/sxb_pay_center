using iSchool.FinanceCenter.Appliaction.HttpDto;
using iSchool.Infrastructure.Common;
using Microsoft.Extensions.Logging;
using ProductManagement.Tool.HttpRequest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace iSchool.FinanceCenter.Appliaction.Http
{
    /// <summary>
    /// 内部服务接口
    /// </summary>
    public class InsideHttpRepository : HttpBaseClient<RequestConfig>, IInsideHttpRepository
    {
        public InsideHttpRepository(HttpClient client, HttpApiConfigs configs, ILoggerFactory log)
            : base(client, configs.InsideApiConfig, log)
        {
        }

        public async Task<List<UserInfo>> GetUsers(IEnumerable<Guid> userIds)
        {
            var option = new GetUsersOption(userIds);
            var result = await PostAsync<HttpResultWrapper<List<UserInfo>>, GetUsersOption>(option);
            return result.Data;
        }
        public async Task<List<OpenIdWeixinDto>> GetUserOpenIds(IEnumerable<Guid> userIds)
        {
            var option = new GetUserOpenIdOption(userIds);
            var result = await PostAsync<HttpResultWrapper<List<OpenIdWeixinDto>>, GetUserOpenIdOption>(option);
            return result.Data;
        }

        public async Task<List<Guid>> GetUserIds(string idNamePhone)
        {
            if (string.IsNullOrWhiteSpace(idNamePhone))
            {
                return new List<Guid>() { };
            }

            if (Guid.TryParse(idNamePhone, out Guid userId))
            {
                return new List<Guid>() { userId };
            }

            //get userid by name/phone
            var users = await GetUsersByPhone(phone: idNamePhone);
            if (users == null || users.Count == 0)
            {
                users = await GetUsersByName(name: idNamePhone);
            }
            return users?.Select(user => user.Id).ToList();
        }

        public async Task<List<UserInfo>> GetUsersByPhone(string phone)
        {
            var option = new GetUserByPhoneOption(phone);
            var result = await GetAsync<HttpResultWrapper<List<UserInfo>>, GetUserByPhoneOption>(option);
            return result.Data;
        }


        public async Task<List<UserInfo>> GetUsersByName(string name)
        {
            var option = new GetUserByNameOption(name);
            var result = await GetAsync<HttpResultWrapper<List<UserInfo>>, GetUserByNameOption>(option);
            return result.Data;
        }
      
    }
}
