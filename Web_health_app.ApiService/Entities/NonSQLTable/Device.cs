using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Web_health_app.ApiService.Entities.NonSQLTable
{
    public class Device
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

        [BsonElement("deviceId")]
        public string DeviceId { get; set; } = string.Empty;

        [BsonElement("ownerId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string OwnerId { get; set; } = string.Empty;

        [BsonElement("model")]
        public string Model { get; set; } = string.Empty;

        [BsonElement("osVersion")]
        public string OsVersion { get; set; } = string.Empty;

        [BsonElement("sdkVersion")]
        public string SdkVersion { get; set; } = string.Empty;

        [BsonElement("registeredAt")]
        public string RegisteredAt { get; set; } = string.Empty; // ISO 8601 string

        [BsonElement("lastSyncAt")]
        public string LastSyncAt { get; set; } = string.Empty; // ISO 8601 string

        [BsonElement("status")]
        public string Status { get; set; } = string.Empty; // "online" hoặc "offline"
    }
}
