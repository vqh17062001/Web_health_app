using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Web_health_app.ApiService.Entities;
using Web_health_app.Models.Models;

namespace Web_health_app.ApiService.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class AuthController (IConfiguration configuration) : ControllerBase
    {
       

        [HttpPost("login")]
      
        public ActionResult<LoginResponseModel> Login([FromBody] LoginModel request)
        {
            // Simulate a login process
            if (CheckUsernamePassword(request))

            {
                var token = GenerateJwtToken(request.Username);
                return Ok(new LoginResponseModel{ Token = token });
            }
            return null;
        }

        private string GenerateJwtToken(string username)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.Name, username),
                new Claim(ClaimTypes.Role, "Admin"),

            

                
                // Add other claims as needed
            };
            string secretKey  = configuration.GetValue< string>("Jwt:Secret");
            var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(secretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                issuer: configuration.GetValue<string>("Jwt:Issuer"),
                audience: configuration.GetValue<string>("Jwt:Audience"),
                claims: claims,
                expires: DateTime.Now.AddMinutes(30),
                signingCredentials: creds
                
            );
            return new JwtSecurityTokenHandler().WriteToken(token) ;
        }
        private bool CheckUsernamePassword(LoginModel request) {

            using (HealthDbContext context = new HealthDbContext())
            {
                var hasvalue = context.Users.FirstOrDefault(u => u.UserName == request.Username && u.PasswordHash == request.Password);
                if (hasvalue == null)
                {
                    return false; // User not found or password does not match
                }
                else
                {
                    return true; // User found and password matches


                }
            }

                 // Replace with actual username/password check logic
        }





        [HttpGet("testauthen")]
        [Authorize]
        public ActionResult<string> Get()
        {
            // This endpoint is protected and requires a valid JWT token
            return Ok("This is a protected endpoint. You are authenticated.");
        }

    }
}
