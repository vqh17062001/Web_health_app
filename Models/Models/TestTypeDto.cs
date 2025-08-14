using System.ComponentModel.DataAnnotations;

namespace Web_health_app.Models.Models
{
    /// <summary>
    /// DTO for test type information display (read-only)
    /// </summary>
    public class TestTypeInfoDto
    {
        public string? TestTypeId { get; set; }

        public string? Code { get; set; }

        public string? Name { get; set; }

        public string? Description { get; set; }

        public string? Unit { get; set; }
    }

    /// <summary>
    /// DTO for test type search and filter
    /// </summary>
    public class TestTypeSearchDto
    {
        public string? TestTypeId { get; set; }

        public string? Code { get; set; }

        public string? Name { get; set; }

        public string? SearchTerm { get; set; }

        public string? Unit { get; set; }

        public string? SortBy { get; set; } = "Name";

        public string? SortDirection { get; set; } = "asc";

        public int Page { get; set; } = 1;

        public int PageSize { get; set; } = 20;
    }

    /// <summary>
    /// DTO for paginated test types response
    /// </summary>
    public class TestTypesApiResponse
    {
        public List<TestTypeInfoDto> TestTypes { get; set; } = new List<TestTypeInfoDto>();
        public TestTypesPaginationInfo Pagination { get; set; } = new TestTypesPaginationInfo();
    }

    /// <summary>
    /// Pagination information for test types
    /// </summary>
    public class TestTypesPaginationInfo
    {
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
        public bool HasNextPage { get; set; }
        public bool HasPreviousPage { get; set; }
    }

    /// <summary>
    /// DTO for test type dropdown/select options
    /// </summary>
    public class TestTypeSelectDto
    {
        public string? TestTypeId { get; set; }
        public string? Name { get; set; }
        public string? Code { get; set; }
        public string? Unit { get; set; }
    }
}
