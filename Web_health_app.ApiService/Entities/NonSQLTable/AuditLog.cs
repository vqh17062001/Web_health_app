using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;

namespace Web_health_app.ApiService.Entities.NonSQLTable
{
    public class AuditLog
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

        [BsonElement("eventAt")]
        public string EventAt { get; set; } = string.Empty; // ISO 8601 string

        [BsonElement("userId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? UserId { get; set; }

        [BsonElement("action")]
        public string Action { get; set; } = string.Empty;

        [BsonElement("resource")]
        public string Resource { get; set; } = string.Empty;

        [BsonElement("resourceId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? ResourceId { get; set; }

        [BsonElement("ipAddress")]
        public string IpAddress { get; set; } = string.Empty;

        [BsonElement("detail")]
        [BsonExtraElements]
        public Dictionary<string, object?> Detail { get; set; } = new Dictionary<string, object?>();
    }
}
