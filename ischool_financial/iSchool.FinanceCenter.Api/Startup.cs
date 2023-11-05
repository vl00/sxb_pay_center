using System;
using System.Text;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using iSchool.FinanceCenter.Api.Configurations;
using iSchool.FinanceCenter.Api.Extension;
using iSchool.FinanceCenter.Api.Filters;
using iSchool.FinanceCenter.Api.Middlewares;
using iSchool.FinanceCenter.Api.Modules.AutofacModule;
using iSchool.FinanceCenter.Api.Modules.Configure;
using iSchool.FinanceCenter.Appliaction.Http;
using iSchool.FinanceCenter.Domain.Settings;
using iSchool.FinanceCenter.Hanlder;
using iSchool.FinanceCenter.Messeage;
using iSchool.FinanceCenter.Messeage.Config;
using iSchool.Infrastructure;
using iSchool.Infrastructure.Extensions;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NLog.Web;
using Polly;
using Sxb.GenerateNo;
using Sxb.PayCenter.WechatPay;

namespace iSchool.FinanceCenter.Api
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;


        }

        public IConfiguration Configuration { get; }
        public ILifetimeScope AutofacContainer { get; private set; }

        public void ConfigureServices(IServiceCollection services)
        {
            ConfigHelper.Configure(Configuration);

            //避免中文乱码
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);  //避免日志中的中文输出乱码

            //services.AddControllersWithViews();
            services.AddHttpClient();//添加client factory

            //微信支付
            services.AddWeChatPay();
            services.AddFinanceCenterRabbitMQ(option =>
            {
                var config = Configuration.GetSection("rabbitMQSetting").Get<RabbitMQOption>();
                option.AmqpUris = config.AmqpUris;
                option.Uri = config.Uri;
                option.ExtName = config.ExtName;
            }, new NewtonsoftSerializer())
                .ScanMessage(typeof(WalletOpreateFaileHandle).Assembly, this.GetType().Assembly); ;
            services.Configure<WeChatPayOptions>(Configuration.GetSection("WeChatPay"));
            //注入全局异常处理
            services.AddMvc(option =>
            {
                option.Filters.Add(typeof(GlobalExceptionsFilter));
            }).AddNewtonsoftJson(options =>
            {
                options.SerializerSettings.ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver();
                options.SerializerSettings.Converters.Add(new DecimalConverter());

            });

            //automapper
            services.AddAutoMapperSetup();



            // httpcontext
            services.AddHttpContextAccessor();
#if DEBUG
            //注册swagger服务
            services.AddSwaggerConfig();
#endif


            // csredis
            services.AddSingleton(sp => new CSRedis.CSRedisClient(Configuration["redis:0"]));

            services.AddSingleton<ISxbGenerateNo, SxbGenerateNo>();

            //内部服务接口
            //services.AddScoped<IInsideHttpRepository, InsideHttpRepository>();
            services.AddSingleton(Configuration.GetSection("HttpApiConfigs").Get<HttpApiConfigs>());
            services.AddHttpClient<IInsideHttpRepository, InsideHttpRepository>()

                 .AddTransientHttpErrorPolicy(b => b.WaitAndRetryAsync(3, times =>
                 {
                     return TimeSpan.FromSeconds(times * 30);
                 }));
            services.AddHttpClient<IGaoDengHttpRepository, GaoDengHttpRepository>();
            //services.AddHttpClient<IWechatAppRepository, WechatAppRepository>()
            //     .AddTransientHttpErrorPolicy(b => b.WaitAndRetryAsync(3, times =>
            //     {
            //         return TimeSpan.FromSeconds(times * 30);
            //     }));
            //add cors
            services.AddCors(options =>
            {
                options.AddPolicy("Cors", configurePolicy =>
                {
                    configurePolicy
                    .SetIsOriginAllowed(isOriginAllowed => true)
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials()
                    //.AllowAnyOrigin()
                    //.WithOrigins("http://localhost:8001", "http://localhost:8002")
                    ;
                });
            });
            services.Configure<WechatMessageTplSetting>(Configuration.GetSection("WechatMessageTplSetting"));
            //MediatR 
            services.AddMediatR(typeof(Startup));

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IHostApplicationLifetime applicationLifetime)
        {

            applicationLifetime.ApplicationStarted.Register(o =>
            {
                var t = ((Tuple<Startup, IServiceProvider>)o);
                t.Item1.OnApplicationStarted(t.Item2);
            }, Tuple.Create(this, app.ApplicationServices), false);
            applicationLifetime.ApplicationStopping.Register(o =>
            {
                var t = ((Tuple<Startup, IServiceProvider>)o);
                t.Item1.OnApplicationStopping(t.Item2);
            }, Tuple.Create(this, app.ApplicationServices), false);

            app.Use(next => context =>
            {
                context.Request.EnableBuffering();
                return next(context);
            });

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            #region Nlog记日志
            //将日志记录到数据库
            //使用NLog作为日志记录工具
            if (env.IsDevelopment())
            {
                NLogBuilder.ConfigureNLog("nlog.Development.config");
                app.UseDeveloperExceptionPage();
            }
            else if (env.IsProduction())
            {
                NLogBuilder.ConfigureNLog("nlog.config");
            }
            if (1 == ConfigHelper.GetConfigInt("RabbitMqCousumer"))
            {

                //rabbitemq消费
                app.SubscribeRabbitMQ();
            }
           
            //在nlog 配置文件中配置链接日志连接字符串
            NLog.LogManager.Configuration.Variables["connectionString"] = Configuration["ConnectionStrings:LogSqlServerConnection"];
            #endregion

            if (env.IsDevelopment())
            {
                //swagger配置
                app.UseSwagger();
                app.UseSwaggerUI(s =>
                {
                    s.SwaggerEndpoint($"/swagger/v1/swagger.json", "iSchool.FinanceCenter V1");
                    //路劲配置，设置为空，表示直接再根域名(localhost:8001)访问该文件，注意localhost:8001/swagger是访问不到的，去launchSettings.josn吧launchUrl去掉，如果你想换一个路径，直接写名字即可，比如直接写s.RoutePrefix = "swagger";
                    s.RoutePrefix = "swagger";
                });
            }

            //添加日志中间件
            app.UseLoggerMiddleware();

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCors("Cors");

            app.UseCookiePolicy();
            app.UseRouting();

            app.UseCookiePolicy();
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

            builder.RegisterModule(new InfrastructureModule(Configuration.GetConnectionString("SqlServerConnection"), Configuration.GetConnectionString("SqlServer-Financial-readonly")));
            builder.RegisterModule(new DomainModule());
            builder.RegisterModule(new MediatorModule());
        }

        private void OnApplicationStarted(IServiceProvider services)
        {
            this.AutofacContainer = services.GetAutofacRoot();
            var serviceScopeFactory = services.GetService<IServiceScopeFactory>();
            AsyncUtils.SetServiceScopeFactory(serviceScopeFactory);
            SimpleQueue_Extension.ServiceScopeFactory = serviceScopeFactory;
            //RedisHelper.Initialization(services.GetService<CSRedis.CSRedisClient>());
        }

        /// <summary>
        /// on app stop
        /// </summary>
        private void OnApplicationStopping(IServiceProvider services)
        {
            //...
        }
    }
}
