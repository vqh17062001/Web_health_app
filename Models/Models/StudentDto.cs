using System.ComponentModel.DataAnnotations;

namespace Web_health_app.Models.Models
{
    /// <summary>
    /// DTO for student information display
    /// </summary>
    public class StudentInfoDto
    {
        public string StudentId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Student name is required")]
        [StringLength(100, ErrorMessage = "Student name cannot exceed 100 characters")]
        public string Name { get; set; } = string.Empty;

        [StringLength(10, ErrorMessage = "Date of birth cannot exceed 10 characters")]
        public string? Dob { get; set; }

        [StringLength(10, ErrorMessage = "Gender cannot exceed 10 characters")]
        public string? Gender { get; set; }

        [Phone(ErrorMessage = "Invalid phone number format")]
        [StringLength(15, ErrorMessage = "Phone number cannot exceed 15 characters")]
        public string? Phone { get; set; }

        [EmailAddress(ErrorMessage = "Invalid email format")]
        [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters")]
        public string? Email { get; set; }

        public short Status { get; set; }

        public string StatusString { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdateAt { get; set; }

        public Guid? CreatedBy { get; set; }

        public string? CreatedByName { get; set; }

        public Guid? ManageBy { get; set; }

        public string? ManageByName { get; set; }

        [StringLength(100, ErrorMessage = "Department cannot exceed 100 characters")]
        public string? Department { get; set; }

        // Health metrics counts
        public int BodyMetricsCount { get; set; } = 0;
        public int PhysiologicalMetricsCount { get; set; } = 0;
        public int DailyActivitiesCount { get; set; } = 0;
        public int SleepSessionsCount { get; set; } = 0;
        public int ExercisesCount { get; set; } = 0;
        public int AssessmentBatchCount { get; set; } = 0;
      

    }

    /// <summary>
    /// DTO for creating new student
    /// </summary>
    public class CreateStudentDto
    {
        [Required(ErrorMessage = "Student name is required")]
        [StringLength(100, ErrorMessage = "Student name cannot exceed 100 characters")]
        public string Name { get; set; } = string.Empty;

        [StringLength(10, ErrorMessage = "Date of birth cannot exceed 10 characters")]
        public string? Dob { get; set; }

        [StringLength(10, ErrorMessage = "Gender cannot exceed 10 characters")]
        public string? Gender { get; set; }

        [Phone(ErrorMessage = "Invalid phone number format")]
        [StringLength(15, ErrorMessage = "Phone number cannot exceed 15 characters")]
        public string? Phone { get; set; }

        [EmailAddress(ErrorMessage = "Invalid email format")]
        [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters")]
        public string? Email { get; set; }
        [Required(ErrorMessage = "Status is required")]
        public short Status { get; set; } = 1;

        public Guid CreatedBy { get; set; }

        public Guid? ManageBy { get; set; }

        [StringLength(100, ErrorMessage = "Department cannot exceed 100 characters")]
        public string? Department { get; set; }
    }

    /// <summary>
    /// DTO for updating student information
    /// </summary>
    public class UpdateStudentDto
    {
        [StringLength(100, ErrorMessage = "Student name cannot exceed 100 characters")]
        public string? Name { get; set; }

        [StringLength(10, ErrorMessage = "Date of birth cannot exceed 10 characters")]
        public string? Dob { get; set; }

        [StringLength(10, ErrorMessage = "Gender cannot exceed 10 characters")]
        public string? Gender { get; set; }

        [Phone(ErrorMessage = "Invalid phone number format")]
        [StringLength(15, ErrorMessage = "Phone number cannot exceed 15 characters")]
        public string? Phone { get; set; }

        [EmailAddress(ErrorMessage = "Invalid email format")]
        [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters")]
        public string? Email { get; set; }

        public short? Status { get; set; }

        public Guid? ManageBy { get; set; }

        [StringLength(100, ErrorMessage = "Department cannot exceed 100 characters")]
        public string? Department { get; set; }
    }

    /// <summary>
    /// DTO for student with health metrics details
    /// </summary>
    public class StudentDetailDto : StudentInfoDto
    {
        public List<BodyMetricDto> BodyMetrics { get; set; } = new List<BodyMetricDto>();
        public List<PhysiologicalMetricDto> PhysiologicalMetrics { get; set; } = new List<PhysiologicalMetricDto>();
        public List<DailyActivityDto> DailyActivities { get; set; } = new List<DailyActivityDto>();
        public List<SleepSessionDto> SleepSessions { get; set; } = new List<SleepSessionDto>();
        public List<ExerciseDto> Exercises { get; set; } = new List<ExerciseDto>();
        public List<AssessmentBatchStudentDto> AssessmentBatches { get; set; } = new List<AssessmentBatchStudentDto>();
    }

    /// <summary>
    /// DTO for paginated students response
    /// </summary>
    public class StudentsApiResponse
    {
        public List<StudentInfoDto> Students { get; set; } = new List<StudentInfoDto>();
        public StudentsPaginationInfo Pagination { get; set; } = new StudentsPaginationInfo();
    }

    /// <summary>
    /// Pagination information for students
    /// </summary>
    public class StudentsPaginationInfo
    {
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
        public bool HasNextPage { get; set; }
        public bool HasPreviousPage { get; set; }
    }

    /// <summary>
    /// DTO for student search and filter
    /// </summary>
    public class StudentSearchDto
    {
        public string? SearchTerm { get; set; }
        public string? Department { get; set; }
        public string? Gender { get; set; }
        public short? Status { get; set; }
        public Guid? ManageBy { get; set; }
        public string? DobFrom { get; set; }
        public string? DobTo { get; set; }


        public DateTime? CreatedFrom { get; set; }
        public DateTime? CreatedTo { get; set; }
        public string? SortBy { get; set; } = "StudentId";
        public string? SortDirection { get; set; } = "asc";
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    /// <summary>
    /// DTO for bulk student operations
    /// </summary>
    public class BulkStudentOperationDto
    {
        [Required(ErrorMessage = "Student IDs are required")]
        public List<string> StudentIds { get; set; } = new List<string>();

        public string? Operation { get; set; } // "activate", "deactivate", "delete", "update_department"
        public string? Reason { get; set; }
        public string? NewDepartment { get; set; }
        public Guid? NewManageBy { get; set; }
    }

    /// <summary>
    /// DTO for student with manager information
    /// </summary>
    public class StudentWithManagerDto : StudentInfoDto
    {
        public UserInfoDto? Manager { get; set; }
        public UserInfoDto? CreatedByUser { get; set; }
    }

    /// <summary>
    /// DTO for student health summary
    /// </summary>
    public class StudentHealthSummaryDto
    {
        public string StudentId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public DateTime? LastHealthCheck { get; set; }
        public DateTime? LastActivity { get; set; }
        public DateTime? LastSleepRecord { get; set; }
        public int TotalHealthRecords { get; set; }
        public string HealthStatus { get; set; } = string.Empty;
        public List<string> RecentActivities { get; set; } = new List<string>();
    }

    /// <summary>
    /// DTO for assigning students to assessment batch
    /// </summary>
    public class StudentAssessmentAssignmentDto
    {
        [Required(ErrorMessage = "Student IDs are required")]
        public List<string> StudentIds { get; set; } = new List<string>();

        [Required(ErrorMessage = "Assessment Batch ID is required")]
        public string AssessmentBatchId { get; set; } = string.Empty;

        public string? Note { get; set; }
    }

    // Placeholder DTOs for related entities (to be implemented separately)
    public class BodyMetricDto
    {
        public string Id { get; set; } = string.Empty;
        public string StudentId { get; set; } = string.Empty;
        public DateTime RecordDate { get; set; }
        // Add other properties as needed
    }

    public class PhysiologicalMetricDto
    {
        public string Id { get; set; } = string.Empty;
        public string StudentId { get; set; } = string.Empty;
        public DateTime RecordDate { get; set; }
        // Add other properties as needed
    }

    public class DailyActivityDto
    {
        public string Id { get; set; } = string.Empty;
        public string StudentId { get; set; } = string.Empty;
        public DateTime ActivityDate { get; set; }
        // Add other properties as needed
    }

    public class SleepSessionDto
    {
        public string Id { get; set; } = string.Empty;
        public string StudentId { get; set; } = string.Empty;
        public DateTime SleepDate { get; set; }
        // Add other properties as needed
    }

    public class ExerciseDto
    {
        public string Id { get; set; } = string.Empty;
        public string StudentId { get; set; } = string.Empty;
        public DateTime ExerciseDate { get; set; }
        // Add other properties as needed
    }

    public class AssessmentBatchStudentDto
    {
        public string AssessmentBatchId { get; set; } = string.Empty;
        public string StudentId { get; set; } = string.Empty;
        public DateTime AssignedDate { get; set; }
        // Add other properties as needed
    }

    /// <summary>
    /// DTO for student statistics
    /// </summary>
    public class StudentStatisticsDto
    {
        public int TotalStudents { get; set; }
        public int ActiveStudents { get; set; }
        public int InactiveStudents { get; set; }
        public int StudentsWithSyncData { get; set; }
        public int StudentsOffline { get; set; }
        public int StudentsOnline { get; set; }
        public Dictionary<string, int> StudentsByDepartment { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, int> StudentsByGender { get; set; } = new Dictionary<string, int>();
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    }
}
