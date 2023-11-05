using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace iSchool.Infrastructure.Extensions
{
    /// <summary>
    /// Decimal 返回保留两位小数
    /// </summary>
    public class DecimalConverter : JsonConverter<decimal>
    {
        /// <summary>
        /// 读
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="objectType"></param>
        /// <param name="existingValue"></param>
        /// <param name="hasExistingValue"></param>
        /// <param name="serializer"></param>
        /// <returns></returns>
        public override decimal ReadJson(JsonReader reader, Type objectType, [AllowNull] decimal existingValue, bool hasExistingValue, JsonSerializer serializer)
            => decimal.Parse(reader.Value?.ToString());

        /// <summary>
        /// 写
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="value"></param>
        /// <param name="serializer"></param>
        public override void WriteJson(JsonWriter writer, [AllowNull] decimal value, JsonSerializer serializer)
        {
            var values = value.ToString().Split(".");
            if (values.Length == 2)
            {
                if (values[1].Length > 2)
                {
                    var res = values[0] + "." + values[1].Substring(0, 2);
                    writer.WriteValue(res);
                }
            }
            else {
                writer.WriteValue(value);
            }
        }
    }
}
