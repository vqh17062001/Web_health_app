using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web_health_app.ApiService.Repository.Interface;
using Web_health_app.Models.Models;

namespace Web_health_app.ApiService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TestTypeController : ControllerBase
    {
        private readonly ITestTypeRepository _testTypeRepository;

        public TestTypeController(ITestTypeRepository testTypeRepository)
        {
            _testTypeRepository = testTypeRepository ?? throw new ArgumentNullException(nameof(testTypeRepository));
        }

        /// <summary>
        /// Get all test types with pagination and optional search
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "READ.TestTypes")]
        public async Task<ActionResult<TestTypesApiResponse>> GetAllTestTypes(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? searchTerm = null)
        {
            try
            {
                if (pageNumber < 1) pageNumber = 1;
                if (pageSize < 1 || pageSize > 100) pageSize = 20;

                var (testTypes, totalCount) = await _testTypeRepository.GetAllTestTypesAsync(pageNumber, pageSize, searchTerm);

                var response = new TestTypesApiResponse
                {
                    TestTypes = testTypes,
                    Pagination = new TestTypesPaginationInfo
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
                return StatusCode(500, new { message = "An error occurred while retrieving test types", error = ex.Message });
            }
        }

        /// <summary>
        /// Search test types with advanced filters
        /// </summary>
        [HttpPost("search")]
        [Authorize(Roles = "READ.TestTypes")]
        public async Task<ActionResult<TestTypesApiResponse>> SearchTestTypes([FromBody] TestTypeSearchDto searchDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var (testTypes, totalCount) = await _testTypeRepository.SearchTestTypesAsync(searchDto);

                var response = new TestTypesApiResponse
                {
                    TestTypes = testTypes,
                    Pagination = new TestTypesPaginationInfo
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
                return StatusCode(500, new { message = "An error occurred while searching test types", error = ex.Message });
            }
        }

        /// <summary>
        /// Get test type by ID
        /// </summary>
        [HttpGet("{testTypeId}")]
        [Authorize(Roles = "READ.TestTypes")]
        public async Task<ActionResult<TestTypeInfoDto>> GetTestTypeById(string testTypeId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(testTypeId))
                {
                    return BadRequest(new { message = "Test type ID is required" });
                }

                var testType = await _testTypeRepository.GetTestTypeByIdAsync(testTypeId);

                if (testType == null)
                {
                    return NotFound(new { message = "Test type not found" });
                }

                return Ok(testType);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving test type", error = ex.Message });
            }
        }

        /// <summary>
        /// Get all test types for dropdown/select options
        /// </summary>
        [HttpGet("select-options")]
        [Authorize(Roles = "READ.TestTypes")]
        public async Task<ActionResult<List<TestTypeSelectDto>>> GetTestTypeSelectOptions()
        {
            try
            {
                var testTypes = await _testTypeRepository.GetTestTypeSelectOptionsAsync();
                return Ok(testTypes);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving test type options", error = ex.Message });
            }
        }

        /// <summary>
        /// Get test types by unit
        /// </summary>
        [HttpGet("unit/{unit}")]
        [Authorize(Roles = "READ.TestTypes")]
        public async Task<ActionResult<List<TestTypeInfoDto>>> GetTestTypesByUnit(string unit)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(unit))
                {
                    return BadRequest(new { message = "Unit is required" });
                }

                var testTypes = await _testTypeRepository.GetTestTypesByUnitAsync(unit);
                return Ok(new
                {
                    unit = unit,
                    testTypes = testTypes,
                    totalCount = testTypes.Count
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving test types by unit", error = ex.Message });
            }
        }

        /// <summary>
        /// Get test types by code pattern
        /// </summary>
        [HttpGet("code-pattern/{codePattern}")]
        [Authorize(Roles = "READ.TestTypes")]
        public async Task<ActionResult<List<TestTypeInfoDto>>> GetTestTypesByCodePattern(string codePattern)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(codePattern))
                {
                    return BadRequest(new { message = "Code pattern is required" });
                }

                var testTypes = await _testTypeRepository.GetTestTypesByCodePatternAsync(codePattern);
                return Ok(new
                {
                    codePattern = codePattern,
                    testTypes = testTypes,
                    totalCount = testTypes.Count
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving test types by code pattern", error = ex.Message });
            }
        }

        /// <summary>
        /// Check if test type exists
        /// </summary>
        [HttpHead("{testTypeId}")]
        [Authorize(Roles = "READ.TestTypes")]
        public async Task<ActionResult> TestTypeExists(string testTypeId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(testTypeId))
                {
                    return BadRequest(new { message = "Test type ID is required" });
                }

                var exists = await _testTypeRepository.TestTypeExistsAsync(testTypeId);

                if (exists)
                {
                    return Ok();
                }

                return NotFound();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while checking test type existence", error = ex.Message });
            }
        }

        /// <summary>
        /// Check if test type ID exists (returns JSON response)
        /// </summary>
        [HttpGet("check-id/{testTypeId}")]
        [Authorize(Roles = "READ.TestTypes")]
        public async Task<ActionResult> CheckTestTypeId(string testTypeId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(testTypeId))
                {
                    return BadRequest(new { message = "Test type ID is required" });
                }

                var exists = await _testTypeRepository.TestTypeExistsAsync(testTypeId);
                return Ok(new { testTypeId = testTypeId, exists = exists });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while checking test type ID", error = ex.Message });
            }
        }

        /// <summary>
        /// Get total count of test types
        /// </summary>
        [HttpGet("count")]
        [Authorize(Roles = "READ.TestTypes")]
        public async Task<ActionResult> GetTotalTestTypesCount()
        {
            try
            {
                var totalCount = await _testTypeRepository.GetTotalTestTypesCountAsync();
                return Ok(new { totalCount = totalCount });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving test types count", error = ex.Message });
            }
        }

        /// <summary>
        /// Get unique units from all test types
        /// </summary>
        [HttpGet("units")]
        [Authorize(Roles = "READ.TestTypes")]
        public async Task<ActionResult<List<string>>> GetUniqueUnits()
        {
            try
            {
                var units = await _testTypeRepository.GetUniqueUnitsAsync();
                return Ok(new
                {
                    units = units,
                    totalCount = units.Count
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving unique units", error = ex.Message });
            }
        }
    }
}
