using Web_health_app.Models.Models;

namespace Web_health_app.ApiService.Repository
{
    public interface IPermissionRepository
    {
        /// <summary>
        /// Get all permissions with pagination and search
        /// </summary>
        Task<(List<PermissionInfoDto> Permissions, int TotalCount)> GetAllPermissionsAsync(int pageNumber = 1, int pageSize = 10, string? searchTerm = null, bool includeInactive = false);

        /// <summary>
        /// Get permission by ID
        /// </summary>
        Task<PermissionInfoDto?> GetPermissionByIdAsync(string permissionId);

        /// <summary>
        /// Create new permission
        /// </summary>
        Task<PermissionInfoDto> CreatePermissionAsync(CreatePermissionDto createPermissionDto);

        /// <summary>
        /// Update permission
        /// </summary>
        Task<PermissionInfoDto?> UpdatePermissionAsync(string permissionId, UpdatePermissionDto updatePermissionDto);

        /// <summary>
        /// Delete permission (soft delete by setting IsActive = false)
        /// </summary>
        Task<bool> DeletePermissionAsync(string permissionId);

        /// <summary>
        /// Hard delete permission (permanent removal)
        /// </summary>
        Task<bool> HardDeletePermissionAsync(string permissionId);

        /// <summary>
        /// Check if permission ID exists
        /// </summary>
        Task<bool> PermissionIdExistsAsync(string permissionId, string? excludePermissionId = null);

        /// <summary>
        /// Get permissions by Action ID
        /// </summary>
        Task<List<PermissionInfoDto>> GetPermissionsByActionAsync(string actionId);

        /// <summary>
        /// Get permissions by Entity ID
        /// </summary>
        Task<List<PermissionInfoDto>> GetPermissionsByEntityAsync(string entityId);

        /// <summary>
        /// Get permissions by Role ID
        /// </summary>
        Task<List<PermissionInfoDto>> GetPermissionsByRoleAsync(string roleId);

        /// <summary>
        /// Get available actions for dropdown
        /// </summary>
        Task<List<ActionInfoDto>> GetAvailableActionsAsync();

        /// <summary>
        /// Get available entities for dropdown
        /// </summary>
        Task<List<EntityInfoDto>> GetAvailableEntitiesAsync();
    }

    /// <summary>
    /// DTO for Action dropdown
    /// </summary>
    public class ActionInfoDto
    {
        public string ActionId { get; set; } = string.Empty;
        public string ActionName { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }

    /// <summary>
    /// DTO for Entity dropdown
    /// </summary>
    public class EntityInfoDto
    {
        public string EntityId { get; set; } = string.Empty;
        public string EntityName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }
}
