using System.ComponentModel.DataAnnotations;

namespace Web_health_app.Models.Models
{
    /// <summary>
    /// Data Transfer Object for Permission Information
    /// </summary>
    public class PermissionInfoDto
    {

        [StringLength(50, ErrorMessage = "Permission ID cannot exceed 50 characters")]
        public string PermissionId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Permission name is required")]
        [StringLength(100, ErrorMessage = "Permission name cannot exceed 100 characters")]
        public string PermissionName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Action ID is required")]
        [StringLength(50, ErrorMessage = "Action ID cannot exceed 50 characters")]
        public string ActionId { get; set; } = string.Empty;

        public string ActionName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Entity ID is required")]
        [StringLength(50, ErrorMessage = "Entity ID cannot exceed 50 characters")]
        public string EntityId { get; set; } = string.Empty;

        public string EntityName { get; set; } = string.Empty;

        [StringLength(50, ErrorMessage = "Time Active ID cannot exceed 50 characters")]
        public string? TimeActiveId { get; set; }

        [StringLength(50, ErrorMessage = "Role ID cannot exceed 50 characters")]
        public string? RoleId { get; set; }

        public string? RoleName { get; set; }

        public bool IsActive { get; set; }

        /// <summary>
        /// Combined permission display name (Action.Entity format)
        /// </summary>
        public string DisplayName => $"{ActionName}.{EntityName}";
    }

    /// <summary>
    /// DTO for creating new permission
    /// </summary>
    public class CreatePermissionDto
    {
        
        [StringLength(50, ErrorMessage = "Permission ID cannot exceed 50 characters")]
        public string PermissionId { get; set; } = string.Empty;

        
        [StringLength(100, ErrorMessage = "Permission name cannot exceed 100 characters")]
        public string PermissionName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Action ID is required")]
        [StringLength(50, ErrorMessage = "Action ID cannot exceed 50 characters")]
        public string ActionId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Entity ID is required")]
        [StringLength(50, ErrorMessage = "Entity ID cannot exceed 50 characters")]
        public string EntityId { get; set; } = string.Empty;

        [StringLength(50, ErrorMessage = "Time Active ID cannot exceed 50 characters")]
        public string? TimeActiveId { get; set; }

        [Required(ErrorMessage = "Role ID is required")]
        [StringLength(50, ErrorMessage = "Role ID cannot exceed 50 characters")]
        public string RoleId { get; set; }

        public bool IsActive { get; set; } = true;
    }

    /// <summary>
    /// DTO for updating permission information
    /// </summary>
    public class UpdatePermissionDto
    {
        [StringLength(100, ErrorMessage = "Permission name cannot exceed 100 characters")]
        public string? PermissionName { get; set; }

        [StringLength(50, ErrorMessage = "Action ID cannot exceed 50 characters")]
        public string? ActionId { get; set; }

        [StringLength(50, ErrorMessage = "Entity ID cannot exceed 50 characters")]
        public string? EntityId { get; set; }

        [StringLength(50, ErrorMessage = "Time Active ID cannot exceed 50 characters")]
        public string? TimeActiveId { get; set; }

        [StringLength(50, ErrorMessage = "Role ID cannot exceed 50 characters")]
        public string? RoleId { get; set; }

        public bool? IsActive { get; set; }
    }

    /// <summary>
    /// DTO for paginated permission response
    /// </summary>
    public class PermissionsApiResponse
    {
        public List<PermissionInfoDto> Permissions { get; set; } = new();
        public PermissionsPaginationInfo Pagination { get; set; } = new();
    }

    /// <summary>
    /// Pagination information for permissions
    /// </summary>
    public class PermissionsPaginationInfo
    {
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
        public bool HasNextPage { get; set; }
        public bool HasPreviousPage { get; set; }
    }

    /// <summary>
    /// API response wrapper for permissions
    /// </summary>
    public class ApiResponse<T>
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
    }
}
