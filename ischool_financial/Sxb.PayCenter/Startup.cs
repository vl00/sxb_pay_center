using Autofac;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using NLog.Web;
using Sxb.PayCenter.Filters;
using Sxb.PayCenter.Modules;
using Sxb.PayCenter.WechatPay;

namespace Sxb.PayCenter
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            //MediatR 
            services.AddMediatR(typeof(Startup));
            services.AddOptions();
            services.AddWeChatPay();
            //注册swagger服务
            services.AddSwaggerConfig();
            services.Configure<WeChatPayOptions>(Configuration.GetSection("WeChatPay"));
          
            services.AddControllersWithViews(o =>
            {
                //添加全局错误过滤器
                o.Filters.Add(typeof(GlobalExceptionsFilter));
           

            });
         
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                NLogBuilder.ConfigureNLog("nlog.Development.config");

          
            }

            else if (env.IsProduction())
            {


                NLogBuilder.ConfigureNLog("nlog.config");

             
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            //swagger配置
            app.UseSwagger();
            app.UseSwaggerUI(s =>
            {
                s.SwaggerEndpoint($"/swagger/v1/swagger.json", "Sxb.PayCenter V1");
                //路劲配置，设置为空，表示直接再根域名(localhost:8001)访问该文件，注意localhost:8001/swagger是访问不到的，去launchSettings.josn吧launchUrl去掉，如果你想换一个路径，直接写名字即可，比如直接写s.RoutePrefix = "swagger";
                s.RoutePrefix = "swagger";
            });
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
        /// <summary>
        /// 注册autofac module
        /// </summary>
        /// <param name="builder"></param>
        public void ConfigureContainer(ContainerBuilder builder)
        {
            builder.RegisterModule(new InfrastructureModule(Configuration.GetConnectionString("SqlServerConnection")));
            builder.RegisterModule(new DomainModule());
            builder.RegisterModule(new MediatorModule());
        }
    }
}
