using System.ComponentModel.DataAnnotations;

namespace Web_health_app.Models.Models
{
    /// <summary>
    /// Data Transfer Object for User Information
    /// </summary>
    public class UserInfoDto
    {
        public Guid UserId { get; set; }

        [Required(ErrorMessage = "Username is required")]
        [StringLength(50, ErrorMessage = "Username cannot exceed 50 characters")]
        public string UserName { get; set; } = string.Empty;

        [StringLength(100, ErrorMessage = "Full name cannot exceed 100 characters")]
        public string? FullName { get; set; }

        [Phone(ErrorMessage = "Invalid phone number format")]
        [StringLength(15, ErrorMessage = "Phone number cannot exceed 15 characters")]
        public string? PhoneNumber { get; set; }

        [StringLength(100, ErrorMessage = "Department cannot exceed 100 characters")]
        public string? Department { get; set; }

        public short UserStatus { get; set; }

        public string UserStatusString { get; set; } = string.Empty;

        public Guid? ManageBy { get; set; }

        public string? ManagerName { get; set; }

        public byte LevelSecurity { get; set; }

        public DateTime CreateAt { get; set; }

        public DateTime? UpdateAt { get; set; }

        public string? GroupId { get; set; }

        public string? GroupName { get; set; }
    }

    /// <summary>
    /// DTO for creating new user
    /// </summary>
    public class CreateUserDto
    {
        [Required(ErrorMessage = "Username is required")]
        [StringLength(50, ErrorMessage = "Username cannot exceed 50 characters")]
        public string UserName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be between 6 and 100 characters")]
        public string Password { get; set; } = string.Empty;

        [StringLength(100, ErrorMessage = "Full name cannot exceed 100 characters")]
        public string? FullName { get; set; }

        [Phone(ErrorMessage = "Invalid phone number format")]
        [StringLength(15, ErrorMessage = "Phone number cannot exceed 15 characters")]
        public string? PhoneNumber { get; set; }

        [StringLength(100, ErrorMessage = "Department cannot exceed 100 characters")]
        public string? Department { get; set; }

        public short UserStatus { get; set; }  // Default to Active

        public Guid? ManageBy { get; set; }

        public byte LevelSecurity { get; set; } = 1; // Default security level

        public string? GroupId { get; set; }
    }

    /// <summary>
    /// DTO for updating user information
    /// </summary>
    public class UpdateUserDto
    {
        [StringLength(100, ErrorMessage = "Full name cannot exceed 100 characters")]
        public string? FullName { get; set; }

        [Phone(ErrorMessage = "Invalid phone number format")]
        [StringLength(15, ErrorMessage = "Phone number cannot exceed 15 characters")]
        public string? PhoneNumber { get; set; }

        [StringLength(100, ErrorMessage = "Department cannot exceed 100 characters")]
        public string? Department { get; set; }

        public short? UserStatus { get; set; }

        public Guid? ManageBy { get; set; }

        public byte? LevelSecurity { get; set; }

        public string? GroupId { get; set; }
    }
    public class UserSearchDto
    {
        /// <summary>
        /// General search term for username, full name, or department
        /// </summary>
        public string? SearchTerm { get; set; }

        /// <summary>
        /// Filter by user status
        /// </summary>
        public short? UserStatus { get; set; }

        /// <summary>
        /// Filter by department
        /// </summary>
        public string? Department { get; set; }

        /// <summary>
        /// Filter by group ID
        /// </summary>
        public string? GroupId { get; set; }

        /// <summary>
        /// Filter by minimum security level
        /// </summary>
        public byte? MinLevelSecurity { get; set; }

        /// <summary>
        /// Filter by maximum security level
        /// </summary>
        public byte? MaxLevelSecurity { get; set; }

        /// <summary>
        /// Filter by manager ID
        /// </summary>
        public Guid? ManageBy { get; set; }

        /// <summary>
        /// Filter by creation date from
        /// </summary>
        public string? CreatedFrom { get; set; }

        /// <summary>
        /// Filter by creation date to
        /// </summary>
        public string? CreatedTo { get; set; }

        /// <summary>
        /// Filter by update date from
        /// </summary>
        public string? UpdatedFrom { get; set; }

        /// <summary>
        /// Filter by update date to
        /// </summary>
        public string? UpdatedTo { get; set; }

        /// <summary>
        /// Filter by users who have phone number
        /// </summary>
        public bool? HasPhoneNumber { get; set; }

        /// <summary>
        /// Filter by users who have manager
        /// </summary>
        public bool? HasManager { get; set; }

        /// <summary>
        /// Sort field
        /// </summary>
        public string SortBy { get; set; } = "CreateAt";

        /// <summary>
        /// Sort direction (asc/desc)
        /// </summary>
        public string SortDirection { get; set; } = "desc";
    }
}
