using Microsoft.Extensions.Configuration;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.Infrastructure
{
    public static class ConfigurationExtension
    {
        public static T Bind<T>(this IConfiguration configuration, string key, T instance, Action<IConfiguration, T> fix) where T : class
        {
            var st = configuration.GetSection(key);
            st.Bind(instance);
            fix?.Invoke(st, instance);
            return instance;
        }

        public static T Bind<T>(this IConfiguration configuration, T instance, Action<IConfiguration, T> fix) where T : class
        {
            configuration.Bind(instance);
            fix?.Invoke(configuration, instance);
            return instance;
        }
    }
}