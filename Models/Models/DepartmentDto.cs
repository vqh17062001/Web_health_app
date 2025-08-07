using System.ComponentModel.DataAnnotations;

namespace Web_health_app.Models.Models
{
    /// <summary>
    /// DTO for department information display (read-only)
    /// </summary>
    public class DepartmentInfoDto
    {
        [Required]
        public string DepartmentCode { get; set; } = string.Empty;

        public string? Battalion { get; set; }

        public string? Course { get; set; }

        public string? CharacterCode { get; set; }

        // Additional computed properties
        public string DisplayName => GetDisplayName();
        public string FullDescription => GetFullDescription();

        private string GetDisplayName()
        {
            if (!string.IsNullOrEmpty(Battalion) && !string.IsNullOrEmpty(Course))
            {
                return $"{DepartmentCode} - {Battalion} - {Course}";
            }
            else if (!string.IsNullOrEmpty(Battalion))
            {
                return $"{DepartmentCode} - {Battalion}";
            }
            else if (!string.IsNullOrEmpty(Course))
            {
                return $"{DepartmentCode} - {Course}";
            }
            return DepartmentCode;
        }

        private string GetFullDescription()
        {
            var parts = new List<string> { DepartmentCode };

            if (!string.IsNullOrEmpty(Battalion))
                parts.Add($"Tiểu đoàn: {Battalion}");

            if (!string.IsNullOrEmpty(Course))
                parts.Add($"Khóa: {Course}");

            if (!string.IsNullOrEmpty(CharacterCode))
                parts.Add($"Ký hiệu: {CharacterCode}");

            return string.Join(" | ", parts);
        }
    }

    /// <summary>
    /// DTO for department summary with student count
    /// </summary>
    public class DepartmentSummaryDto
    {
        public string DepartmentCode { get; set; } = string.Empty;
        public string? Battalion { get; set; }
        public string? Course { get; set; }
        public string? CharacterCode { get; set; }
        public int StudentCount { get; set; }
        public string DisplayName => GetDisplayName();
        public string FullDescription => GetFullDescription();

        private string GetDisplayName()
        {
            if (!string.IsNullOrEmpty(Battalion) && !string.IsNullOrEmpty(Course))
            {
                return $"{DepartmentCode} - {Battalion} - {Course}";
            }
            else if (!string.IsNullOrEmpty(Battalion))
            {
                return $"{DepartmentCode} - {Battalion}";
            }
            else if (!string.IsNullOrEmpty(Course))
            {
                return $"{DepartmentCode} - {Course}";
            }
            return DepartmentCode;
        }

        private string GetFullDescription()
        {
            var parts = new List<string> { DepartmentCode };

            if (!string.IsNullOrEmpty(Battalion))
                parts.Add($"Tiểu đoàn: {Battalion}");

            if (!string.IsNullOrEmpty(Course))
                parts.Add($"Khóa: {Course}");

            if (!string.IsNullOrEmpty(CharacterCode))
                parts.Add($"Ký hiệu: {CharacterCode}");

            return string.Join(" | ", parts);
        }
    }

    /// <summary>
    /// DTO for departments API response with pagination
    /// </summary>
    public class DepartmentsApiResponse
    {
        public List<DepartmentInfoDto> Departments { get; set; } = new List<DepartmentInfoDto>();
        public DepartmentsPaginationInfo Pagination { get; set; } = new DepartmentsPaginationInfo();
    }

    /// <summary>
    /// Pagination information for departments
    /// </summary>
    public class DepartmentsPaginationInfo
    {
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
        public bool HasNextPage { get; set; }
        public bool HasPreviousPage { get; set; }
    }

    /// <summary>
    /// DTO for department statistics
    /// </summary>
    public class DepartmentStatisticsDto
    {
        public int TotalDepartments { get; set; }
        public int TotalStudents { get; set; }
        public int DepartmentsWithStudents { get; set; }
        public int DepartmentsWithoutStudents { get; set; }
        public Dictionary<string, int> StudentsByDepartment { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, int> StudentsByBattalion { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, int> StudentsByCourse { get; set; } = new Dictionary<string, int>();
    }

    /// <summary>
    /// DTO for department search and filter
    /// </summary>
    public class DepartmentSearchDto
    {
        public string? SearchTerm { get; set; }
        public string? Battalion { get; set; }
        public string? Course { get; set; }
        public string? CharacterCode { get; set; }
        public string? SortBy { get; set; } = "DepartmentCode";
        public string? SortDirection { get; set; } = "asc";
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}
