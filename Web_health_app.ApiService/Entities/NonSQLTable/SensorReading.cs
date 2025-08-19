using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

namespace Web_health_app.ApiService.Entities.NonSQLTable
{
    public class SensorReading
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

        [BsonElement("timestamp")]
        public DateTimeOffset Timestamp { get; set; }

        [BsonElement("metadata")]
        public Metadata Metadata { get; set; } = new Metadata();

        [BsonElement("readings")]
        public List<Reading> Readings { get; set; } = new List<Reading>();
    }

    public class Metadata
    {
        [BsonElement("userId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string UserId { get; set; } = string.Empty;

        [BsonElement("deviceId")]
        public string DeviceId { get; set; } = string.Empty;

        [BsonElement("sensorType")]
        public string? SensorType { get; set; }
    }

    public class Reading
    {
        [BsonElement("key")]
        public string Key { get; set; } = string.Empty;

        [BsonElement("value")]
        public BsonValue Value { get; set; } = BsonNull.Value;
    }

  
}
