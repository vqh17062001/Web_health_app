namespace Web_health_app.Models.Models
{
    public class LoginResponseModel
    {
        public string Token { get; set; }
        public string UserName { get; set; }
        public string FullName { get; set; }
        public short UserStatus { get; set; }
        public long TokenExpired { get; set; }


      
    }
}
