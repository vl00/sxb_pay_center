using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;

namespace iSchool.Infrastructure
{
    public static class JsonExtensions
    {
        static readonly CamelCasePropertyNamesContractResolver camelCasePropertyNamesContractResolver;

        public static readonly IList<JsonConverter> Converters;

        static JsonExtensions()
        {
            camelCasePropertyNamesContractResolver = new CamelCasePropertyNamesContractResolver();
            Converters = new List<JsonConverter>();
            Converters.Insert(0, new StoveDateTimeConverter());
        }

        /// <summary>
        ///     Converts given object to JSON string.
        /// </summary>
        /// <returns></returns>
        public static string ToJsonString(this object obj, bool camelCase = false, bool indented = false, bool ignoreNull = false)
        {
            if (obj == null) return null; //null is default json to "null"

            var options = new JsonSerializerSettings();

            if (camelCase)
            {
                options.ContractResolver = camelCasePropertyNamesContractResolver;
            }
            if (indented)
            {
                options.Formatting = Formatting.Indented;
            }
            options.NullValueHandling = ignoreNull ? NullValueHandling.Ignore : NullValueHandling.Include;

            options.Converters = Converters;

            return JsonConvert.SerializeObject(obj, options);
        }

        public static T ToObject<T>(this string json, bool camelCase = false)
        {
            if (json == null) return (T)(object)null;

            var options = new JsonSerializerSettings();

            if (camelCase)
            {
                options.ContractResolver = camelCasePropertyNamesContractResolver;
            }

            options.Converters = Converters;

            return (T)JsonConvert.DeserializeObject(json, typeof(T), options);
        }

        public static object ToObject(string json, Type type)
        {
            if (json == null) return null;

            var options = new JsonSerializerSettings();

            options.Converters = Converters;

            return JsonConvert.DeserializeObject(json, type, options);
        }
    }
}
