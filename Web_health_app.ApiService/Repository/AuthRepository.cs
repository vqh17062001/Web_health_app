using Microsoft.EntityFrameworkCore;
using Web_health_app.ApiService.Entities;
using BCrypt.Net;
using Web_health_app.Models.Models;

namespace Web_health_app.ApiService.Repository
{
    public class AuthRepository : IAuthRepository
    {
        private readonly HealthDbContext _context;

        public AuthRepository(HealthDbContext context)
        {
            _context = context;
        }

        public async Task<bool> ValidateUserCredentialsAsync(LoginModel request)
        {
            try
            {
                var user = await _context.Users.Where(u => u.UserStatus != -1 && u.UserStatus!=-2 && u.UserStatus != 2)
                    .FirstOrDefaultAsync(u => u.UserName == request.Username);
                if (user.UserStatus != 0)
                {
                   return PasswordHasher.VerifyPassword(request.Password,user.PasswordHash);
                }
                else { 

                    return user != null;


                }


               
            }
            catch (Exception ex)
            {
                // Log exception here if you have logging configured
                throw new Exception("Error validating user credentials", ex);
            }
        }

        public async Task<User?> GetUserByUsernameAsync(string username)
        {
            try
            {
                return await _context.Users
                    .FirstOrDefaultAsync(u => u.UserName == username);
            }
            catch (Exception ex)
            {
                // Log exception here if you have logging configured
                throw new Exception("Error getting user by username", ex);
            }
        }

        public async Task<User?> GetUserByCredentialsAsync(string username, string password)
        {
            try
            {
                return await _context.Users
                    .FirstOrDefaultAsync(u => u.UserName == username && u.PasswordHash == password);
            }
            catch (Exception ex)
            {
                // Log exception here if you have logging configured
                throw new Exception("Error getting user by credentials", ex);
            }
        }


      

        public async Task RecordLoginHistoryAsync(string username, string ipAddress, bool isSuccess)
        {
            try
            {
                var user = await GetUserByUsernameAsync(username);
                if (user != null)
                {
                    var loginHistory = new LoginHistory
                    {
                        LoginHisId = Guid.NewGuid().ToString(),
                        UserId = user.UserId,
                        LoginTime = DateTime.Now,
                        IpAddress = ipAddress,
                        StatusLogin = (byte)(isSuccess ? 1 : 0) // 1 for success, 0 for failure
                    };

                    _context.LoginHistorys.Add(loginHistory);
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                // Log exception here if you have logging configured
                throw new Exception("Error recording login history", ex);
            }
        }

        public async Task<List<UserPermissionDto>> GetUserEffectivePermissionsAsync(Guid userId)
        {
            try
            {
                var lisPermissions = await _context.Database
                    .SqlQueryRaw<UserPermissionDto>(
                        "EXEC dbo.usp_GetUserEffectivePermissions @User_ID = {0}",
                        userId.ToString())
                    .ToListAsync();

                return lisPermissions;
            }
            catch (Exception ex)
            {
                // Log exception here if you have logging configured
                throw new Exception("Error getting user effective permissions", ex);
            }
        }

        public async Task<List<UserPermissionDto>> GetUserEffectivePermissionsByUsernameAsync(string username)
        {
            try
            {
                // First get the user to obtain their ID
                var user = await GetUserByUsernameAsync(username);
                if (user == null)
                {
                    return new List<UserPermissionDto>(); // Return empty list if user not found
                }

                // Use the user ID to get permissions
                return await GetUserEffectivePermissionsAsync(user.UserId);
            }
            catch (Exception ex)
            {
                // Log exception here if you have logging configured
                throw new Exception("Error getting user effective permissions by username", ex);
            }
        }
    }


    public class PasswordHasher
    {
        // Băm mật khẩu
        public static string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password, 12); // 12 là work factor
        }

        // Xác minh mật khẩu
        public static bool VerifyPassword(string password, string hash)
        {
            return BCrypt.Net.BCrypt.Verify(password, hash);
        }
    }
}
