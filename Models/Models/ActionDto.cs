using System.ComponentModel.DataAnnotations;

namespace Web_health_app.Models.Models
{
    /// <summary>
    /// Data Transfer Object for Action Information
    /// </summary>
    public class ActionInfoDto
    {
        [Required(ErrorMessage = "Action ID is required")]
        [StringLength(50, ErrorMessage = "Action ID cannot exceed 50 characters")]
        public string ActionId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Action name is required")]
        [StringLength(100, ErrorMessage = "Action name cannot exceed 100 characters")]
        public string ActionName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Code is required")]
        [StringLength(50, ErrorMessage = "Code cannot exceed 50 characters")]
        public string Code { get; set; } = string.Empty;

        public bool IsActive { get; set; }
    }

    /// <summary>
    /// DTO for updating action information
    /// </summary>
    public class UpdateActionDto
    {
        public bool? IsActive { get; set; }
    }

    /// <summary>
    /// API Response for Actions with pagination
    /// </summary>
    public class ActionsApiResponse
    {
        public List<ActionInfoDto> Actions { get; set; } = new();
        public ActionsPaginationInfo Pagination { get; set; } = new();
    }

    /// <summary>
    /// Pagination information for Actions
    /// </summary>
    public class ActionsPaginationInfo
    {
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
        public bool HasNextPage { get; set; }
        public bool HasPreviousPage { get; set; }
    }
}
