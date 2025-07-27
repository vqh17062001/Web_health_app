using Web_health_app.Models.Models;

namespace Web_health_app.ApiService.Repository
{
    public interface IUserRepository
    {
        /// <summary>
        /// Get all users with pagination
        /// </summary>
        /// <param name="pageNumber">Page number (starting from 1)</param>
        /// <param name="pageSize">Number of items per page</param>
        /// <param name="searchTerm">Optional search term for filtering</param>
        /// <returns>Paginated list of users</returns>
        Task<(List<UserInfoDto> Users, int TotalCount)> GetAllUsersAsync(int pageNumber = 1, int pageSize = 10, string? searchTerm = null);

        /// <summary>
        /// Get user by ID
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>User information or null if not found</returns>
        Task<UserInfoDto?> GetUserByIdAsync(Guid userId);

        /// <summary>
        /// Get user by username
        /// </summary>
        /// <param name="username">Username</param>
        /// <returns>User information or null if not found</returns>
        Task<UserInfoDto?> GetUserByUsernameAsync(string username);

        /// <summary>
        /// Create new user
        /// </summary>
        /// <param name="createUserDto">User creation data</param>
        /// <returns>Created user information</returns>
        Task<UserInfoDto> CreateUserAsync(CreateUserDto createUserDto);

        /// <summary>
        /// Update user information
        /// </summary>
        /// <param name="userId">User ID to update</param>
        /// <param name="updateUserDto">Updated user data</param>
        /// <returns>Updated user information or null if not found</returns>
        Task<UserInfoDto?> UpdateUserAsync(Guid userId, UpdateUserDto updateUserDto);

        /// <summary>
        /// Delete user (soft delete by changing status)
        /// </summary>
        /// <param name="userId">User ID to delete</param>
        /// <returns>True if deleted successfully, false if not found</returns>
        Task<bool> DeleteUserAsync(Guid userId);

        /// <summary>
        /// Hard delete user (permanently remove from database)
        /// </summary>
        /// <param name="userId">User ID to delete permanently</param>
        /// <returns>True if deleted successfully, false if not found</returns>
        Task<bool> HardDeleteUserAsync(Guid userId);

        /// <summary>
        /// Check if username already exists
        /// </summary>
        /// <param name="username">Username to check</param>
        /// <param name="excludeUserId">User ID to exclude from check (for updates)</param>
        /// <returns>True if username exists, false otherwise</returns>
        Task<bool> UsernameExistsAsync(string username, Guid? excludeUserId = null);

        /// <summary>
        /// Get users managed by a specific user
        /// </summary>
        /// <param name="managerId">Manager user ID</param>
        /// <returns>List of users managed by the specified manager</returns>
        Task<List<UserInfoDto>> GetUsersByManagerAsync(Guid managerId);

        /// <summary>
        /// Change user password
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="newPassword">New password</param>
        /// <returns>True if password changed successfully, false if user not found</returns>
        Task<bool> ChangePasswordAsync(Guid userId, string newPassword);
    }
}
