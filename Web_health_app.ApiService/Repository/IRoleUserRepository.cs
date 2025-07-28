using Web_health_app.Models.Models;

namespace Web_health_app.ApiService.Repository
{
    public interface IRoleUserRepository
    {
        /// <summary>
        /// Assign roles to user
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="roleIds">List of role IDs to assign</param>
        /// <returns>True if assignment successful</returns>
        Task<bool> AssignRolesToUserAsync(Guid userId, List<string> roleIds);

        /// <summary>
        /// Remove roles from user
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="roleIds">List of role IDs to remove</param>
        /// <returns>True if removal successful</returns>
        Task<bool> RemoveRolesFromUserAsync(Guid userId, List<string> roleIds);

        /// <summary>
        /// Get all roles assigned to a user
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>List of role information assigned to the user</returns>
        Task<List<RoleInfoDto>> GetUserRolesAsync(Guid userId);

        /// <summary>
        /// Get all users assigned to a role
        /// </summary>
        /// <param name="roleId">Role ID</param>
        /// <returns>List of user information assigned to the role</returns>
        Task<List<UserInfoDto>> GetRoleUsersAsync(string roleId);

        /// <summary>
        /// Check if user has specific role
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="roleId">Role ID</param>
        /// <returns>True if user has the role</returns>
        Task<bool> UserHasRoleAsync(Guid userId, string roleId);

        /// <summary>
        /// Replace all roles for a user
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="roleIds">New list of role IDs</param>
        /// <returns>True if replacement successful</returns>
        Task<bool> ReplaceUserRolesAsync(Guid userId, List<string> roleIds);

        /// <summary>
        /// Remove all roles from user
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>True if removal successful</returns>
        Task<bool> RemoveAllUserRolesAsync(Guid userId);
    }
}
