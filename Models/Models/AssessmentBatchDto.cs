using System.ComponentModel.DataAnnotations;

namespace Web_health_app.Models.Models
{
    /// <summary>
    /// DTO for assessment batch information display
    /// </summary>
    public class AssessmentBatchInfoDto
    {
        public string AssessmentBatchId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Assessment batch name is required")]
        [StringLength(100, ErrorMessage = "Assessment batch name cannot exceed 100 characters")]
        public string CodeName { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string? Description { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public short Status { get; set; }

        public string StatusString { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public Guid CreatedBy { get; set; }

        public string? CreatedByName { get; set; }

        public int StudentCount { get; set; } = 0;

        public int CompletedCount { get; set; } = 0;

        public int PendingCount { get; set; } = 0;
    }

    /// <summary>
    /// DTO for creating new assessment batch
    /// </summary>
    public class CreateAssessmentBatchDto
    {
        [Required(ErrorMessage = "Assessment batch name is required")]
        [StringLength(100, ErrorMessage = "Assessment batch name cannot exceed 100 characters")]
        public string CodeName { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Start date is required")]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "End date is required")]
        public DateTime EndDate { get; set; }

        [Required(ErrorMessage = "Status is required")]
        public short Status { get; set; } = 1;

        public Guid CreatedBy { get; set; }
    }

    /// <summary>
    /// DTO for updating assessment batch information
    /// </summary>
    public class UpdateAssessmentBatchDto
    {
        [StringLength(100, ErrorMessage = "Assessment batch name cannot exceed 100 characters")]
        public string? CodeName { get; set; }

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string? Description { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public short? Status { get; set; }
    }

    /// <summary>
    /// DTO for assessment batch with detailed information
    /// </summary>
    public class AssessmentBatchDetailDto : AssessmentBatchInfoDto
    {
        public List<AssessmentBatchStudentDto> Students { get; set; } = new List<AssessmentBatchStudentDto>();
        public List<AssessmentTestDto> Tests { get; set; } = new List<AssessmentTestDto>();
    }

    /// <summary>
    /// DTO for paginated assessment batches response
    /// </summary>
    public class AssessmentBatchesApiResponse
    {
        public List<AssessmentBatchInfoDto> AssessmentBatches { get; set; } = new List<AssessmentBatchInfoDto>();
        public AssessmentBatchesPaginationInfo Pagination { get; set; } = new AssessmentBatchesPaginationInfo();
    }

    /// <summary>
    /// Pagination information for assessment batches
    /// </summary>
    public class AssessmentBatchesPaginationInfo
    {
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
        public bool HasNextPage { get; set; }
        public bool HasPreviousPage { get; set; }
    }

    /// <summary>
    /// DTO for assessment batch search and filter
    /// </summary>
    public class AssessmentBatchSearchDto
    {
        public string? SearchTerm { get; set; }
        public short? Status { get; set; }
      
        public Guid? ManagerBy { get; set; }
        public Guid? CreatedBy { get; set; }
        public DateTime? StartDateFrom { get; set; }
        public DateTime? StartDateTo { get; set; }
        public DateTime? EndDateFrom { get; set; }
        public DateTime? EndDateTo { get; set; }
        public DateTime? CreatedFrom { get; set; }
        public DateTime? CreatedTo { get; set; }
        public string? SortBy { get; set; } = "AssessmentBatchId";
        public string? SortDirection { get; set; } = "asc";
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    /// <summary>
    /// DTO for bulk assessment batch operations
    /// </summary>
    public class BulkAssessmentBatchOperationDto
    {
        [Required(ErrorMessage = "Assessment batch IDs are required")]
        public List<string> AssessmentBatchIds { get; set; } = new List<string>();

        public string? Operation { get; set; } // "activate", "deactivate", "delete", "close"
        public string? Reason { get; set; }
    }

    /// <summary>
    /// DTO for assessment batch statistics
    /// </summary>
    public class AssessmentBatchStatisticsDto
    {
        public int TotalBatches { get; set; }
        public int ActiveBatches { get; set; }
        public int CompletedBatches { get; set; }
        public int PendingBatches { get; set; }
        public int TotalStudentsInBatches { get; set; }
        public int TotalCompletedAssessments { get; set; }
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    }

    // Placeholder DTOs for related entities
    public class AssessmentTestDto
    {
        public string AssessmentTestId { get; set; } = string.Empty;
        public string AssessmentBatchId { get; set; } = string.Empty;
        public string TestName { get; set; } = string.Empty;
        public DateTime TestDate { get; set; }
        // Add other properties as needed
    }
}
