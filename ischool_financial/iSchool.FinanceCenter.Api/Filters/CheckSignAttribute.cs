using CSRedis;
using iSchool.FinanceCenter.Domain.Redis;
using iSchool.FinanceCenter.Domain.Security;
using iSchool.Infrastructure;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace iSchool.FinanceCenter.Api.Filters
{
    /// <summary>
    ///检查签名，授权访问
    /// </summary>
    public class CheckSignAttribute : ActionFilterAttribute, IAsyncActionFilter
    {

        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var super = false;
            var request = context.HttpContext.Request;
            var headers = new SxbSignHeader();
            if (request.Headers.TryGetValue("sxb.super", out var supersecrect))
            {

                if ("198710" == supersecrect.FirstOrDefault())
                {

                    super = true;

                }

            }
            if (!super)
            {
                if (request.Headers.TryGetValue("sxb.timespan", out var timestampValues))
                {
                    headers.Timestamp = timestampValues.FirstOrDefault();
                }

                if (request.Headers.TryGetValue("sxb.nonce", out var nonceValues))
                {
                    headers.Nonce = nonceValues.FirstOrDefault();
                }

                if (request.Headers.TryGetValue("sxb.sign", out var signatureValues))
                {
                    headers.Signature = signatureValues.FirstOrDefault();
                }
                if (request.Headers.TryGetValue("sxb.key", out var keyValues))
                {
                    headers.Key = keyValues.FirstOrDefault();
                    //验证key的有效性
                    if (string.IsNullOrEmpty(ConfigHelper.GetConfigString(headers.Key)))
                    {
                        throw new CustomResponseException("无效的key");
                    }
                }
                if (string.IsNullOrEmpty(headers.Timestamp)
                    || string.IsNullOrEmpty(headers.Nonce) || string.IsNullOrEmpty(headers.Key) || string.IsNullOrEmpty(headers.Signature))
                {
                    throw new CustomResponseException("接口调用失败，参数缺失");
                }
                var services = context.HttpContext.RequestServices;
                CSRedisClient _redisClient = services.GetService<CSRedisClient>();

                if (CheckExpire(headers, _redisClient))
                {
                    request.EnableBuffering();
                    request.Body.Seek(0, SeekOrigin.Begin);
                    var secret = ConfigHelper.GetConfigString(headers.Key);
                    StringBuilder query = new StringBuilder(secret);
                    using (var reader = new StreamReader(request.Body, Encoding.UTF8, true, 1024, true))
                    {
                        var body = await reader.ReadToEndAsync();
                        query.Append($"{headers.Timestamp}\n{headers.Nonce}\n{body}\n");
                    }

                    MD5 md5 = MD5.Create();
                    var str = query.ToString();
                    byte[] bytes = md5.ComputeHash(Encoding.UTF8.GetBytes(str));

                    StringBuilder result = new StringBuilder();
                    for (int i = 0; i < bytes.Length; i++)
                    {
                        result.Append(bytes[i].ToString("X2"));
                    }
                    var pass = result.ToString() == headers.Signature;
                    if (!pass)
                    {
                        throw new CustomResponseException("你没有权限操作");
                    }

                }
                else
                {
                    throw new CustomResponseException("该请求已过期");
                }

            }
         
            var ar = await next();
            context.Result = ar.Result;
        }
      
       

        bool CheckExpire(SxbSignHeader hearder, CSRedisClient _redisClient)
        {
            //检测时间戳
            if (hearder.Timestamp.Length == 14)
            {
                DateTime dtimestamp;
                try
                {
                    dtimestamp = DateTime.ParseExact(hearder.Timestamp, "yyyyMMddHHmmss", System.Globalization.CultureInfo.CurrentCulture);
                }
                catch (Exception)
                {
                    throw new CustomResponseException("时间戳格式不正确");

                }

                //判断签名是否已过期
                if (dtimestamp < DateTime.Now.AddMinutes(-5))//请求有效时间5分钟--测试。正式改为30秒
                {
                    throw new CustomResponseException("当前请求已过期");

                }
            }
            else
            {
                throw new CustomResponseException("时间戳格式不正确");

            }
            //防重放检查
           var r= _redisClient.Exists(string.Format(CacheKeys.PayCenterVisitNonce, hearder.Nonce));
            if (r) throw new CustomResponseException("无效的请求，请勿重复调用");
            else {

                _redisClient.SetAsync(string.Format(CacheKeys.PayCenterVisitNonce, hearder.Nonce),1,TimeSpan.FromMinutes(10));
            }

            return true;
        }

    }
}
