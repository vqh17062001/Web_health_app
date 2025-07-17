namespace Web_health_app.ApiService.Models
{
    public class LoginResponseModel
    {
        public string Token { get; set; }
        public string Username { get; set; }
        public long TokenExpired { get; set; }
        //public LoginResponseModel(string token, string username, long expiration)
        //{
        //    Token = token;
        //    Username = username;
        //    TokenExpired = expiration;
        //}
    }
}
