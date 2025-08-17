using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using Web_health_app.ApiService.Repository;
using Web_health_app.Models.Models;

namespace Web_health_app.ApiService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AssessmentBatchController : ControllerBase
    {
        private readonly IAssessmentBatchRepository _assessmentBatchRepository;

        public AssessmentBatchController(IAssessmentBatchRepository assessmentBatchRepository)
        {
            _assessmentBatchRepository = assessmentBatchRepository;
        }

        /// <summary>
        /// Get all assessment batches with pagination and search
        /// </summary>CREATE_AssessmentBatch_ADMIN
        /// <param name="pageNumber">Page number (default: 1)</param>
        /// <param name="pageSize">Page size (default: 10)</param>
        /// <param name="searchTerm">Search term for filtering</param>
        /// <param name="includeInactive">Include inactive assessment batches</param>
        /// <returns>Paginated list of assessment batches</returns>
        [HttpGet]
        [Authorize(Roles = "READ.AssessmentBatch,READ_SELF_MANAGED.AssessmentBatch")]
        public async Task<ActionResult> GetAllAssessmentBatches(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? searchTerm = null,
            [FromQuery] bool includeInactive = false,
            [FromQuery] string currenUserId = "")
        {
            try
            {
                if (pageNumber < 1) pageNumber = 1;
                if (pageSize < 1 || pageSize > 100) pageSize = 10;

                var (assessmentBatches, totalCount) = await _assessmentBatchRepository.GetAllAssessmentBatchesAsync(pageNumber, pageSize, searchTerm, includeInactive, currenUserId);

                var response = new AssessmentBatchesApiResponse
                {
                    AssessmentBatches = assessmentBatches,
                    Pagination = new AssessmentBatchesPaginationInfo
                    {
                        CurrentPage = pageNumber,
                        PageSize = pageSize,
                        TotalCount = totalCount,
                        TotalPages = (int)Math.Ceiling((double)totalCount / pageSize),
                        HasNextPage = pageNumber < (int)Math.Ceiling((double)totalCount / pageSize),
                        HasPreviousPage = pageNumber > 1
                    }
                };

                return Ok(new { IsSuccess = true, Data = response, Message = "Assessment batches retrieved successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { IsSuccess = false, Message = $"Internal server error: {ex.Message}" });
            }
        }

        /// <summary>
        /// Search assessment batches with advanced filtering
        /// </summary>
        /// <param name="searchDto">Search criteria</param>
        /// <returns>Filtered list of assessment batches</returns>
        [HttpPost("search")]
        [Authorize(Roles = "READ.AssessmentBatch,READ_SELF_MANAGED.AssessmentBatch")]

        public async Task<ActionResult> SearchAssessmentBatches([FromBody] AssessmentBatchSearchDto searchDto)
        {
            try
            {
                if (searchDto == null)
                {
                    return BadRequest(new { IsSuccess = false, Message = "Search criteria is required" });
                }

                if (searchDto.Page < 1) searchDto.Page = 1;
                if (searchDto.PageSize < 1 || searchDto.PageSize > 100) searchDto.PageSize = 10;

                var (assessmentBatches, totalCount) = await _assessmentBatchRepository.SearchAssessmentBatchesAsync(searchDto);

                var response = new AssessmentBatchesApiResponse
                {
                    AssessmentBatches = assessmentBatches,
                    Pagination = new AssessmentBatchesPaginationInfo
                    {
                        CurrentPage = searchDto.Page,
                        PageSize = searchDto.PageSize,
                        TotalCount = totalCount,
                        TotalPages = (int)Math.Ceiling((double)totalCount / searchDto.PageSize),
                        HasNextPage = searchDto.Page < (int)Math.Ceiling((double)totalCount / searchDto.PageSize),
                        HasPreviousPage = searchDto.Page > 1
                    }
                };

                return Ok(new { IsSuccess = true, Data = response, Message = "Assessment batches searched successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { IsSuccess = false, Message = $"Internal server error: {ex.Message}" });
            }
        }

        /// <summary>
        /// Get assessment batch by ID
        /// </summary>
        /// <param name="id">Assessment batch ID</param>
        /// <returns>Assessment batch information</returns>
        [HttpGet("{id}")]
        [Authorize(Roles = "READ.AssessmentBatch,READ_SELF_MANAGED.AssessmentBatch")]
        public async Task<ActionResult> GetAssessmentBatchById(string id)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(id))
                {
                    return BadRequest(new { IsSuccess = false, Message = "Assessment batch ID is required" });
                }

                var assessmentBatch = await _assessmentBatchRepository.GetAssessmentBatchByIdAsync(id);

                if (assessmentBatch == null)
                {
                    return NotFound(new { IsSuccess = false, Message = "Assessment batch not found" });
                }

                return Ok(new { IsSuccess = true, Data = assessmentBatch, Message = "Assessment batch retrieved successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { IsSuccess = false, Message = $"Internal server error: {ex.Message}" });
            }
        }

        /// <summary>
        /// Get detailed assessment batch information with students and tests
        /// </summary>
        /// <param name="id">Assessment batch ID</param>
        /// <returns>Detailed assessment batch information</returns>
        [HttpGet("{id}/detail")]
        [Authorize(Roles = "READ.AssessmentBatch,READ_SELF_MANAGED.AssessmentBatch")]
        public async Task<ActionResult> GetAssessmentBatchDetail(string id)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(id))
                {
                    return BadRequest(new { IsSuccess = false, Message = "Assessment batch ID is required" });
                }

                var assessmentBatchDetail = await _assessmentBatchRepository.GetAssessmentBatchDetailAsync(id);

                if (assessmentBatchDetail == null)
                {
                    return NotFound(new { IsSuccess = false, Message = "Assessment batch not found" });
                }

                return Ok(new { IsSuccess = true, Data = assessmentBatchDetail, Message = "Assessment batch detail retrieved successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { IsSuccess = false, Message = $"Internal server error: {ex.Message}" });
            }
        }

        /// <summary>
        /// Create a new assessment batch
        /// </summary>
        /// <param name="createDto">Assessment batch creation data</param>
        /// <returns>Created assessment batch information</returns>
        [HttpPost]
        [Authorize(Roles = "CREATE.AssessmentBatch")]
        public async Task<ActionResult> CreateAssessmentBatch([FromBody] CreateAssessmentBatchDto createDto)
        {
            try
            {
                if (createDto == null)
                {
                    return BadRequest(new { IsSuccess = false, Message = "Assessment batch data is required" });
                }

                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                    return BadRequest(new { IsSuccess = false, Message = "Validation failed", Errors = errors });
                }

                var createdAssessmentBatch = await _assessmentBatchRepository.CreateAssessmentBatchAsync(createDto);

                if (createdAssessmentBatch == null)
                {
                    return StatusCode(500, new { IsSuccess = false, Message = "Failed to create assessment batch" });
                }

                return CreatedAtAction(nameof(GetAssessmentBatchById), new { id = createdAssessmentBatch.AssessmentBatchId },
                    new { IsSuccess = true, Data = createdAssessmentBatch, Message = "Assessment batch created successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { IsSuccess = false, Message = $"Internal server error: {ex.Message}" });
            }
        }

        /// <summary>
        /// Update an existing assessment batch
        /// </summary>
        /// <param name="id">Assessment batch ID</param>
        /// <param name="updateDto">Assessment batch update data</param>
        /// <returns>Updated assessment batch information</returns>
        [HttpPut("{id}")]
        [Authorize(Roles = "UPDATE.AssessmentBatch,UPDATE_SELF_MANAGED.AssessmentBatch")]
        public async Task<ActionResult> UpdateAssessmentBatch(string id, [FromBody] UpdateAssessmentBatchDto updateDto)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(id))
                {
                    return BadRequest(new { IsSuccess = false, Message = "Assessment batch ID is required" });
                }

                if (updateDto == null)
                {
                    return BadRequest(new { IsSuccess = false, Message = "Update data is required" });
                }

                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                    return BadRequest(new { IsSuccess = false, Message = "Validation failed", Errors = errors });
                }

                var updatedAssessmentBatch = await _assessmentBatchRepository.UpdateAssessmentBatchAsync(id, updateDto);

                if (updatedAssessmentBatch == null)
                {
                    return NotFound(new { IsSuccess = false, Message = "Assessment batch not found" });
                }

                return Ok(new { IsSuccess = true, Data = updatedAssessmentBatch, Message = "Assessment batch updated successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { IsSuccess = false, Message = $"Internal server error: {ex.Message}" });
            }
        }

        /// <summary>
        /// Delete an assessment batch (hard delete)
        /// </summary>
        /// <param name="id">Assessment batch ID</param>
        /// <returns>Success status</returns>
        [HttpDelete("{id}")]
        [Authorize(Roles = "DELETE.AssessmentBatch,DELETE_SELF_MANAGED.AssessmentBatch")]
        public async Task<ActionResult> DeleteAssessmentBatch(string id)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(id))
                {
                    return BadRequest(new { IsSuccess = false, Message = "Assessment batch ID is required" });
                }

                var result = await _assessmentBatchRepository.DeleteAssessmentBatchAsync(id);

                if (!result)
                {
                    return NotFound(new { IsSuccess = false, Message = "Assessment batch not found" });
                }

                return Ok(new { IsSuccess = true, Message = "Assessment batch deleted successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { IsSuccess = false, Message = $"Internal server error: {ex.Message}" });
            }
        }

        /// <summary>
        /// Soft delete an assessment batch (mark as inactive)
        /// </summary>
        /// <param name="id">Assessment batch ID</param>
        /// <returns>Success status</returns>
        [HttpPatch("{id}/soft-delete")]
        [Authorize(Roles = "DELETE.AssessmentBatch,DELETE_SELF_MANAGED.AssessmentBatch")]
        public async Task<ActionResult> SoftDeleteAssessmentBatch(string id)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(id))
                {
                    return BadRequest(new { IsSuccess = false, Message = "Assessment batch ID is required" });
                }

                var result = await _assessmentBatchRepository.SoftDeleteAssessmentBatchAsync(id);

                if (!result)
                {
                    return NotFound(new { IsSuccess = false, Message = "Assessment batch not found" });
                }

                return Ok(new { IsSuccess = true, Message = "Assessment batch soft deleted successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { IsSuccess = false, Message = $"Internal server error: {ex.Message}" });
            }
        }

        /// <summary>
        /// Get assessment batch statistics
        /// </summary>
        /// <returns>Assessment batch statistics</returns>
        [HttpGet("statistics")]
        [Authorize(Roles = "READ.AssessmentBatch,READ_SELF_MANAGED.AssessmentBatch")]
        public async Task<ActionResult> GetAssessmentBatchStatistics()
        {
            try
            {
                var statistics = await _assessmentBatchRepository.GetAssessmentBatchStatisticsAsync();

                return Ok(new { IsSuccess = true, Data = statistics, Message = "Assessment batch statistics retrieved successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { IsSuccess = false, Message = $"Internal server error: {ex.Message}" });
            }
        }

        /// <summary>
        /// Assign students to an assessment batch
        /// </summary>
        /// <param name="id">Assessment batch ID</param>
        /// <param name="studentIds">List of student IDs to assign</param>
        /// <returns>Number of students assigned</returns>
        [HttpPost("{id}/students")]
        [Authorize(Roles = "UPDATE.AssessmentBatch,UPDATE_SELF_MANAGED.AssessmentBatch")]
        public async Task<ActionResult> AssignStudentsToAssessmentBatch(string id, [FromBody] List<string> studentIds)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(id))
                {
                    return BadRequest(new { IsSuccess = false, Message = "Assessment batch ID is required" });
                }

                if (studentIds == null || !studentIds.Any())
                {
                    return BadRequest(new { IsSuccess = false, Message = "Student IDs are required" });
                }

                var assignedCount = await _assessmentBatchRepository.AssignStudentsToAssessmentBatchAsync(id, studentIds);

                return Ok(new { IsSuccess = true, Data = assignedCount , Message = $"{assignedCount} students assigned successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { IsSuccess = false, Message = $"Internal server error: {ex.Message}" });
            }
        }

        /// <summary>
        /// Remove students from an assessment batch
        /// </summary>
        /// <param name="id">Assessment batch ID</param>
        /// <param name="studentIds">List of student IDs to remove</param>
        /// <returns>Number of students removed</returns>
        [HttpDelete("{id}/students")]
        [Authorize(Roles = "UPDATE.AssessmentBatch,UPDATE_SELF_MANAGED.AssessmentBatch")]
        public async Task<ActionResult> RemoveStudentsFromAssessmentBatch(string id, [FromBody] List<string> studentIds)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(id))
                {
                    return BadRequest(new { IsSuccess = false, Message = "Assessment batch ID is required" });
                }

                if (studentIds == null || !studentIds.Any())
                {
                    return BadRequest(new { IsSuccess = false, Message = "Student IDs are required" });
                }

                var removedCount = await _assessmentBatchRepository.RemoveStudentsFromAssessmentBatchAsync(id, studentIds);

                return Ok(new { IsSuccess = true, Data = removedCount , Message = $"{removedCount} students removed successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { IsSuccess = false, Message = $"Internal server error: {ex.Message}" });
            }
        }

        /// <summary>
        /// Get students in an assessment batch
        /// </summary>
        /// <param name="id">Assessment batch ID</param>
        /// <param name="pageNumber">Page number (default: 1)</param>
        /// <param name="pageSize">Page size (default: 10)</param>
        /// <returns>Paginated list of students in the assessment batch</returns>
        [HttpGet("{id}/students")]
        [Authorize(Roles = "READ.AssessmentBatch,READ_SELF_MANAGED.AssessmentBatch")]
        public async Task<ActionResult> GetStudentsInAssessmentBatch(
            string id,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(id))
                {
                    return BadRequest(new { IsSuccess = false, Message = "Assessment batch ID is required" });
                }

                if (pageNumber < 1) pageNumber = 1;
                if (pageSize < 1 || pageSize > 100) pageSize = 10;

                var (students, totalCount) = await _assessmentBatchRepository.GetStudentsInAssessmentBatchAsync(id, pageNumber, pageSize);

                var response = new
                {
                    students = students,
                    pagination = new
                    {
                        currentPage = pageNumber,
                        pageSize = pageSize,
                        totalCount = totalCount,
                        totalPages = (int)Math.Ceiling((double)totalCount / pageSize),
                        hasNextPage = pageNumber < (int)Math.Ceiling((double)totalCount / pageSize),
                        hasPreviousPage = pageNumber > 1
                    }
                };

                return Ok(new { IsSuccess = true, Data = response, Message = "Students in assessment batch retrieved successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { IsSuccess = false, Message = $"Internal server error: {ex.Message}" });
            }
        }

        /// <summary>
        /// Get assessment batches by creator
        /// </summary>
        /// <param name="createdBy">Creator user ID</param>
        /// <param name="pageNumber">Page number (default: 1)</param>
        /// <param name="pageSize">Page size (default: 10)</param>
        /// <returns>Paginated list of assessment batches created by the user</returns>
        [HttpGet("creator/{createdBy}")]
        [Authorize(Roles = "READ.AssessmentBatch,READ_SELF_MANAGED.AssessmentBatch")]
        public async Task<ActionResult> GetAssessmentBatchesByCreator(
            Guid createdBy,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                if (createdBy == Guid.Empty)
                {
                    return BadRequest(new { IsSuccess = false, Message = "Creator ID is required" });
                }

                if (pageNumber < 1) pageNumber = 1;
                if (pageSize < 1 || pageSize > 100) pageSize = 10;

                var (assessmentBatches, totalCount) = await _assessmentBatchRepository.GetAssessmentBatchesByCreatorAsync(createdBy, pageNumber, pageSize);

                var response = new AssessmentBatchesApiResponse
                {
                    AssessmentBatches = assessmentBatches,
                    Pagination = new AssessmentBatchesPaginationInfo
                    {
                        CurrentPage = pageNumber,
                        PageSize = pageSize,
                        TotalCount = totalCount,
                        TotalPages = (int)Math.Ceiling((double)totalCount / pageSize),
                        HasNextPage = pageNumber < (int)Math.Ceiling((double)totalCount / pageSize),
                        HasPreviousPage = pageNumber > 1
                    }
                };

                return Ok(new { IsSuccess = true, Data = response, Message = "Assessment batches by creator retrieved successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { IsSuccess = false, Message = $"Internal server error: {ex.Message}" });
            }
        }

        /// <summary>
        /// Get active assessment batches
        /// </summary>
        /// <param name="pageNumber">Page number (default: 1)</param>
        /// <param name="pageSize">Page size (default: 10)</param>
        /// <returns>Paginated list of active assessment batches</returns>
        [HttpGet("active")]
        [Authorize(Roles = "READ.AssessmentBatch,READ_SELF_MANAGED.AssessmentBatch")]
        public async Task<ActionResult> GetActiveAssessmentBatches(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                if (pageNumber < 1) pageNumber = 1;
                if (pageSize < 1 || pageSize > 100) pageSize = 10;

                var (assessmentBatches, totalCount) = await _assessmentBatchRepository.GetActiveAssessmentBatchesAsync(pageNumber, pageSize);

                var response = new AssessmentBatchesApiResponse
                {
                    AssessmentBatches = assessmentBatches,
                    Pagination = new AssessmentBatchesPaginationInfo
                    {
                        CurrentPage = pageNumber,
                        PageSize = pageSize,
                        TotalCount = totalCount,
                        TotalPages = (int)Math.Ceiling((double)totalCount / pageSize),
                        HasNextPage = pageNumber < (int)Math.Ceiling((double)totalCount / pageSize),
                        HasPreviousPage = pageNumber > 1
                    }
                };

                return Ok(new { IsSuccess = true, Data = response, Message = "Active assessment batches retrieved successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { IsSuccess = false, Message = $"Internal server error: {ex.Message}" });
            }
        }

        /// <summary>
        /// Bulk operations on assessment batches
        /// </summary>
        /// <param name="operationDto">Bulk operation data</param>
        /// <returns>Number of assessment batches affected</returns>
        [HttpPost("bulk-operation")]
        [Authorize(Roles = "UPDATE.AssessmentBatch")]
        public async Task<ActionResult> BulkOperation([FromBody] BulkAssessmentBatchOperationDto operationDto)
        {
            try
            {
                if (operationDto == null)
                {
                    return BadRequest(new { IsSuccess = false, Message = "Operation data is required" });
                }

                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                    return BadRequest(new { IsSuccess = false, Message = "Validation failed", Errors = errors });
                }

                var affectedCount = await _assessmentBatchRepository.BulkOperationAsync(operationDto);

                return Ok(new { IsSuccess = true, Data = new { AffectedCount = affectedCount }, Message = $"Bulk operation completed. {affectedCount} assessment batches affected." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { IsSuccess = false, Message = $"Internal server error: {ex.Message}" });
            }
        }

        /// <summary>
        /// Get all assessment batches managed by a specific user
        /// </summary>
        /// <param name="pageNumber">Page number (default: 1)</param>
        /// <param name="pageSize">Page size (default: 20)</param>
        /// <param name="searchTerm">Search term for filtering</param>
        /// <param name="includeInactive">Include inactive assessment batches</param>
        /// <param name="managerID">Manager user ID</param>
        /// <returns>Paginated list of assessment batches managed by the user</returns>
        [HttpGet("AssessmentBatchWithManager")]
        [Authorize(Roles = "READ_SELF_MANAGED.AssessmentBatch")]
        public async Task<ActionResult<AssessmentBatchesApiResponse>> GetAllAssessmentBatchesManaged(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string? searchTerm = null,
            [FromQuery] bool includeInactive = false,
            [FromQuery] string? managerID = "")
        {
            try
            {
                if (pageNumber < 1) pageNumber = 1;
                if (pageSize < 1 || pageSize > 100) pageSize = 20;

                // Use existing GetAllAssessmentBatchesAsync method with basic parameters
                var (assessmentBatches, totalCount) = await _assessmentBatchRepository.GetAllAssessmentBatchesAsync(pageNumber, pageSize, searchTerm, includeInactive);

                // Filter by manager if specified
                if (!string.IsNullOrWhiteSpace(managerID) && Guid.TryParse(managerID, out var managerId))
                {
                    var (filteredBatches, filteredCount) = await _assessmentBatchRepository.GetAssessmentBatchesByCreatorAsync(managerId, pageNumber, pageSize);
                    assessmentBatches = filteredBatches;
                    totalCount = filteredCount;
                }

                var response = new AssessmentBatchesApiResponse
                {
                    AssessmentBatches = assessmentBatches,
                    Pagination = new AssessmentBatchesPaginationInfo
                    {
                        CurrentPage = pageNumber,
                        PageSize = pageSize,
                        TotalCount = totalCount,
                        TotalPages = (int)Math.Ceiling((double)totalCount / pageSize),
                        HasNextPage = pageNumber * pageSize < totalCount,
                        HasPreviousPage = pageNumber > 1
                    }
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving assessment batches", error = ex.Message });
            }
        }

        /// <summary>
        /// Get my assessment batches (for current user)
        /// </summary>
        /// <param name="pageNumber">Page number (default: 1)</param>
        /// <param name="pageSize">Page size (default: 20)</param>
        /// <param name="searchTerm">Search term for filtering</param>
        /// <param name="includeInactive">Include inactive assessment batches</param>
        /// <returns>Paginated list of current user's assessment batches</returns>
        [HttpGet("My")]
        [Authorize(Roles = "READ_SELF.AssessmentBatch")]
        public async Task<ActionResult<AssessmentBatchesApiResponse>> GetMyAssessmentBatches(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string? searchTerm = null,
            [FromQuery] bool includeInactive = false)
        {
            try
            {
                if (pageNumber < 1) pageNumber = 1;
                if (pageSize < 1 || pageSize > 100) pageSize = 20;

                // Get current user ID from claims
                var userIdClaim = User.FindFirst("sub") ?? User.FindFirst("id");
                if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var currentUserId))
                {
                    return Unauthorized(new { IsSuccess = false, Message = "User ID not found in token" });
                }

                var (assessmentBatches, totalCount) = await _assessmentBatchRepository.GetAssessmentBatchesByCreatorAsync(currentUserId, pageNumber, pageSize);

                var response = new AssessmentBatchesApiResponse
                {
                    AssessmentBatches = assessmentBatches,
                    Pagination = new AssessmentBatchesPaginationInfo
                    {
                        CurrentPage = pageNumber,
                        PageSize = pageSize,
                        TotalCount = totalCount,
                        TotalPages = (int)Math.Ceiling((double)totalCount / pageSize),
                        HasNextPage = pageNumber * pageSize < totalCount,
                        HasPreviousPage = pageNumber > 1
                    }
                };

                return Ok(new { IsSuccess = true, Data = response, Message = "My assessment batches retrieved successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { IsSuccess = false, Message = $"Internal server error: {ex.Message}" });
            }
        }

        /// <summary>
        /// Get recent assessment batches (from last 30 days)
        /// </summary>
        /// <param name="days">Number of days to look back (default: 30)</param>
        /// <param name="pageNumber">Page number (default: 1)</param>
        /// <param name="pageSize">Page size (default: 20)</param>
        /// <returns>Paginated list of recent assessment batches</returns>
        [HttpGet("Recent")]
        [Authorize(Roles = "READ.AssessmentBatch,READ_SELF_MANAGED.AssessmentBatch")]
        public async Task<ActionResult<AssessmentBatchesApiResponse>> GetRecentAssessmentBatches(
            [FromQuery] int days = 30,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20)
        {
            try
            {
                if (days < 1) days = 30;
                if (pageNumber < 1) pageNumber = 1;
                if (pageSize < 1 || pageSize > 100) pageSize = 20;

                // Use search functionality to filter by recent dates
                var searchDto = new AssessmentBatchSearchDto
                {
                    Page = pageNumber,
                    PageSize = pageSize,
                    CreatedFrom = DateTime.Now.AddDays(-days),
                    CreatedTo = DateTime.Now
                };

                var (assessmentBatches, totalCount) = await _assessmentBatchRepository.SearchAssessmentBatchesAsync(searchDto);

                var response = new AssessmentBatchesApiResponse
                {
                    AssessmentBatches = assessmentBatches,
                    Pagination = new AssessmentBatchesPaginationInfo
                    {
                        CurrentPage = pageNumber,
                        PageSize = pageSize,
                        TotalCount = totalCount,
                        TotalPages = (int)Math.Ceiling((double)totalCount / pageSize),
                        HasNextPage = pageNumber * pageSize < totalCount,
                        HasPreviousPage = pageNumber > 1
                    }
                };

                return Ok(new { IsSuccess = true, Data = response, Message = $"Recent assessment batches (last {days} days) retrieved successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { IsSuccess = false, Message = $"Internal server error: {ex.Message}" });
            }
        }

        /// <summary>
        /// Get assessment batches by date range
        /// </summary>
        /// <param name="startDate">Start date (format: yyyy-MM-dd)</param>
        /// <param name="endDate">End date (format: yyyy-MM-dd)</param>
        /// <param name="pageNumber">Page number (default: 1)</param>
        /// <param name="pageSize">Page size (default: 20)</param>
        /// <returns>Paginated list of assessment batches in date range</returns>
        [HttpGet("DateRange")]
        [Authorize(Roles = "READ.AssessmentBatch,READ_SELF_MANAGED.AssessmentBatch")]
        public async Task<ActionResult<AssessmentBatchesApiResponse>> GetAssessmentBatchesByDateRange(
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20)
        {
            try
            {
                if (pageNumber < 1) pageNumber = 1;
                if (pageSize < 1 || pageSize > 100) pageSize = 20;

                var searchDto = new AssessmentBatchSearchDto
                {
                    Page = pageNumber,
                    PageSize = pageSize,
                    StartDateFrom = startDate,
                    StartDateTo = endDate
                };

                var (assessmentBatches, totalCount) = await _assessmentBatchRepository.SearchAssessmentBatchesAsync(searchDto);

                var response = new AssessmentBatchesApiResponse
                {
                    AssessmentBatches = assessmentBatches,
                    Pagination = new AssessmentBatchesPaginationInfo
                    {
                        CurrentPage = pageNumber,
                        PageSize = pageSize,
                        TotalCount = totalCount,
                        TotalPages = (int)Math.Ceiling((double)totalCount / pageSize),
                        HasNextPage = pageNumber * pageSize < totalCount,
                        HasPreviousPage = pageNumber > 1
                    }
                };

                var dateRangeText = startDate.HasValue || endDate.HasValue
                    ? $" from {startDate?.ToString("yyyy-MM-dd") ?? "beginning"} to {endDate?.ToString("yyyy-MM-dd") ?? "end"}"
                    : "";

                return Ok(new { IsSuccess = true, Data = response, Message = $"Assessment batches{dateRangeText} retrieved successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { IsSuccess = false, Message = $"Internal server error: {ex.Message}" });
            }
        }

        /// <summary>
        /// Get assessment batches by status
        /// </summary>
        /// <param name="status">Assessment batch status (Active, Inactive, Completed, etc.)</param>
        /// <param name="pageNumber">Page number (default: 1)</param>
        /// <param name="pageSize">Page size (default: 20)</param>
        /// <param name="searchTerm">Search term for filtering</param>
        /// <returns>Paginated list of assessment batches with specific status</returns>
        [HttpGet("Status/{status}")]
        [Authorize(Roles = "READ.AssessmentBatch,READ_SELF_MANAGED.AssessmentBatch")]
        public async Task<ActionResult<AssessmentBatchesApiResponse>> GetAssessmentBatchesByStatus(
            string status,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string? searchTerm = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(status))
                {
                    return BadRequest(new { IsSuccess = false, Message = "Status is required" });
                }

                if (pageNumber < 1) pageNumber = 1;
                if (pageSize < 1 || pageSize > 100) pageSize = 20;

                var searchDto = new AssessmentBatchSearchDto
                {
                    Page = pageNumber,
                    PageSize = pageSize,
                    SearchTerm = searchTerm
                };

                // Try to parse status as short
                if (short.TryParse(status, out var statusValue))
                {
                    searchDto.Status = statusValue;
                }

                var (assessmentBatches, totalCount) = await _assessmentBatchRepository.SearchAssessmentBatchesAsync(searchDto);

                var response = new AssessmentBatchesApiResponse
                {
                    AssessmentBatches = assessmentBatches,
                    Pagination = new AssessmentBatchesPaginationInfo
                    {
                        CurrentPage = pageNumber,
                        PageSize = pageSize,
                        TotalCount = totalCount,
                        TotalPages = (int)Math.Ceiling((double)totalCount / pageSize),
                        HasNextPage = pageNumber * pageSize < totalCount,
                        HasPreviousPage = pageNumber > 1
                    }
                };

                return Ok(new { IsSuccess = true, Data = response, Message = $"Assessment batches with status '{status}' retrieved successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { IsSuccess = false, Message = $"Internal server error: {ex.Message}" });
            }
        }

        /// <summary>
        /// Check if assessment batch exists
        /// </summary>
        /// <param name="id">Assessment batch ID</param>
        /// <returns>Existence status</returns>
        [HttpHead("{id}")]
        [Authorize(Roles = "READ.AssessmentBatch,READ_SELF_MANAGED.AssessmentBatch")]
        public async Task<ActionResult> AssessmentBatchExists(string id)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(id))
                {
                    return BadRequest();
                }

                var exists = await _assessmentBatchRepository.AssessmentBatchExistsAsync(id);

                return exists ? Ok() : NotFound();
            }
            catch (Exception)
            {
                return StatusCode(500);
            }
        }
    }
}
