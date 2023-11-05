using iSchool.FinanceCenter.Domain.Security;
using iSchool.Infrastructure.Common;
using iSchool.Infrastructure.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ILogger = NLog.ILogger;

namespace iSchool.FinanceCenter.Api.Middlewares
{
    public class LoggerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger _logger;
        private IHttpContextAccessor _accessor;

        public LoggerMiddleware(RequestDelegate next,
            IHttpContextAccessor accessor)
        {
            _next = next;
            _logger = LogManager.GetCurrentClassLogger();
            _accessor = accessor;
        }
        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            finally
            {
                var routes = context.GetRouteData()?.Values ?? null;
                if (routes != null && routes.Keys.Count() > 0)
                {
                    var controller = routes["controller"]?.ToString();
                    var action = routes["action"]?.ToString();
                    var id = routes["id"]?.ToString();

                    var actionDesc = ActionExtension.Description(controller, action);
                    string path = "/" + controller + "/" + action;
                    var queryDic = context.Request.Query.ToDictionary(kv => kv.Key, kv =>
                    {
                        var v = kv.Value.ToString();
                        if (Guid.TryParse(v, out var gid)) return gid.ToString("n");
                        return v;
                    });
                    if (!string.IsNullOrWhiteSpace(id) && !queryDic.ContainsKey("id"))
                    {
                        queryDic.Add("id", id);
                    }
                    var queryString = string.Join("&", queryDic.Select(q => q.Key + "=" + q.Value));
                    var cookie = context.Request.Cookies;
                    var header = context.Request.Headers;

                    //获取head中参数的集合
                    Dictionary<string, string> headerDic = new Dictionary<string, string>();

                    var headStr = new StringBuilder();
                    foreach (var h in header)
                    {
                        headStr.Append(h.Key + ":" + h.Value + ",");

                        headerDic.Add(h.Key, h.Value);
                    }

                    //从header 中获取 platform  system  client
                    var platformMode = UserAgentUtils.GetPlatformMode(headerDic);

                    int platform = platformMode & 0x100;
                    int system = platformMode & 0x010;
                    int client = platformMode & 0x001;


                    //IP
                    string ipString = _accessor.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
                    Regex regEx = new Regex(@"(25[0-5]|2[0-4]\d|[0-1]\d{2}|[1-9]?\d)\.(25[0-5]|2[0-4]\d|[0-1]\d{2}|[1-9]?\d)\.(25[0-5]|2[0-4]\d|[0-1]\d{2}|[1-9]?\d)\.(25[0-5]|2[0-4]\d|[0-1]\d{2}|[1-9]?\d)");
                    long ip = IPConvertUnit.Ip2Long("127.0.0.1");
                    if (regEx.IsMatch(ipString))
                    {
                        ip = IPConvertUnit.Ip2Long(ipString);
                    }


                    //取得head中的userid跟userid
                    var Headers = GetHeardParams(context, "userid", "token");
                    var userId = Conv.ToGuidOrNull(Headers["userid"]).ToString();

                    //设备ID
                    string deviceId = null;
                    if (cookie.ContainsKey("uuid"))
                    {
                        deviceId = cookie["uuid"];
                    }


                    //渠道
                    string fw = null;
                    if (cookie.ContainsKey("fw"))
                    {
                        fw = cookie["fw"];
                    }
                    else if (queryDic.ContainsKey("fw"))
                    {
                        fw = queryDic["fw"];
                    }

                    string fx = null;
                    if (cookie.ContainsKey("fx"))
                    {
                        fw = cookie["fx"];
                    }
                    else if (queryDic.ContainsKey("fx"))
                    {
                        fw = queryDic["fx"];
                    }

                    string adcode = context.Request.GetCity("0").ToString();


                    //session
                    string sessionid = null;
                    if (cookie.ContainsKey("Sessionid"))
                    {
                        sessionid = cookie["Sessionid"];
                    }
                    else if (cookie.ContainsKey("sessionid"))
                    {
                        sessionid = cookie["sessionid"];
                    }

                    //坐标
                    var latitude = context.Request.GetLatitude("0");
                    var longitude = context.Request.GetLongitude("0");

                    string Params = "";
                    if (context.Request.Method.ToLower() == "get")
                    {
                        Params = queryString;
                    }
                    else
                    {
                        //context.Request.EnableBuffering();
                        //using (var reader = new StreamReader(context.Request.Body, Encoding.UTF8, true, 1024, true))
                        //{
                        //    Params = await reader.ReadToEndAsync();
                        //}
                        //context.Request.Body.Position = 0;
                        Params = BodyToString(context.Request);
                    }


                    //获取用户信息
                    var user = context.RequestServices.GetService<IUserInfo>();

                    LogEventInfo ei = new LogEventInfo();

                    ei.Properties["userId"] = user?.UserId;
                    ei.Properties["deviceId"] = deviceId;
                    ei.Properties["fw"] = fw;
                    ei.Properties["fx"] = fx;
                    ei.Properties["ip"] = ip;

                    ei.Properties["latitude"] = latitude;
                    ei.Properties["longitude"] = longitude;

                    ei.Properties["path"] = path;
                    //ei.Properties["queryString"] = queryString;
                    ei.Properties["actionName"] = actionDesc;

                    ei.Properties["platform"] = platform == 0x100 ? 1 : 2;
                    ei.Properties["system"] = platform == 0x100 ? 0 : system == 0x010 ? 1 : 2;
                    ei.Properties["client"] = platform == 0x100 ? 0 : client == 0x001 ? 1 : client == 0x002 ? 2 : client == 0x003 ? 3 : 0;

                    ei.Properties["adcode"] = adcode;
                    ei.Properties["sessionid"] = sessionid;
                    ei.Properties["params"] = Params;

                    ei.Properties["level"] = NLog.LogLevel.Info;


                    ei.Properties["header"] = headStr.ToString();

                    ei.Level = NLog.LogLevel.Info;
                    _logger.Info(ei);
                }
            }
        }


        /// <summary>
        /// 取得请求的heard信息
        /// </summary>
        /// <param name="actionContext"></param>
        /// <param name="parmas"></param>
        /// <returns></returns>
        private Dictionary<string, string> GetHeardParams(HttpContext actionContext, params string[] parmas)
        {

            parmas = parmas.Distinct().ToArray();
            var _paramsDic = new Dictionary<string, string>();
            foreach (var item in parmas)
            {
                StringValues list = new StringValues();
                actionContext.Request.Headers.TryGetValue(item, out list);
                if (list.Count() > 0)
                {
                    _paramsDic.Add(item, list.FirstOrDefault());
                }
                else
                {
                    _paramsDic.Add(item, "");
                }
            }
            return _paramsDic;
        }

        /// <summary>
        /// 置换请求body参数
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        private string BodyToString(HttpRequest request)
        {
            var returnValue = string.Empty;
            request.EnableBuffering();
            //ensure we read from the begining of the stream - in case a reader failed to read to end before us.
            //确保我们从流的开头开始读取-以防读卡器未能在我们之前读取到结尾。
            request.Body.Position = 0;
            //use the leaveOpen parameter as true so further reading and processing of the request body can be done down the pipeline
            //使用leaveOpen参数为true，这样就可以在管道中进一步读取和处理请求主体
            using (var stream = new StreamReader(request.Body, Encoding.UTF8, true, 1024, leaveOpen: true))
            {
                returnValue = stream.ReadToEndAsync().Result;
            }
            //reset position to ensure other readers have a clear view of the stream 
            //重置位置以确保其他读卡器可以清楚地看到流
            request.Body.Position = 0;
            return returnValue;
        }
    }

    /// <summary>
    /// LoggerMiddlewareExtensions
    /// </summary>
    public static class LoggerMiddlewareExtensions
    {
        public static IApplicationBuilder UseLoggerMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<LoggerMiddleware>();
        }
    }
}
