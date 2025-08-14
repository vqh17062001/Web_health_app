using Web_health_app.Models.Models;

namespace Web_health_app.ApiService.Repository.Interface
{
    public interface IAssessmentTestRepository
    {
        /// <summary>
        /// Get all assessment tests with pagination and search
        /// </summary>
        Task<(List<AssessmentTestInfoDto> AssessmentTests, int TotalCount)> GetAllAssessmentTestsAsync(
            int pageNumber = 1,
            int pageSize = 10,
            string? searchTerm = null);

        /// <summary>
        /// Search assessment tests with advanced filtering
        /// </summary>
        Task<(List<AssessmentTestInfoDto> AssessmentTests, int TotalCount)> SearchAssessmentTestsAsync(
            AssessmentTestSearchDto searchDto);

        /// <summary>
        /// Get assessment test by composite key (TestTypeId + AbsId)
        /// </summary>
        Task<AssessmentTestInfoDto?> GetAssessmentTestByIdAsync(string testTypeId, string absId);

        /// <summary>
        /// Get assessment tests by assessment batch student ID
        /// </summary>
        Task<List<AssessmentTestInfoDto>> GetAssessmentTestsByAbsIdAsync(string absId);

        /// <summary>
        /// Get assessment tests by assessment batch ID
        /// </summary>
        Task<(List<AssessmentTestInfoDto> AssessmentTests, int TotalCount)> GetAssessmentTestsByBatchIdAsync(
            string batchId,
            int pageNumber = 1,
            int pageSize = 10);

        /// <summary>
        /// Create a new assessment test result
        /// </summary>
        Task<AssessmentTestInfoDto?> CreateAssessmentTestAsync(CreateAssessmentTestDto createDto);

        /// <summary>
        /// Update an existing assessment test result
        /// </summary>
        Task<AssessmentTestInfoDto?> UpdateAssessmentTestAsync(
            string testTypeId,
            string absId,
            UpdateAssessmentTestDto updateDto);

        /// <summary>
        /// Delete an assessment test result
        /// </summary>
        Task<bool> DeleteAssessmentTestAsync(string testTypeId, string absId);

        /// <summary>
        /// Check if assessment test exists
        /// </summary>
        Task<bool> AssessmentTestExistsAsync(string testTypeId, string absId);

        /// <summary>
        /// Get assessment tests by recorder
        /// </summary>
        Task<(List<AssessmentTestInfoDto> AssessmentTests, int TotalCount)> GetAssessmentTestsByRecorderAsync(
            Guid recordedBy,
            int pageNumber = 1,
            int pageSize = 10);

        /// <summary>
        /// Bulk operations on assessment tests
        /// </summary>
        Task<int> BulkOperationAsync(BulkAssessmentTestOperationDto operationDto);
    }
}
