using System.ComponentModel.DataAnnotations;

namespace Web_health_app.Models.Models
{
    /// <summary>
    /// DTO for assigning roles to user
    /// </summary>
    public class UserRoleAssignmentDto
    {
        [Required(ErrorMessage = "User ID is required")]
        public Guid UserId { get; set; }

        [Required(ErrorMessage = "Role IDs are required")]
        public List<string> RoleIds { get; set; } = new List<string>();
    }

    /// <summary>
    /// DTO for role assignment to user
    /// </summary>
    public class RoleUserAssignmentDto
    {
        [Required(ErrorMessage = "Role ID is required")]
        public string RoleId { get; set; } = string.Empty;

        [Required(ErrorMessage = "User IDs are required")]
        public List<Guid> UserIds { get; set; } = new List<Guid>();
    }

    /// <summary>
    /// DTO for user with roles information
    /// </summary>
    public class UserWithRolesDto : UserInfoDto
    {
        public List<RoleInfoDto> Roles { get; set; } = new List<RoleInfoDto>();
        public int RoleCount => Roles.Count;
    }

    /// <summary>
    /// DTO for role with users information
    /// </summary>
    public class RoleWithUsersDto : RoleInfoDto
    {
        public List<UserInfoDto> Users { get; set; } = new List<UserInfoDto>();
    }
}
