using System;


using Newtonsoft.Json;

namespace iSchool.Infrastructure
{
    /// <summary>
    ///     Defines helper methods to work with JSON.
    /// </summary>
    public static class JsonSerializationHelper
    {
        private const char TypeSeperator = '|';

        /// <summary>
        ///     Serializes an object with a type information included.
        ///     So, it can be deserialized using <see cref="DeserializeWithType" /> method later.
        /// </summary>
        public static string SerializeWithType(object obj)
        {
            return SerializeWithType(obj, obj.GetType());
        }

        /// <summary>
        ///     Serializes the specified object.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <returns></returns>
        public static string Serialize(object obj)
        {
            string serialized = obj.ToJsonString();
            return serialized;
        }

        /// <summary>
        ///     Serializes an object with a type information included.
        ///     So, it can be deserialized using <see cref="DeserializeWithType" /> method later.
        /// </summary>
        public static string SerializeWithType(object obj, Type type)
        {
            string serialized = obj.ToJsonString();

            return $"{type.AssemblyQualifiedName}{TypeSeperator}{serialized}";
        }

        /// <summary>
        ///     Deserializes an object serialized with <see cref="SerializeWithType(object)" /> methods.
        /// </summary>
        public static T DeserializeWithType<T>(string serializedObj)
        {
            return (T)DeserializeWithType(serializedObj);
        }

        /// <summary>
        ///     Deserializes an object serialized with <see cref="SerializeWithType(object)" /> methods.
        /// </summary>
        public static object DeserializeWithType(string serializedObj)
        {
            int typeSeperatorIndex = serializedObj.IndexOf(TypeSeperator);
            Type type = Type.GetType(serializedObj.Substring(0, typeSeperatorIndex));
            string serialized = serializedObj.Substring(typeSeperatorIndex + 1);

            var options = new JsonSerializerSettings();
            options.Converters.Insert(0, new StoveDateTimeConverter());

            return JsonConvert.DeserializeObject(serialized, type, options);
        }

        /// <summary>
        ///     Deserializes the specified serialized object.
        /// </summary>
        /// <param name="serializedObj">The serialized object.</param>
        /// <returns></returns>
        public static object Deserialize(string serializedObj)
        {
            var options = new JsonSerializerSettings();
            options.Converters.Insert(0, new StoveDateTimeConverter());

            return JsonConvert.DeserializeObject(serializedObj, options);
        }

        /// <summary> 
        /// JSON文本转对象,泛型方法 
        /// </summary> 
        /// <typeparam name="T">类型</typeparam> 
        /// <param name="jsonText">JSON文本</param> 
        /// <returns>指定类型的对象</returns> 
        public static T JSONToObject<T>(string jsonText)
        {
            //JavaScriptSerializer jss = new JavaScriptSerializer();
            try
            {
               
                //var json = jsonText.Replace("\"\\\"[", "[").Replace("]\\\"\"", "]")
                //    .Replace(" \\","").Replace("\\", "")
                //    .Replace("\"[", "[").Replace("]\"", "]");
                //json = json.Replace("\"\"[", "[").Replace("]\"\"", "]");
                
                return JsonConvert.DeserializeObject<T>(jsonText);
                
            }
            catch (Exception ex)
            {
                throw new Exception("JSONHelper.JSONToObject(): " + ex.Message);
            }
        }

        /// <summary> 
        /// [字段同步专用]
        /// JSON文本转对象,泛型方法 
        /// </summary> 
        /// <typeparam name="T">类型</typeparam> 
        /// <param name="jsonText">JSON文本</param> 
        /// <returns>指定类型的对象</returns> 
        public static T JSONToObjectOfExtFieldsSync<T>(string jsonText)
        {            
            try
            {

                var json = jsonText.Replace("\"\\\"[", "[").Replace("]\\\"\"", "]")
                    .Replace(" \\", "").Replace("\\", "")
                    .Replace("\"[", "[").Replace("]\"", "]");
                json = json.Replace("\"\"[", "[").Replace("]\"\"", "]");
                return JsonConvert.DeserializeObject<T>(json);
            }
            catch (Exception ex)
            {
                throw new Exception("JSONHelper.JSONToObject(): " + ex.Message);
            }
        }

    }
}
