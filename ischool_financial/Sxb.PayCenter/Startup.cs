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
            //ע��swagger����
            services.AddSwaggerConfig();
            services.Configure<WeChatPayOptions>(Configuration.GetSection("WeChatPay"));
          
            services.AddControllersWithViews(o =>
            {
                //���ȫ�ִ��������
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
            //swagger����
            app.UseSwagger();
            app.UseSwaggerUI(s =>
            {
                s.SwaggerEndpoint($"/swagger/v1/swagger.json", "Sxb.PayCenter V1");
                //·�����ã�����Ϊ�գ���ʾֱ���ٸ�����(localhost:8001)���ʸ��ļ���ע��localhost:8001/swagger�Ƿ��ʲ����ģ�ȥlaunchSettings.josn��launchUrlȥ����������뻻һ��·����ֱ��д���ּ��ɣ�����ֱ��дs.RoutePrefix = "swagger";
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
        /// ע��autofac module
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
