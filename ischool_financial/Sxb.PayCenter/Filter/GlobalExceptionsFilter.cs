using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using NLog;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using Logger = NLog.Logger;

namespace Sxb.PayCenter.Filters
{

    /// <summary>
    /// 全局错误日志
    /// </summary>
    public class GlobalExceptionsFilter : IExceptionFilter
    {
        public void OnException(ExceptionContext actionExecutedContext)
        {

            Logger _logger = LogManager.GetCurrentClassLogger();

            //获取错误信息
            var exption = actionExecutedContext.Exception;

            _logger.Error(exption);

        }




        public string BodyToString(HttpRequest request)
        {
            var returnValue = string.Empty;
            //request.EnableRewind();
            //ensure we read from the begining of the stream - in case a reader failed to read to end before us.
            request.Body.Position = 0;
            //use the leaveOpen parameter as true so further reading and processing of the request body can be done down the pipeline
            using (var stream = new StreamReader(request.Body, Encoding.UTF8, true, 1024, leaveOpen: true))
            {
                returnValue = stream.ReadToEndAsync().Result;
            }
            //reset position to ensure other readers have a clear view of the stream 
            request.Body.Position = 0;
            return returnValue;
        }
    }
}
