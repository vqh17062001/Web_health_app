namespace Web_health_app.Models.Models
{
    public class UserPermissionDto
    {
        public string action_ID { get; set; }
        public string entity_ID { get; set; }
        public byte entity_security_level { get; set; }
        public string role_ID { get; set; }
        public bool isActive { get; set; }

    }
}
