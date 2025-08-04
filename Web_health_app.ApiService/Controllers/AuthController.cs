using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Web_health_app.ApiService.Entities;
using Web_health_app.ApiService.Repository;
using Web_health_app.Models.Models;

namespace Web_health_app.ApiService.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly IAuthRepository _authRepository;
        private readonly IUserRepository _userRepository;

        public AuthController(IConfiguration configuration, IAuthRepository authRepository, IUserRepository userRepository)
        {
            _configuration = configuration;
            _authRepository = authRepository;
            _userRepository = userRepository;
        }


        [HttpPost("login")]
        public async Task<ActionResult<LoginResponseModel>> Login([FromBody] LoginModel request)
        {
            try
            {
                // Get client IP address
                var ipAddress = GetClientIpAddress();

                var userStatus = await _userRepository.GetUserByUsernameAsync(request.Username);
                if (userStatus.UserStatus == 0)
                {

                    return Ok(new LoginResponseModel { 
                        
                        FullName = userStatus.FullName,
                        UserName =userStatus.UserName,
                        UserStatus = userStatus.UserStatus,

                    });

                }


                // Validate user credentials using repository
                if (await _authRepository.ValidateUserCredentialsAsync(request))
                {
                    List<string> listStringcodePerm = new List<string>();
                    // Record successful login
                    await _authRepository.RecordLoginHistoryAsync(request.Username, ipAddress, true);
                    var permissions = await _authRepository.GetUserEffectivePermissionsByUsernameAsync(request.Username);
                    if (permissions != null )

                    {
                        listStringcodePerm = PermissionToStringCode(permissions);
                    }
                    var user = await _authRepository.GetUserByUsernameAsync(request.Username);

                    var token = GenerateJwtToken(request.Username, listStringcodePerm, user);



                    return Ok(new LoginResponseModel { Token = token, 
                        FullName = user?.FullName ?? "Unknown User",
                        UserName = userStatus.UserName,
                        UserStatus = userStatus.UserStatus,

                    });
                }

                // Record failed login attempt
                await _authRepository.RecordLoginHistoryAsync(request.Username, ipAddress, false);

                return Unauthorized(new { message = "Invalid username or password" });
            }
            catch (Exception ex)
            {
                // Log the exception here if you have logging configured
                // You can add logging here: _logger.LogError(ex, "Login error for user: {Username}", request.Username);
                return StatusCode(500, new { message = "An error occurred during login" });
            }
        }

        private List<string> PermissionToStringCode(List<UserPermissionDto> permissions)
        {
            var listStringcodePerm = new List<string>();    
            foreach ( var permission in permissions)
            {
                listStringcodePerm.Add(permission.action_ID+"."+permission.entity_ID);
            }
            return listStringcodePerm;
        }

        private string GenerateJwtToken(string username, List<string> listStringcodePerm = null, User user = null )
        {

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, username),
                new Claim("fullname", user?.FullName ?? "Unknown User"),
                
                // Add other claims as needed
            };

            if (listStringcodePerm != null)
            {
                foreach (var stringpermission in listStringcodePerm)
                {
                    claims.Add(new Claim(ClaimTypes.Role, stringpermission));
                }
                
            }





            string? secretKey = _configuration.GetValue<string>("Jwt:Secret");
            if (string.IsNullOrEmpty(secretKey))
            {
                throw new InvalidOperationException("JWT secret key is not configured");
            }

            var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(secretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                issuer: _configuration.GetValue<string>("Jwt:Issuer"),
                audience: _configuration.GetValue<string>("Jwt:Audience"),
                claims: claims,
                expires: DateTime.Now.AddMinutes(30),
                signingCredentials: creds
            );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private string GetClientIpAddress()
        {
            var xForwardedFor = Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (!string.IsNullOrEmpty(xForwardedFor))
            {
                return xForwardedFor.Split(',')[0].Trim();
            }

            var xRealIp = Request.Headers["X-Real-IP"].FirstOrDefault();
            if (!string.IsNullOrEmpty(xRealIp))
            {
                return xRealIp;
            }

            return HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
        }


        [HttpGet("testauthen")]
        [Authorize]
        public ActionResult<string> Get()
        {
            // This endpoint is protected and requires a valid JWT token
            return Ok("This is a protected endpoint. You are authenticated.");
        }

        [HttpGet("permissions/{username}")]
        [Authorize]
        public async Task<ActionResult> GetUserPermissions(string username)
        {
            try
            {
                var permissions = await _authRepository.GetUserEffectivePermissionsByUsernameAsync(username);

                if (!permissions.Any())
                {
                    return NotFound(new { message = "No permissions found for this user or user does not exist" });
                }

                return Ok(new
                {
                    username = username,
                    permissions = permissions,
                    totalPermissions = permissions.Count
                });
            }
            catch (Exception ex)
            {
                // Log the exception here if you have logging configured
                return StatusCode(500, new { message = "An error occurred while retrieving user permissions" });
            }
        }

        [HttpGet("permissions/current")]
        [Authorize]
        public async Task<ActionResult> GetCurrentUserPermissions()
        {
            try
            {
                // Get username from JWT token claims
                var username = User.Identity?.Name;

                if (string.IsNullOrEmpty(username))
                {
                    return BadRequest(new { message = "Cannot determine current user from token" });
                }

                var permissions = await _authRepository.GetUserEffectivePermissionsByUsernameAsync(username);

                return Ok(new
                {
                    username = username,
                    permissions = permissions,
                    totalPermissions = permissions.Count
                });
            }
            catch (Exception ex)
            {
                // Log the exception here if you have logging configured
                return StatusCode(500, new { message = "An error occurred while retrieving current user permissions" });
            }
        }

    }
}
