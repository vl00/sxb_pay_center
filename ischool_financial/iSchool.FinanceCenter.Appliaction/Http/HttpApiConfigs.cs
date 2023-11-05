using ProductManagement.Tool.HttpRequest;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.FinanceCenter.Appliaction.Http
{
    public class HttpApiConfigs
    {
        public RequestConfig UserApiConfig { get; set; }
        public RequestConfig InsideApiConfig { get; set; }
        public RequestConfig OrgApiConfig { get; set; }
        public RequestConfig FinancialCenterApiConfig { get; set; }
        public RequestConfig WeChatAppConfig { get; set; }
        public RequestConfig GaoDengApiConfig { get; set; }
    }
}
