using Autofac;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace iSchool.Infrastructure
{
    public static class MsDIExtension
    {
        public static IServiceProvider NewScope(this IServiceProvider serviceProvider)
        {
            return serviceProvider.CreateScope().ServiceProvider;
        }

        public static void Dispose(this IServiceProvider serviceProvider)
        {
            (serviceProvider as IDisposable)?.Dispose();
        }

        public static T GetService<T>(this IServiceProvider services, string name)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));
            var ax = services.GetService<IComponentContext>();
            return ax.ResolveNamed<T>(name);
        }        
    }
}