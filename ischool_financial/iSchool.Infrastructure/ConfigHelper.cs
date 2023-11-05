using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Linq;

namespace iSchool.Infrastructure
{
    public class ConfigHelper
    {
        private static IConfiguration _config;
    
        public static void Configure(IConfiguration config)
        {
            _config = config;
        }
   
        public static string GetConfigString(string Key)
        {
            var value = _config.GetSection("AppSettings")?[Key];
            return value;
        }
        public static int GetConfigInt(string Key)
        {
            var value = _config.GetSection("AppSettings")?[Key];
            return Convert.ToInt32(value);
        }
      
        public static string GetConfigs(params string[] sections)
        {
            try
            {

                if (sections.Any())
                {
                    return _config[string.Join(":", sections)];
                }
            }
            catch (Exception) { }

            return "";
        }
        
    }
}
