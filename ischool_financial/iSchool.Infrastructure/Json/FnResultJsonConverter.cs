using iSchool.Domain.Modles;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace iSchool
{
    public class FnResultJsonConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType.IsGenericType && objectType.GetGenericTypeDefinition() == typeof(FnResult<>);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var j = JToken.Load(reader) as JObject;
            if (j == null) return null;

            var r = (existingValue ?? Activator.CreateInstance(objectType)) as IFnResult;
            var isok = (bool?)null;
            foreach (var p in j.Properties())
            {
                var propertyName = p.Name;
                switch (1)
                {
                    case 1 when (isok == null && string.Equals(propertyName, "isok", StringComparison.OrdinalIgnoreCase)):
                    case 1 when (isok == null && string.Equals(propertyName, "succeed", StringComparison.OrdinalIgnoreCase)):
                        {
                            isok = (bool?)p.Value == true;
                            r.Code = isok == true ? 0 : -1;
                        }
                        break;
                    case 1 when (isok == null && string.Equals(propertyName, "status", StringComparison.OrdinalIgnoreCase)):
                        {
                            var c = (int?)p.Value ?? 0;
                            r.Code = c == 200 ? 0 : c;
                            isok = r.IsOk;
                        }
                        break;
                    case 1 when (isok == null && string.Equals(propertyName, "code", StringComparison.OrdinalIgnoreCase)):
                        {
                            var c = (int?)p.Value ?? 0;
                            r.Code = c == 200 || c == 0 ? 0 : c;
                            isok = r.IsOk;
                        }
                        break;
                    case 1 when (isok == null && string.Equals(propertyName, "errcode", StringComparison.OrdinalIgnoreCase)):
                    case 1 when (isok == null && string.Equals(propertyName, "errno", StringComparison.OrdinalIgnoreCase)):
                        {
                            var code = (int?)p.Value ?? 0;
                            r.Code = code == 0 ? 0 : code;
                            isok = r.IsOk;
                        }
                        break;
                    case 1 when (string.Equals(propertyName, "msg", StringComparison.OrdinalIgnoreCase)):
                    case 1 when (string.Equals(propertyName, "errormsg", StringComparison.OrdinalIgnoreCase)):
                    case 1 when (string.Equals(propertyName, "errmsg", StringComparison.OrdinalIgnoreCase)):
                    case 1 when (string.Equals(propertyName, "errormessage", StringComparison.OrdinalIgnoreCase)):
                        {
                            r.Msg = (string)p.Value;
                        }
                        break;
                    case 1 when (string.Equals(propertyName, "data", StringComparison.CurrentCultureIgnoreCase)):
                        {
                            r.Data = p.Value.ToObject(objectType.GetGenericArguments()[0], serializer);
                        }
                        break;
                }
            }

            return r;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value == null)
            {
                writer.WriteNull();
                return;
            }

            var contractResolver = serializer.ContractResolver as DefaultContractResolver;

            writer.WriteStartObject();

            var r = (IFnResult)value;
            if (r.IsOk)
            {
                writer.WritePropertyName(GetPropertyName(contractResolver, nameof(r.IsOk)));
                writer.WriteValue(r.IsOk);
                writer.WritePropertyName(GetPropertyName(contractResolver, nameof(r.Data)));
                serializer.Serialize(writer, r.Data);
                writer.WritePropertyName(GetPropertyName(contractResolver, nameof(r.Code)));
                writer.WriteValue(r.Code);
            }
            else
            {
                writer.WritePropertyName(GetPropertyName(contractResolver, "errCode"));
                writer.WriteValue(r.Code);
                writer.WritePropertyName(GetPropertyName(contractResolver, nameof(r.Msg)));
                writer.WriteValue(r.Msg);
            }

            writer.WriteEndObject();
        }

        static string GetPropertyName(DefaultContractResolver contractResolver, string propertyName)
        {
            return contractResolver?.GetResolvedPropertyName(propertyName) ?? propertyName;
        }
    }
}
