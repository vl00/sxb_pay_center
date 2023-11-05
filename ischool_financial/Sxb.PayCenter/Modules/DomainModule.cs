using Autofac;
using iSchool.Domain.Repository.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Sxb.PayCenter.Modules
{
    public class DomainModule : Autofac.Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            // 需要跳过的程序集列表
            string AssemblySkipLoadingPattern = "^System|^mscorlib|^Microsoft|^AjaxControlToolkit|^Antlr3|^Autofac|^NSubstitute|^AutoMapper|^Castle|^ComponentArt|^CppCodeProvider|^DotNetOpenAuth|^EntityFramework|^EPPlus|^FluentValidation|^ImageResizer|^itextsharp|^log4net|^MaxMind|^MbUnit|^MiniProfiler|^Mono.Math|^MvcContrib|^Newtonsoft|^NHibernate|^nunit|^Org.Mentalis|^PerlRegex|^QuickGraph|^Recaptcha|^Remotion|^RestSharp|^Telerik|^Iesi|^TestFu|^UserAgentStringLibrary|^VJSharpCodeProvider|^WebActivator|^WebDev|^WebGrease";
            Type baseType = typeof(IDependency);
            var assemblies = Assembly
                  .GetEntryAssembly()
                  .GetReferencedAssemblies()
                  .Select(Assembly.Load)
                  .SelectMany(x => x.DefinedTypes)
                  .Where(type => baseType.IsAssignableFrom(type.AsType()))
                  .Select(p => p.Assembly);

            var assembliesFilter = assemblies
                .Where(assembly => !Regex.IsMatch(assembly.FullName, AssemblySkipLoadingPattern, RegexOptions.IgnoreCase | RegexOptions.Compiled))
                .ToArray();//过滤程序集

            builder.RegisterAssemblyTypes(assembliesFilter)
               .Where(type => baseType.IsAssignableFrom(type) && !type.IsAbstract)
               .AsSelf() //自身服务，用于没有接口的类
               .AsImplementedInterfaces()//接口服务
               .PropertiesAutowired()//属性注入
               .InstancePerLifetimeScope();//保证生命周期基于请求
        }
    }

}
