using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Sxb.PayCenter.Modules
{
    /// <summary>
    /// SwaggerConfig
    /// </summary>
    public static class SwaggerConfig
    {
        /// <summary>
        /// AddSwaggerConfig
        /// </summary>
        /// <param name="services"></param>
        public static void AddSwaggerConfig(this IServiceCollection services)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));

            var ApiName = "Sxb.PayCenter";

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    // {ApiName} 定义成全局变量，方便修改
                    Version = "v1",
                    Title = $"{ApiName} 接口文档",
                    Description = $"{ApiName} HTTP API V1",

                });
                c.OrderActionsBy(o => o.RelativePath);

                // 获取xml注释文件的目录
                var xmlApi = Path.Combine(AppContext.BaseDirectory, "Sxb.PayCenter.xml");
                c.IncludeXmlComments(xmlApi, true);//默认的第二个参数是false，这个是controller的注释，记得修改

                // 获取xml注释文件的目录
                var xmlAppliaction = Path.Combine(AppContext.BaseDirectory, "Sxb.PayCenter.Application.xml");
                c.IncludeXmlComments(xmlAppliaction, true);

                // 获取xml注释文件的目录
                var xmlDomain = Path.Combine(AppContext.BaseDirectory, "iSchool.FinanceCenter.Domain.xml");
                c.IncludeXmlComments(xmlDomain, true);

                // 获取xml注释文件的目录
                var xmlInfrastructure = Path.Combine(AppContext.BaseDirectory, "iSchool.Infrastructure.xml");
                c.IncludeXmlComments(xmlInfrastructure, true);
            });

        }
    }
}
