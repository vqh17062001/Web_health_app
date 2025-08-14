using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web_health_app.ApiService.Repository.Interface;
using Web_health_app.Models.Models;

namespace Web_health_app.ApiService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AssessmentTestController : ControllerBase
    {
        private readonly IAssessmentTestRepository _assessmentTestRepository;

        public AssessmentTestController(IAssessmentTestRepository assessmentTestRepository)
        {
            _assessmentTestRepository = assessmentTestRepository ?? throw new ArgumentNullException(nameof(assessmentTestRepository));
        }

        /// <summary>
        /// Get all assessment tests with pagination and optional search
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "READ.AssessmentTests")]
        public async Task<ActionResult<AssessmentTestsApiResponse>> GetAllAssessmentTests(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? searchTerm = null)
        {
            try
            {
                var (assessmentTests, totalCount) = await _assessmentTestRepository.GetAllAssessmentTestsAsync(pageNumber, pageSize, searchTerm);

                var response = new AssessmentTestsApiResponse
                {
                    AssessmentTests = assessmentTests,
                    Pagination = new AssessmentTestsPaginationInfo
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
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>
        /// Search assessment tests with filters
        /// </summary>
        [HttpPost("search")]
        [Authorize(Roles = "READ.AssessmentTests")]
        public async Task<ActionResult<AssessmentTestsApiResponse>> SearchAssessmentTests([FromBody] AssessmentTestSearchDto searchDto)
        {
            try
            {
                var (assessmentTests, totalCount) = await _assessmentTestRepository.SearchAssessmentTestsAsync(searchDto);

                var response = new AssessmentTestsApiResponse
                {
                    AssessmentTests = assessmentTests,
                    Pagination = new AssessmentTestsPaginationInfo
                    {
                        CurrentPage = searchDto.Page,
                        PageSize = searchDto.PageSize,
                        TotalCount = totalCount,
                        TotalPages = (int)Math.Ceiling((double)totalCount / searchDto.PageSize),
                        HasNextPage = searchDto.Page * searchDto.PageSize < totalCount,
                        HasPreviousPage = searchDto.Page > 1
                    }
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>
        /// Get assessment test by composite key (TestTypeId and AbsId)
        /// </summary>
        [HttpGet("{testTypeId}/{absId}")]
        [Authorize(Roles = "READ.AssessmentTests")]
        public async Task<ActionResult<AssessmentTestInfoDto>> GetAssessmentTestById(string testTypeId, string absId)
        {
            try
            {
                var assessmentTest = await _assessmentTestRepository.GetAssessmentTestByIdAsync(testTypeId, absId);

                if (assessmentTest == null)
                {
                    return NotFound(new { message = "Assessment test not found" });
                }

                return Ok(assessmentTest);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>
        /// Get all assessment tests for a specific student (by AbsId)
        /// </summary>
        [HttpGet("abs/{absId}")]
        [Authorize(Roles = "READ.AssessmentTests")]
        public async Task<ActionResult<List<AssessmentTestInfoDto>>> GetAssessmentTestsByAbsId(string absId)
        {
            try
            {
                var assessmentTests = await _assessmentTestRepository.GetAssessmentTestsByAbsIdAsync(absId);
                return Ok(assessmentTests);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>
        /// Get assessment tests by assessment batch ID with pagination
        /// </summary>
        [HttpGet("batch/{batchId}")]
        [Authorize(Roles = "READ.AssessmentTests")]
        public async Task<ActionResult<AssessmentTestsApiResponse>> GetAssessmentTestsByBatchId(
            string batchId,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                var (assessmentTests, totalCount) = await _assessmentTestRepository.GetAssessmentTestsByBatchIdAsync(batchId, pageNumber, pageSize);

                var response = new AssessmentTestsApiResponse
                {
                    AssessmentTests = assessmentTests,
                    Pagination = new AssessmentTestsPaginationInfo
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
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>
        /// Get assessment tests by recorder with pagination
        /// </summary>
        [HttpGet("recorder/{recordedBy}")]
        [Authorize(Roles = "READ.AssessmentTests")]
        public async Task<ActionResult<AssessmentTestsApiResponse>> GetAssessmentTestsByRecorder(
            Guid recordedBy,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                var (assessmentTests, totalCount) = await _assessmentTestRepository.GetAssessmentTestsByRecorderAsync(recordedBy, pageNumber, pageSize);

                var response = new AssessmentTestsApiResponse
                {
                    AssessmentTests = assessmentTests,
                    Pagination = new AssessmentTestsPaginationInfo
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
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>
        /// Create new assessment 
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "CREATE.AssessmentTests")]
        public async Task<ActionResult<AssessmentTestInfoDto>> CreateAssessmentTest([FromBody] CreateAssessmentTestDto createDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var assessmentTest = await _assessmentTestRepository.CreateAssessmentTestAsync(createDto);

                return CreatedAtAction(
                    nameof(GetAssessmentTestById),
                    new { testTypeId = createDto.TestTypeId, absId = createDto.AbsId },
                    assessmentTest);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>
        /// Update assessment test
        /// </summary>
        [HttpPut("{testTypeId}/{absId}")]
        [Authorize(Roles = "UPDATE.AssessmentTests")]
        public async Task<ActionResult<AssessmentTestInfoDto>> UpdateAssessmentTest(
            string testTypeId,
            string absId,
            [FromBody] UpdateAssessmentTestDto updateDto)
        {
            try
            {
                var assessmentTest = await _assessmentTestRepository.UpdateAssessmentTestAsync(testTypeId, absId, updateDto);

                if (assessmentTest == null)
                {
                    return NotFound(new { message = "Assessment test not found" });
                }

                return Ok(assessmentTest);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>
        /// Delete assessment test
        /// </summary>
        [HttpDelete("{testTypeId}/{absId}")]
        [Authorize(Roles = "DELETE.AssessmentTests")]
        public async Task<ActionResult> DeleteAssessmentTest(string testTypeId, string absId)
        {
            try
            {
                var deleted = await _assessmentTestRepository.DeleteAssessmentTestAsync(testTypeId, absId);

                if (!deleted)
                {
                    return NotFound(new { message = "Assessment test not found" });
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>
        /// Check if assessment test exists
        /// </summary>
        [HttpHead("{testTypeId}/{absId}")]
        [Authorize(Roles = "READ.AssessmentTests")]
        public async Task<ActionResult> AssessmentTestExists(string testTypeId, string absId)
        {
            try
            {
                var exists = await _assessmentTestRepository.AssessmentTestExistsAsync(testTypeId, absId);

                if (exists)
                {
                    return Ok();
                }

                return NotFound();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>
        /// Bulk operations on assessment tests
        /// </summary>
        [HttpPost("bulk")]
        [Authorize(Roles = "UPDATE.AssessmentTests,DELETE.AssessmentTests")]
        public async Task<ActionResult> BulkOperation([FromBody] BulkAssessmentTestOperationDto operationDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var affectedCount = await _assessmentTestRepository.BulkOperationAsync(operationDto);

                return Ok(new { message = $"Operation completed successfully. {affectedCount} records affected." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}
