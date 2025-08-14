using Web_health_app.Models.Models;

namespace Web_health_app.ApiService.Repository.Interface
{
    /// <summary>
    /// Repository interface for TestType read-only operations
    /// </summary>
    public interface ITestTypeRepository
    {
        /// <summary>
        /// Get all test types with pagination and optional search
        /// </summary>
        /// <param name="pageNumber">Page number</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="searchTerm">Search term for filtering</param>
        /// <returns>Tuple containing list of test types and total count</returns>
        Task<(List<TestTypeInfoDto> TestTypes, int TotalCount)> GetAllTestTypesAsync(
            int pageNumber = 1, int pageSize = 10, string? searchTerm = null);

        /// <summary>
        /// Search test types with advanced filters
        /// </summary>
        /// <param name="searchDto">Search criteria</param>
        /// <returns>Tuple containing filtered test types and total count</returns>
        Task<(List<TestTypeInfoDto> TestTypes, int TotalCount)> SearchTestTypesAsync(
            TestTypeSearchDto searchDto);

        /// <summary>
        /// Get test type by ID
        /// </summary>
        /// <param name="testTypeId">Test type ID</param>
        /// <returns>Test type information or null if not found</returns>
        Task<TestTypeInfoDto?> GetTestTypeByIdAsync(string testTypeId);

        /// <summary>
        /// Get all test types for dropdown/select options
        /// </summary>
        /// <returns>List of test type select options</returns>
        Task<List<TestTypeSelectDto>> GetTestTypeSelectOptionsAsync();

        /// <summary>
        /// Get test types by unit
        /// </summary>
        /// <param name="unit">Unit to filter by</param>
        /// <returns>List of test types with specified unit</returns>
        Task<List<TestTypeInfoDto>> GetTestTypesByUnitAsync(string unit);

        /// <summary>
        /// Get test types by code pattern
        /// </summary>
        /// <param name="codePattern">Code pattern to search for</param>
        /// <returns>List of test types matching code pattern</returns>
        Task<List<TestTypeInfoDto>> GetTestTypesByCodePatternAsync(string codePattern);

        /// <summary>
        /// Check if test type exists
        /// </summary>
        /// <param name="testTypeId">Test type ID to check</param>
        /// <returns>True if exists, false otherwise</returns>
        Task<bool> TestTypeExistsAsync(string testTypeId);

        /// <summary>
        /// Get total count of test types
        /// </summary>
        /// <returns>Total number of test types</returns>
        Task<int> GetTotalTestTypesCountAsync();

        /// <summary>
        /// Get unique units from all test types
        /// </summary>
        /// <returns>List of unique units</returns>
        Task<List<string>> GetUniqueUnitsAsync();
    }
}
