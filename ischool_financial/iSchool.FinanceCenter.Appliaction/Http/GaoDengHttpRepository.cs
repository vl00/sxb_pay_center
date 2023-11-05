using iSchool.FinanceCenter.Appliaction.HttpDto;
using iSchool.FinanceCenter.Appliaction.RequestDto.GaoDeng;
using iSchool.Infrastructure.Common;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using ProductManagement.Tool.HttpRequest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace iSchool.FinanceCenter.Appliaction.Http
{
    /// <summary>
    /// 内部服务接口
    /// </summary>
    public class GaoDengHttpRepository : HttpBaseClient<RequestConfig>, IGaoDengHttpRepository
    {
        ILogger<GaoDengHttpRepository> _log;
        string baseUrl = "";
        string financialBaseUrl = "";
        public GaoDengHttpRepository(HttpClient client, HttpApiConfigs configs, ILoggerFactory log, ILogger<GaoDengHttpRepository> logC)
            : base(client, configs.GaoDengApiConfig, log)
        {
            _log = logC;
            baseUrl = configs.GaoDengApiConfig.ServerUrl;
            financialBaseUrl = configs.FinancialCenterApiConfig.ServerUrl;
        }

        public async Task<bool> CommitBill(CommitBillRequest req,Guid userid)
        {
            try
            {
                var option = new GetGaoDengOption(req);
                var url = baseUrl + option.UrlPath+ "?userId="+ userid;
                //打款后回调地址
                req.statusCallBackUrl = financialBaseUrl+ "api/Withdraw/VerifyCallBack";
                var result = await PostAsync<GaoDengResponseResult<string>>(url,JsonConvert.SerializeObject(req));
                return result.succeed;
            }
            catch (Exception ex)
            {

                _log.LogError(ex,"审核同步高登系统出错");
            }
            return false;
        }
        /// <summary>
        /// 检查是否已经签约
        /// </summary>
        /// <param name="userid"></param>
        /// <returns></returns>
        public async Task<bool> CheckSign(Guid userid)
        {
            try
            {
                var options = new GetGaoDengCheckSignOption(userid);
                var result = await GetAsync<GaoDengResponseResult<CheckGdSignResult>, GetGaoDengCheckSignOption>(options);
                return result.data.isSign;
            }
            catch (Exception ex)
            {

                _log.LogError(ex, "验证高登签约出错");
            }
            return false;
        }
        private class CheckGdSignResult
        {
            public bool isSign { get; set; }

        }
        public async Task<GaoDengResponseResult<TResult>> PostAsync<TResult>(string requestUri, string requestJson)
        {
            string body = string.Empty;
            try
            {
                using (var reqContent = new StringContent(requestJson, Encoding.UTF8, "application/json"))
                {
                    reqContent.Headers.Add("token", "22a0cd7872c2b1464e7dd7d45817acbaf5a65ae508618e28334b4c1d7d302b40");
                    using (var resp = await _client.PostAsync(requestUri, reqContent))
                    using (var respContent = resp.Content)
                    {
                     
                        body = await respContent.ReadAsStringAsync();
                        return JsonConvert.DeserializeObject<GaoDengResponseResult<TResult>>(body);

                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
    }
}
