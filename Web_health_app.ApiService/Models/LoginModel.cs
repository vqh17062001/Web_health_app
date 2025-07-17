using System.ComponentModel.DataAnnotations;

namespace Web_health_app.ApiService.Models
{
    public class LoginModel
    {
        [Required]
        public string _username { get; set; }
        [Required]
        public string _password { get; set; }
    }
}
