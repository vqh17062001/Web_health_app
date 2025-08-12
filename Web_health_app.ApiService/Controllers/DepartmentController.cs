using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web_health_app.ApiService.Repository.Interface;
using Web_health_app.Models.Models;

namespace Web_health_app.ApiService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class DepartmentController : ControllerBase
    {
        private readonly IDepartmentRepository _departmentRepository;

        public DepartmentController(IDepartmentRepository departmentRepository)
        {
            _departmentRepository = departmentRepository;
        }

        /// <summary>
        /// Get all departments with pagination and search
        /// </summary>
        /// <param name="pageNumber">Page number (default: 1)</param>
        /// <param name="pageSize">Page size (default: 20)</param>
        /// <param name="searchTerm">Search term for filtering</param>
        /// <returns>Paginated list of departments</returns>
        [HttpGet]
        [Authorize(Roles = "READ.Department")]
        public async Task<ActionResult<DepartmentsApiResponse>> GetAllDepartments(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string? searchTerm = null)
        {
            try
            {
                if (pageNumber < 1) pageNumber = 1;
                if (pageSize < 1 || pageSize > 100) pageSize = 20;

                var (departments, totalCount) = await _departmentRepository.GetAllDepartmentsAsync(pageNumber, pageSize, searchTerm);

                var response = new DepartmentsApiResponse
                {
                    Departments = departments,
                    Pagination = new DepartmentsPaginationInfo
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
                return StatusCode(500, new { message = "An error occurred while retrieving departments", error = ex.Message });
            }
        }

        /// <summary>
        /// Search departments with advanced filters
        /// </summary>
        /// <param name="searchDto">Search criteria</param>
        /// <returns>Filtered list of departments</returns>
        [HttpPost("search")]
        [Authorize(Roles = "READ.Department")]
        public async Task<ActionResult<DepartmentsApiResponse>> SearchDepartments([FromBody] DepartmentSearchDto searchDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var (departments, totalCount) = await _departmentRepository.SearchDepartmentsAsync(searchDto);

                var response = new DepartmentsApiResponse
                {
                    Departments = departments,
                    Pagination = new DepartmentsPaginationInfo
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
                return StatusCode(500, new { message = "An error occurred while searching departments", error = ex.Message });
            }
        }

        /// <summary>
        /// Get department by code
        /// </summary>
        /// <param name="departmentCode">Department code</param>
        /// <returns>Department information</returns>
        [HttpGet("{departmentCode}")]
        [Authorize(Roles = "READ.Department")]
        public async Task<ActionResult<DepartmentInfoDto>> GetDepartmentByCode(string departmentCode)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(departmentCode))
                {
                    return BadRequest(new { message = "Department code is required" });
                }

                var department = await _departmentRepository.GetDepartmentByCodeAsync(departmentCode);
                if (department == null)
                {
                    return NotFound(new { message = "Department not found" });
                }

                return Ok(department);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving department", error = ex.Message });
            }
        }

        /// <summary>
        /// Get all departments with student count
        /// </summary>
        /// <param name="pageNumber">Page number (default: 1)</param>
        /// <param name="pageSize">Page size (default: 20)</param>
        /// <param name="searchTerm">Search term for filtering</param>
        /// <returns>Paginated list of departments with student count</returns>
        [HttpGet("with-student-count")]
        [Authorize(Roles = "READ.Department")]
        public async Task<ActionResult<object>> GetDepartmentsWithStudentCount(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string? searchTerm = null)
        {
            try
            {
                if (pageNumber < 1) pageNumber = 1;
                if (pageSize < 1 || pageSize > 100) pageSize = 20;

                var (departments, totalCount) = await _departmentRepository.GetDepartmentsWithStudentCountAsync(pageNumber, pageSize, searchTerm);

                var response = new
                {
                    Departments = departments,
                    Pagination = new DepartmentsPaginationInfo
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
                return StatusCode(500, new { message = "An error occurred while retrieving departments with student count", error = ex.Message });
            }
        }

        /// <summary>
        /// Get department statistics
        /// </summary>
        /// <returns>Department statistics</returns>
        [HttpGet("statistics")]
        [Authorize(Roles = "READ.Department")]
        public async Task<ActionResult<DepartmentStatisticsDto>> GetDepartmentStatistics()
        {
            try
            {
                var statistics = await _departmentRepository.GetDepartmentStatisticsAsync();
                return Ok(statistics);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving department statistics", error = ex.Message });
            }
        }

        /// <summary>
        /// Get all departments (simple list without pagination)
        /// </summary>
        /// <returns>All departments</returns>
        [HttpGet("simple")]
        [Authorize(Roles = "READ.Department")]
        public async Task<ActionResult<List<DepartmentInfoDto>>> GetAllDepartmentsSimple()
        {
            try
            {
                var departments = await _departmentRepository.GetAllDepartmentsSimpleAsync();
                return Ok(departments);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving departments", error = ex.Message });
            }
        }

        /// <summary>
        /// Get departments by battalion
        /// </summary>
        /// <param name="battalion">Battalion name</param>
        /// <returns>List of departments in the battalion</returns>
        [HttpGet("by-battalion/{battalion}")]
        [Authorize(Roles = "READ.Department")]
        public async Task<ActionResult<List<DepartmentInfoDto>>> GetDepartmentsByBattalion(string battalion)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(battalion))
                {
                    return BadRequest(new { message = "Battalion name is required" });
                }

                var departments = await _departmentRepository.GetDepartmentsByBattalionAsync(battalion);
                return Ok(departments);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving departments by battalion", error = ex.Message });
            }
        }

        /// <summary>
        /// Get departments by course
        /// </summary>
        /// <param name="course">Course name</param>
        /// <returns>List of departments in the course</returns>
        [HttpGet("by-course/{course}")]
        [Authorize(Roles = "READ.Department")]
        public async Task<ActionResult<List<DepartmentInfoDto>>> GetDepartmentsByCourse(string course)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(course))
                {
                    return BadRequest(new { message = "Course name is required" });
                }

                var departments = await _departmentRepository.GetDepartmentsByCourseAsync(course);
                return Ok(departments);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving departments by course", error = ex.Message });
            }
        }

        /// <summary>
        /// Get distinct battalions
        /// </summary>
        /// <returns>List of unique battalion names</returns>
        [HttpGet("battalions")]
        [Authorize(Roles = "READ.Department")]
        public async Task<ActionResult<List<string>>> GetBattalions()
        {
            try
            {
                var battalions = await _departmentRepository.GetBattalionsAsync();
                return Ok(battalions);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving battalions", error = ex.Message });
            }
        }

        /// <summary>
        /// Get distinct courses
        /// </summary>
        /// <returns>List of unique course names</returns>
        [HttpGet("courses")]
        [Authorize(Roles = "READ.Department")]
        public async Task<ActionResult<List<string>>> GetCourses()
        {
            try
            {
                var courses = await _departmentRepository.GetCoursesAsync();
                return Ok(courses);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving courses", error = ex.Message });
            }
        }

        /// <summary>
        /// Get distinct character codes
        /// </summary>
        /// <returns>List of unique character codes</returns>
        [HttpGet("character-codes")]
        [Authorize(Roles = "READ.Department")]
        public async Task<ActionResult<List<string>>> GetCharacterCodes()
        {
            try
            {
                var characterCodes = await _departmentRepository.GetCharacterCodesAsync();
                return Ok(characterCodes);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving character codes", error = ex.Message });
            }
        }
    }
}
