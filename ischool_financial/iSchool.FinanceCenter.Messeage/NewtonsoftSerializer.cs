using iSchool.FinanceCenter.Messeage.Serialize;
using Newtonsoft.Json;
using System.Text;

namespace iSchool.FinanceCenter.Messeage
{
    public class NewtonsoftSerializer : IMessageSerialize
    {
        private static readonly Encoding Encoding = Encoding.UTF8;

        private readonly JsonSerializerSettings _settings;

        public NewtonsoftSerializer(JsonSerializerSettings settings = null)
        {
            _settings = settings ?? new JsonSerializerSettings();
        }

        public byte[] Serialize(object item)
        {
            var type = item.GetType();
            var jsonString = JsonConvert.SerializeObject(item, type, _settings);
            return Encoding.GetBytes(jsonString);
        }

        public object Deserialize(byte[] serializedObject)
        {
            var jsonString = Encoding.GetString(serializedObject);
            return JsonConvert.DeserializeObject(jsonString, typeof(object));
        }

        public T Deserialize<T>(byte[] serializedObject)
        {
            var jsonString = Encoding.GetString(serializedObject);
            return JsonConvert.DeserializeObject<T>(jsonString, _settings);
        }
    }
}
