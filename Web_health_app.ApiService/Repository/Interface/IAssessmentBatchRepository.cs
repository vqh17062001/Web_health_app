using Web_health_app.Models.Models;

namespace Web_health_app.ApiService.Repository
{
    public interface IAssessmentBatchRepository
    {
        /// <summary>
        /// Get all assessment batches with pagination and search
        /// </summary>
        /// <param name="pageNumber">Page number (starting from 1)</param>
        /// <param name="pageSize">Number of items per page</param>
        /// <param name="searchTerm">Optional search term for filtering</param>
        /// <param name="includeInactive">Include inactive assessment batches in results</param>
        /// <returns>Paginated list of assessment batches</returns>
        Task<(List<AssessmentBatchInfoDto> AssessmentBatches, int TotalCount)> GetAllAssessmentBatchesAsync(int pageNumber = 1, int pageSize = 10, string? searchTerm = null, bool includeInactive = false);

        /// <summary>
        /// Get assessment batches with advanced search and filtering
        /// </summary>
        /// <param name="searchDto">Search criteria</param>
        /// <returns>Paginated list of assessment batches</returns>
        Task<(List<AssessmentBatchInfoDto> AssessmentBatches, int TotalCount)> SearchAssessmentBatchesAsync(AssessmentBatchSearchDto searchDto);

        /// <summary>
        /// Get assessment batch by ID
        /// </summary>
        /// <param name="assessmentBatchId">Assessment batch ID</param>
        /// <returns>Assessment batch information or null if not found</returns>
        Task<AssessmentBatchInfoDto?> GetAssessmentBatchByIdAsync(string assessmentBatchId);

        /// <summary>
        /// Get detailed assessment batch information with students and tests
        /// </summary>
        /// <param name="assessmentBatchId">Assessment batch ID</param>
        /// <returns>Detailed assessment batch information or null if not found</returns>
        Task<AssessmentBatchDetailDto?> GetAssessmentBatchDetailAsync(string assessmentBatchId);

        /// <summary>
        /// Create a new assessment batch
        /// </summary>
        /// <param name="createDto">Assessment batch creation data</param>
        /// <returns>Created assessment batch information</returns>
        Task<AssessmentBatchInfoDto?> CreateAssessmentBatchAsync(CreateAssessmentBatchDto createDto);

        /// <summary>
        /// Update an existing assessment batch
        /// </summary>
        /// <param name="assessmentBatchId">Assessment batch ID to update</param>
        /// <param name="updateDto">Assessment batch update data</param>
        /// <returns>Updated assessment batch information or null if not found</returns>
        Task<AssessmentBatchInfoDto?> UpdateAssessmentBatchAsync(string assessmentBatchId, UpdateAssessmentBatchDto updateDto);

        /// <summary>
        /// Delete an assessment batch
        /// </summary>
        /// <param name="assessmentBatchId">Assessment batch ID to delete</param>
        /// <returns>True if deleted successfully, false if not found</returns>
        Task<bool> DeleteAssessmentBatchAsync(string assessmentBatchId);

        /// <summary>
        /// Soft delete an assessment batch (mark as inactive)
        /// </summary>
        /// <param name="assessmentBatchId">Assessment batch ID to soft delete</param>
        /// <returns>True if soft deleted successfully, false if not found</returns>
        Task<bool> SoftDeleteAssessmentBatchAsync(string assessmentBatchId);

        /// <summary>
        /// Check if assessment batch exists
        /// </summary>
        /// <param name="assessmentBatchId">Assessment batch ID to check</param>
        /// <returns>True if exists, false otherwise</returns>
        Task<bool> AssessmentBatchExistsAsync(string assessmentBatchId);

        /// <summary>
        /// Get assessment batch statistics
        /// </summary>
        /// <returns>Assessment batch statistics</returns>
        Task<AssessmentBatchStatisticsDto> GetAssessmentBatchStatisticsAsync();

        /// <summary>
        /// Assign students to an assessment batch
        /// </summary>
        /// <param name="assessmentBatchId">Assessment batch ID</param>
        /// <param name="studentIds">List of student IDs to assign</param>
        /// <returns>Number of students successfully assigned</returns>
        Task<int> AssignStudentsToAssessmentBatchAsync(string assessmentBatchId, List<string> studentIds);

        /// <summary>
        /// Remove students from an assessment batch
        /// </summary>
        /// <param name="assessmentBatchId">Assessment batch ID</param>
        /// <param name="studentIds">List of student IDs to remove</param>
        /// <returns>Number of students successfully removed</returns>
        Task<int> RemoveStudentsFromAssessmentBatchAsync(string assessmentBatchId, List<string> studentIds);

        /// <summary>
        /// Get students in an assessment batch
        /// </summary>
        /// <param name="assessmentBatchId">Assessment batch ID</param>
        /// <param name="pageNumber">Page number</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>List of students in the assessment batch</returns>
        Task<(List<StudentInfoDto> Students, int TotalCount)> GetStudentsInAssessmentBatchAsync(string assessmentBatchId, int pageNumber = 1, int pageSize = 10);

        /// <summary>
        /// Get assessment batches by creator
        /// </summary>
        /// <param name="createdBy">Creator user ID</param>
        /// <param name="pageNumber">Page number</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>List of assessment batches created by the user</returns>
        Task<(List<AssessmentBatchInfoDto> AssessmentBatches, int TotalCount)> GetAssessmentBatchesByCreatorAsync(Guid createdBy, int pageNumber = 1, int pageSize = 10);

        /// <summary>
        /// Get active assessment batches
        /// </summary>
        /// <param name="pageNumber">Page number</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>List of active assessment batches</returns>
        Task<(List<AssessmentBatchInfoDto> AssessmentBatches, int TotalCount)> GetActiveAssessmentBatchesAsync(int pageNumber = 1, int pageSize = 10);

        /// <summary>
        /// Bulk operations on assessment batches
        /// </summary>
        /// <param name="operationDto">Bulk operation data</param>
        /// <returns>Number of assessment batches affected</returns>
        Task<int> BulkOperationAsync(BulkAssessmentBatchOperationDto operationDto);
    }
}
