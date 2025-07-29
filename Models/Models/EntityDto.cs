using System.ComponentModel.DataAnnotations;

namespace Web_health_app.Models.Models
{

    /// <summary>
    /// Data Transfer Object for Entity Information
    /// </summary>
    public class EntityInfoDto
    {
        [Required(ErrorMessage = "Entity ID is required")]
        [StringLength(50, ErrorMessage = "Entity ID cannot exceed 50 characters")]
        public string EntityId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Entity name is required")]
        [StringLength(100, ErrorMessage = "Entity name cannot exceed 100 characters")]
        public string NameEntity { get; set; } = string.Empty;

        [Range(1, 5, ErrorMessage = "Level security must be between 1 and 5")]
        public byte LevelSecurity { get; set; }

        [StringLength(50, ErrorMessage = "Type cannot exceed 50 characters")]
        public string? Type { get; set; }
    }

    /// <summary>
    /// DTO for updating entity information
    /// </summary>
    public class UpdateEntityDto
    {


        [Range(1, 5, ErrorMessage = "Level security must be between 1 and 5")]
        public byte? LevelSecurity { get; set; }


    }

    /// <summary>
    /// API Response for Entities with pagination
    /// </summary>
    public class EntitiesApiResponse
    {
        public List<EntityInfoDto> Entities { get; set; } = new();
        public EntitiesPaginationInfo Pagination { get; set; } = new();
    }

    /// <summary>
    /// Pagination information for Entities
    /// </summary>
    public class EntitiesPaginationInfo
    {
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
        public bool HasNextPage { get; set; }
        public bool HasPreviousPage { get; set; }
    }
}
