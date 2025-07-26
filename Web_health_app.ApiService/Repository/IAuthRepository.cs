using Web_health_app.ApiService.Entities;
using Web_health_app.ApiService.Entities.NonTable;
using Web_health_app.Models.Models;

namespace Web_health_app.ApiService.Repository
{
    public interface IAuthRepository
    {
        /// <summary>
        /// Validates user credentials
        /// </summary>
        /// <param name="request">Login request containing username and password</param>
        /// <returns>True if credentials are valid, false otherwise</returns>
        Task<bool> ValidateUserCredentialsAsync(LoginModel request);

        /// <summary>
        /// Gets user by username
        /// </summary>
        /// <param name="username">Username to search for</param>
        /// <returns>User entity if found, null otherwise</returns>
        Task<User?> GetUserByUsernameAsync(string username);

        /// <summary>
        /// Gets user by username and password
        /// </summary>
        /// <param name="username">Username</param>
        /// <param name="password">Password</param>
        /// <returns>User entity if found, null otherwise</returns>
        Task<User?> GetUserByCredentialsAsync(string username, string password);

 

        /// <summary>
        /// Records login history
        /// </summary>
        /// <param name="username">Username</param>
        /// <param name="ipAddress">IP address of the login attempt</param>
        /// <param name="isSuccess">Whether the login was successful</param>
        /// <returns>Task</returns>
        Task RecordLoginHistoryAsync(string username, string ipAddress, bool isSuccess);

        /// <summary>
        /// Gets user effective permissions by user ID using stored procedure
        /// </summary>
        /// <param name="userId">User ID to get permissions for</param>
        /// <returns>List of UserPermissionDto</returns>
        Task<List<UserPermissionDto>> GetUserEffectivePermissionsAsync(Guid userId);

        /// <summary>
        /// Gets user effective permissions by username using stored procedure
        /// </summary>
        /// <param name="username">Username to get permissions for</param>
        /// <returns>List of UserPermissionDto</returns>
        Task<List<UserPermissionDto>> GetUserEffectivePermissionsByUsernameAsync(string username);
    }
}
