using Microsoft.Extensions.Logging;
using ProductManagement.Tool.HttpRequest;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace iSchool.FinanceCenter.Appliaction.Http
{
    public class UserHttpRepository : HttpBaseClient<RequestConfig>, IUserHttpRepository
    {
        public UserHttpRepository(HttpClient client, HttpApiConfigs configs, ILoggerFactory log)
            : base(client, configs.UserApiConfig, log)
        {
        }
    }
}
