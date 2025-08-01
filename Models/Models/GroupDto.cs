using System.ComponentModel.DataAnnotations;

namespace Web_health_app.Models.Models
{
    /// <summary>
    /// DTO for group information display
    /// </summary>
    public class GroupInfoDto
    {
        public string GroupId { get; set; } = string.Empty;
        public string GroupName { get; set; } = string.Empty;
        public string? TimeActiveId { get; set; }
        public bool IsActive { get; set; }
        public int UserCount { get; set; } = 0;
        public int RoleCount { get; set; } = 0;
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    /// <summary>
    /// DTO for creating new group
    /// </summary>
    public class CreateGroupDto
    {
     

        [Required(ErrorMessage = "Group name is required")]
        [StringLength(200, ErrorMessage = "Group name cannot exceed 200 characters")]
        public string GroupName { get; set; } = string.Empty;

        [StringLength(50, ErrorMessage = "Time Active ID cannot exceed 50 characters")]
        public string? TimeActiveId { get; set; }

        public bool IsActive { get; set; } = true;
    }

    /// <summary>
    /// DTO for updating group information
    /// </summary>
    public class UpdateGroupDto
    {
       
        [StringLength(200, ErrorMessage = "Group name cannot exceed 200 characters")]
        public string GroupName { get; set; } = string.Empty;

        [StringLength(50, ErrorMessage = "Time Active ID cannot exceed 50 characters")]
        public string? TimeActiveId { get; set; }

        public bool IsActive { get; set; } = true;
    }

    /// <summary>
    /// DTO for group with users information
    /// </summary>
    public class GroupWithUsersDto : GroupInfoDto
    {
        public List<UserInfoDto> Users { get; set; } = new List<UserInfoDto>();
    }

    /// <summary>
    /// DTO for group with roles information
    /// </summary>
    public class GroupWithRolesDto : GroupInfoDto
    {
        public List<RoleInfoDto> Roles { get; set; } = new List<RoleInfoDto>();
    }

    /// <summary>
    /// DTO for group with full details (users and roles)
    /// </summary>
    public class GroupDetailDto : GroupInfoDto
    {
        public List<UserInfoDto> Users { get; set; } = new List<UserInfoDto>();
        public List<RoleInfoDto> Roles { get; set; } = new List<RoleInfoDto>();
        public string? TimeActiveName { get; set; }
    }

    /// <summary>
    /// DTO for assigning users to group
    /// </summary>
    public class GroupUserAssignmentDto
    {
        [Required(ErrorMessage = "Group ID is required")]
        public string GroupId { get; set; } = string.Empty;

        [Required(ErrorMessage = "User IDs are required")]
        public List<Guid> UserIds { get; set; } = new List<Guid>();
    }

    /// <summary>
    /// DTO for assigning roles to group
    /// </summary>
    public class GroupRoleAssignmentDto
    {
        [Required(ErrorMessage = "Group ID is required")]
        public string GroupId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Role IDs are required")]
        public List<string> RoleIds { get; set; } = new List<string>();

        public string? Note { get; set; }
    }

    /// <summary>
    /// DTO for user with groups information
    /// </summary>
    public class UserWithGroupsDto : UserInfoDto
    {
        public List<GroupInfoDto> Groups { get; set; } = new List<GroupInfoDto>();
        public int GroupCount => Groups.Count;
    }

    /// <summary>
    /// DTO for paginated groups response
    /// </summary>
    public class GroupsApiResponse
    {
        public List<GroupInfoDto> Groups { get; set; } = new List<GroupInfoDto>();
        public GroupsPaginationInfo Pagination { get; set; } = new GroupsPaginationInfo();
    }

    /// <summary>
    /// DTO for groups with user count response
    /// </summary>
    public class GroupsWithUserCountApiResponse
    {
        public List<GroupWithUserCountDto> Groups { get; set; } = new List<GroupWithUserCountDto>();
        public GroupsPaginationInfo Pagination { get; set; } = new GroupsPaginationInfo();
    }

    /// <summary>
    /// Pagination information for groups
    /// </summary>
    public class GroupsPaginationInfo
    {
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
        public bool HasNextPage { get; set; }
        public bool HasPreviousPage { get; set; }
    }

    /// <summary>
    /// DTO for group with user count information
    /// </summary>
    public class GroupWithUserCountDto : GroupInfoDto
    {
        public new int UserCount { get; set; } = 0;
        public List<string> UserNames { get; set; } = new List<string>();
        public DateTime? LastActivity { get; set; }
    }

    /// <summary>
    /// DTO for group role assignment with note
    /// </summary>
    public class GroupRoleDto
    {
        public string GroupId { get; set; } = string.Empty;
        public string RoleId { get; set; } = string.Empty;
        public string? Note { get; set; }
        public GroupInfoDto? Group { get; set; }
        public RoleInfoDto? Role { get; set; }
        public DateTime? AssignedAt { get; set; }
    }

    /// <summary>
    /// DTO for bulk group operations
    /// </summary>
    public class BulkGroupOperationDto
    {
        [Required(ErrorMessage = "Group IDs are required")]
        public List<string> GroupIds { get; set; } = new List<string>();

        public string? Operation { get; set; } // "activate", "deactivate", "delete"
        public string? Reason { get; set; }
    }

    /// <summary>
    /// DTO for group search and filter
    /// </summary>
    public class GroupSearchDto
    {
        public string? SearchTerm { get; set; }
        public bool? IsActive { get; set; }
        public string? TimeActiveId { get; set; }
        public int? MinUserCount { get; set; }
        public int? MaxUserCount { get; set; }
        public DateTime? CreatedFrom { get; set; }
        public DateTime? CreatedTo { get; set; }
        public string? SortBy { get; set; } = "GroupId";
        public string? SortDirection { get; set; } = "asc";
    }
}
