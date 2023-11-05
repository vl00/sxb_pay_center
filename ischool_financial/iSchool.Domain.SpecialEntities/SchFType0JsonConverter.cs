using iSchool;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace iSchool
{
    public class SchFType0JsonConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(SchFType0) || objectType == typeof(SchFType0?);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var j = JToken.Load(reader);
            if (j.Type == JTokenType.Null) return null;
            if (j.Type != JTokenType.String) throw new JsonSerializationException("can't serialize to SchFType0");
            var str = (j as JValue).ToString();
            return SchFType0.Parse(str);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value == null) writer.WriteNull();
            else writer.WriteValue(value.ToString());
        }
    }
}
