using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;

namespace Web_health_app.Web.Services
{
    public class JwtTokenService : IJwtTokenService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<JwtTokenService> _logger;

        public JwtTokenService(IHttpContextAccessor httpContextAccessor, ILogger<JwtTokenService> logger)
        {
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public async Task StoreTokenAsync(string token)
        {
            var httpContext = _httpContextAccessor.HttpContext;

            if (httpContext == null)
            {
                _logger.LogError("HttpContext is null");
                return;
            }

            // Kiểm tra xem response đã được gửi chưa
            if (httpContext.Response.HasStarted)
            {
                _logger.LogWarning("Cannot store token - response has already started");
                return;
            }

            try
            {
                // Tạo claims cho user
                var claims = new List<Claim>
                {
                    new Claim("jwt_token", token),
                    new Claim(ClaimTypes.Authentication, "true")
                };

                // Tạo ClaimsIdentity
                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

                // Cấu hình AuthenticationProperties
                var authProperties = new AuthenticationProperties
                {
                    ExpiresUtc = DateTimeOffset.UtcNow.AddHours(1),
                    IsPersistent = true,
                    AllowRefresh = true
                };

                // Sign in user
                await httpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    claimsPrincipal,
                    authProperties);

                _logger.LogInformation("Token stored successfully");
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("Headers are read-only"))
            {
                _logger.LogWarning("Cannot store token - headers are read-only: {Message}", ex.Message);
                // Có thể store token vào session hoặc cache thay thế
                await StoreTokenInSessionAsync(httpContext, token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error storing token: {Message}", ex.Message);
                throw;
            }
        }

        private async Task StoreTokenInSessionAsync(HttpContext httpContext, string token)
        {
            try
            {
                // Fallback: Store in session if available
                if (httpContext.Session != null)
                {
                    httpContext.Session.SetString("jwt_token", token);
                    await httpContext.Session.CommitAsync();
                    _logger.LogInformation("Token stored in session as fallback");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to store token in session");
            }
        }

        public async Task<string?> GetTokenAsync()
        {
            var httpContext = _httpContextAccessor.HttpContext;

            if (httpContext == null)
            {
                return null;
            }

            try
            {
                // Thử lấy từ authentication claims trước
                var user = httpContext.User;
                if (user?.Identity?.IsAuthenticated == true)
                {
                    var tokenClaim = user.FindFirst("jwt_token");
                    if (tokenClaim != null)
                    {
                        return tokenClaim.Value;
                    }
                }

                // Fallback: Lấy từ session
                if (httpContext.Session != null)
                {
                    return httpContext.Session.GetString("jwt_token");
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving token");
                return null;
            }
        }

        public async Task RemoveTokenAsync()
        {
            var httpContext = _httpContextAccessor.HttpContext;

            if (httpContext == null)
            {
                return;
            }

            try
            {
                // Kiểm tra xem response đã được gửi chưa
                if (!httpContext.Response.HasStarted)
                {
                    await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                }

                // Xóa từ session
                if (httpContext.Session != null)
                {
                    httpContext.Session.Remove("jwt_token");
                    await httpContext.Session.CommitAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing token");
            }
        }
    }
}