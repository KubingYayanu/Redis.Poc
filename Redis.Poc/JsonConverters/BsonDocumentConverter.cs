using System.Text.Json;
using System.Text.Json.Serialization;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;

namespace Redis.Poc.JsonConverters
{
    public class BsonDocumentConverter : JsonConverter<BsonDocument>
    {
        public override BsonDocument? Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options)
        {
            var json = reader.GetString();
            if (json == null)
            {
                return null;
            }

            var bson = BsonSerializer.Deserialize<BsonDocument>(json);

            return bson;
        }

        public override void Write(
            Utf8JsonWriter writer,
            BsonDocument value,
            JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToJson());
        }
    }
}