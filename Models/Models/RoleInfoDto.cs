using System.ComponentModel.DataAnnotations;

namespace Web_health_app.Models.Models
{
    /// <summary>
    /// Data Transfer Object for Role Information
    /// </summary>
    public class RoleInfoDto
    {
        [Required(ErrorMessage = "Role ID is required")]
        [StringLength(50, ErrorMessage = "Role ID cannot exceed 50 characters")]
        public string RoleId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Role name is required")]
        [StringLength(100, ErrorMessage = "Role name cannot exceed 100 characters")]
        public string RoleName { get; set; } = string.Empty;

        public bool IsActive { get; set; }

        public List<string> Permissions { get; set; } = new List<string>();

        public int PermissionCount => Permissions.Count;

        public int UserCount { get; set; }
    }

    /// <summary>
    /// DTO for creating new role
    /// </summary>
    public class CreateRoleDto
    {
        [Required(ErrorMessage = "Role name is required")]
        [StringLength(100, ErrorMessage = "Role name cannot exceed 100 characters")]
        public string RoleName { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;
    }

    /// <summary>
    /// DTO for updating role information
    /// </summary>
    public class UpdateRoleDto
    {
        [StringLength(100, ErrorMessage = "Role name cannot exceed 100 characters")]
        public string? RoleName { get; set; }

        public bool? IsActive { get; set; }

        public List<string>? PermissionIds { get; set; }
    }

    /// <summary>
    /// DTO for role assignment operations
    /// </summary>
    public class RoleAssignmentDto
    {
        [Required(ErrorMessage = "Role ID is required")]
        public string RoleId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Permission IDs are required")]
        public List<string> PermissionIds { get; set; } = new List<string>();
    }

    public class RoleSearchDto
    {
        /// <summary>
        /// General search term for role ID or role name
        /// </summary>
        public string? SearchTerm { get; set; }

        /// <summary>
        /// Filter by active status
        /// </summary>
        public bool? IsActive { get; set; }

        /// <summary>
        /// Filter by minimum user count
        /// </summary>
        public int? MinUserCount { get; set; }

        /// <summary>
        /// Filter by maximum user count
        /// </summary>
        public int? MaxUserCount { get; set; }

        /// <summary>
        /// Filter by minimum permission count
        /// </summary>
        public int? MinPermissionCount { get; set; }

        /// <summary>
        /// Filter by maximum permission count
        /// </summary>
        public int? MaxPermissionCount { get; set; }

        /// <summary>
        /// Filter by specific permissions (comma-separated)
        /// </summary>
        public string? HasPermissions { get; set; }

        /// <summary>
        /// Filter by roles with no users
        /// </summary>
        public bool? IsEmpty { get; set; }

        /// <summary>
        /// Sort field
        /// </summary>
        public string SortBy { get; set; } = "RoleId";

        /// <summary>
        /// Sort direction (asc/desc)
        /// </summary>
        public string SortDirection { get; set; } = "asc";
    }

    /// <summary>
    /// DTO for role with user count (inherits UserCount from RoleInfoDto)
    /// </summary>
    public class RoleWithUserCountDto : RoleInfoDto
    {
        // UserCount is now inherited from RoleInfoDto
    }
}
