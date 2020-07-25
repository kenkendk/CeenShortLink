using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace server
{
    /// <summary>
    /// Helper class to support null values for dates
    /// </summary>
    class NullDateJsonConverter : IsoDateTimeConverter
    {
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
                return new DateTime();

            return base.ReadJson(reader, objectType, existingValue, serializer);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value is DateTime dt && dt.Ticks == 0)
            {
                writer.WriteNull();
                return;
            }

            base.WriteJson(writer, value, serializer);
        }
    }

    class Program
    {
        static int Main(string[] args)
        {
            
            JsonConvert.DefaultSettings = () =>
            {
                var res = new JsonSerializerSettings()
                {                
                    ContractResolver = new CamelCasePropertyNamesContractResolver(),                    
                };

                res.Converters.Add(new NullDateJsonConverter());

                return res;
            };

            return Ceen.Httpd.Cli.Program.Main(args);
        }
    }
}
