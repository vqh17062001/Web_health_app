using Web_health_app.Models.Models;

namespace Web_health_app.ApiService.Repository
{
    public interface IRoleRepository
    {
        /// <summary>
        /// Get all roles with pagination
        /// </summary>
        /// <param name="pageNumber">Page number (starting from 1)</param>
        /// <param name="pageSize">Number of items per page</param>
        /// <param name="searchTerm">Optional search term for filtering</param>
        /// <param name="includeInactive">Include inactive roles in results</param>
        /// <returns>Paginated list of roles</returns>
        Task<(List<RoleInfoDto> Roles, int TotalCount)> GetAllRolesAsync(int pageNumber = 1, int pageSize = 10, string? searchTerm = null, bool includeInactive = false);

        /// <summary>
        /// Get role by ID
        /// </summary>
        /// <param name="roleId">Role ID</param>
        /// <returns>Role information or null if not found</returns>
        Task<RoleInfoDto?> GetRoleByIdAsync(string roleId);

        /// <summary>
        /// Get all active roles (for dropdown lists)
        /// </summary>
        /// <returns>List of active roles</returns>
        Task<List<RoleInfoDto>> GetActiveRolesAsync();

        /// <summary>
        /// Create new role
        /// </summary>
        /// <param name="createRoleDto">Role creation data</param>
        /// <returns>Created role information</returns>
        Task<RoleInfoDto> CreateRoleAsync(CreateRoleDto createRoleDto);

        /// <summary>
        /// Update role information
        /// </summary>
        /// <param name="roleId">Role ID to update</param>
        /// <param name="updateRoleDto">Updated role data</param>
        /// <returns>Updated role information or null if not found</returns>
        Task<RoleInfoDto?> UpdateRoleAsync(string roleId, UpdateRoleDto updateRoleDto);

        /// <summary>
        /// Delete role (soft delete by setting IsActive to false)
        /// </summary>
        /// <param name="roleId">Role ID to delete</param>
        /// <returns>True if role deleted successfully, false if not found</returns>
        Task<bool> DeleteRoleAsync(string roleId);

        /// <summary>
        /// Permanently delete role
        /// </summary>
        /// <param name="roleId">Role ID to delete permanently</param>
        /// <returns>True if role deleted successfully, false if not found</returns>
        Task<bool> HardDeleteRoleAsync(string roleId);

        /// <summary>
        /// Check if role ID exists
        /// </summary>
        /// <param name="roleId">Role ID to check</param>
        /// <param name="excludeRoleId">Role ID to exclude from check (for updates)</param>
        /// <returns>True if role ID exists, false otherwise</returns>
        Task<bool> RoleIdExistsAsync(string roleId, string? excludeRoleId = null);

        /// <summary>
        /// Assign permissions to role
        /// </summary>
        /// <param name="roleId">Role ID</param>
        /// <param name="permissionIds">List of permission IDs to assign</param>
        /// <returns>True if assignment successful, false if role not found</returns>
        Task<bool> AssignPermissionsToRoleAsync(string roleId, List<string> permissionIds);

        /// <summary>
        /// Get roles with user count
        /// </summary>
        /// <param name="pageNumber">Page number</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="searchTerm">Search term</param>
        /// <param name="includeInactive">Include inactive roles</param>
        /// <returns>Roles with user count information</returns>
        Task<(List<RoleWithUserCountDto> Roles, int TotalCount)> GetRolesWithUserCountAsync(int pageNumber = 1, int pageSize = 10, string? searchTerm = null, bool includeInactive = false);

        /// <summary>
        /// Get permissions assigned to a role
        /// </summary>
        /// <param name="roleId">Role ID</param>
        /// <returns>List of permission IDs assigned to the role</returns>
        Task<List<string>> GetRolePermissionsAsync(string roleId);

        /// <summary>
        /// Get users assigned to a specific role
        /// </summary>
        /// <param name="roleId">Role ID</param>
        /// <returns>List of user information assigned to the role</returns>
        Task<List<UserInfoDto>> GetUsersInRoleAsync(string roleId);
    }
}
