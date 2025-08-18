using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Web_health_app.ApiService.Entities.NonSQLTable
{
    public class User
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

        [BsonElement("username")]
        public string Username { get; set; } = string.Empty;

        [BsonElement("passwordHash")]
        public string PasswordHash { get; set; } = string.Empty;

        [BsonElement("fullName")]
        public string FullName { get; set; } = string.Empty;

        [BsonElement("gender")]
        public string Gender { get; set; } = string.Empty;

        [BsonElement("Dob")]
        public string Dob { get; set; } = string.Empty; // ISODate dạng string ISO 8601

        [BsonElement("role")]
        public string Role { get; set; } = string.Empty;

        [BsonElement("department")]
        public string Department { get; set; } = string.Empty;

        [BsonElement("email")]
        public string Email { get; set; } = string.Empty;

        [BsonElement("phone")]
        public string Phone { get; set; } = string.Empty;

        [BsonElement("managerIds")]
        public string ManagerIds { get; set; } = string.Empty;

        [BsonElement("createdAt")]
        public string CreatedAt { get; set; } = string.Empty;

        [BsonElement("updatedAt")]
        public string UpdatedAt { get; set; } = string.Empty;
    }
}
