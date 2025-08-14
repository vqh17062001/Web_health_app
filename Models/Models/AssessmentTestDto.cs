using System.ComponentModel.DataAnnotations;

namespace Web_health_app.Models.Models
{
    /// <summary>
    /// DTO for assessment test information display
    /// </summary>
    public class AssessmentTestInfoDto
    {
        public string? TestTypeId { get; set; }

        public string? AbsId { get; set; }

        public string? Code { get; set; }

        public string? Unit { get; set; }

        public string? ResultValue { get; set; }

        public DateTime? RecordedAt { get; set; }

        public Guid? RecordedBy { get; set; }

        public string? RecordedByName { get; set; }

        // Additional info from related entities
        public string? TestTypeName { get; set; }

        public string? StudentId { get; set; }

        public string? StudentName { get; set; }

        public string? AssessmentBatchId { get; set; }
    }

    /// <summary>
    /// DTO for creating new assessment test result
    /// </summary>
    public class CreateAssessmentTestDto
    {
        [Required(ErrorMessage = "Test type ID is required")]
        public string TestTypeId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Assessment batch student ID is required")]
        public string AbsId { get; set; } = string.Empty;

        public string? Code { get; set; }

        [Required(ErrorMessage = "Unit is required")]
        public string Unit { get; set; }

        [Required(ErrorMessage = "Result value is required")]
        public string ResultValue { get; set; } = string.Empty;

        public DateTime? RecordedAt { get; set; } = DateTime.UtcNow;

        [Required(ErrorMessage = "Recorded by is required")]
        public Guid RecordedBy { get; set; }
    }

    /// <summary>
    /// DTO for updating assessment test result
    /// </summary>
    public class UpdateAssessmentTestDto
    {
        public string? Code { get; set; }

        public string? Unit { get; set; }

        public string? ResultValue { get; set; }

        public DateTime? RecordedAt { get; set; }

        public Guid? RecordedBy { get; set; }
    }

    /// <summary>
    /// DTO for assessment test search and filter
    /// </summary>
    public class AssessmentTestSearchDto
    {
        public string? TestTypeId { get; set; }

        public string? AbsId { get; set; }

        public string? AssessmentBatchId { get; set; }

        public string? StudentId { get; set; }

        public string? SearchTerm { get; set; }

        public DateTime? RecordedFrom { get; set; }

        public DateTime? RecordedTo { get; set; }

        public Guid? RecordedBy { get; set; }

        public string? SortBy { get; set; } = "RecordedAt";

        public string? SortDirection { get; set; } = "desc";

        public int Page { get; set; } = 1;

        public int PageSize { get; set; } = 20;
    }

    /// <summary>
    /// DTO for paginated assessment tests response
    /// </summary>
    public class AssessmentTestsApiResponse
    {
        public List<AssessmentTestInfoDto> AssessmentTests { get; set; } = new List<AssessmentTestInfoDto>();
        public AssessmentTestsPaginationInfo Pagination { get; set; } = new AssessmentTestsPaginationInfo();
    }

    /// <summary>
    /// Pagination information for assessment tests
    /// </summary>
    public class AssessmentTestsPaginationInfo
    {
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
        public bool HasNextPage { get; set; }
        public bool HasPreviousPage { get; set; }
    }

    /// <summary>
    /// DTO for bulk assessment test operations
    /// </summary>
    public class BulkAssessmentTestOperationDto
    {
        [Required(ErrorMessage = "Assessment test IDs are required")]
        public List<string> TestIds { get; set; } = new List<string>();

        public string? Operation { get; set; } // "delete", "update_recorder", etc.

        public string? Reason { get; set; }

        public Guid? NewRecordedBy { get; set; }
    }
}
