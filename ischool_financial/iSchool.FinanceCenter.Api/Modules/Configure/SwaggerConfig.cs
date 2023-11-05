using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using System;
using System.IO;

namespace iSchool.FinanceCenter.Api.Modules.Configure
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

            var ApiName = "iSchool.FinanceCenter";

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
                var xmlApi = Path.Combine(AppContext.BaseDirectory, "iSchool.FinanceCenter.Api.xml");
                c.IncludeXmlComments(xmlApi, true);//默认的第二个参数是false，这个是controller的注释，记得修改

                // 获取xml注释文件的目录
                var xmlAppliaction = Path.Combine(AppContext.BaseDirectory, "iSchool.FinanceCenter.Appliaction.xml");
                c.IncludeXmlComments(xmlAppliaction, true);

                // 获取xml注释文件的目录
                var xmlDomain = Path.Combine(AppContext.BaseDirectory, "iSchool.FinanceCenter.Domain.xml");
                c.IncludeXmlComments(xmlDomain, true);

                // 获取xml注释文件的目录
                var xmlInfrastructure = Path.Combine(AppContext.BaseDirectory, "iSchool.Infrastructure.xml");
                c.IncludeXmlComments(xmlInfrastructure, true);

                //枚举信息 文档筛选器
                c.SchemaFilter<Filters.EnumDocumentFilter>();
            });

        }
    }
}
