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
    public class StudentController : ControllerBase
    {
        private readonly IStudentRepository _studentRepository;

        public StudentController(IStudentRepository studentRepository)
        {
            _studentRepository = studentRepository;
        }

        /// <summary>
        /// Get all students with pagination and search
        /// </summary>
        /// <param name="pageNumber">Page number (default: 1)</param>
        /// <param name="pageSize">Page size (default: 20)</param>
        /// <param name="searchTerm">Search term for filtering</param>
        /// <param name="includeInactive">Include inactive students</param>
        /// <returns>Paginated list of students</returns>
        [HttpGet]
        [Authorize(Roles = "READ.Students")]
        public async Task<ActionResult<StudentsApiResponse>> GetAllStudents(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string? searchTerm = null,
            [FromQuery] bool includeInactive = false)
        {
            try
            {
                if (pageNumber < 1) pageNumber = 1;
                if (pageSize < 1 || pageSize > 100) pageSize = 20;

                var (students, totalCount) = await _studentRepository.GetAllStudentsAsync(pageNumber, pageSize, searchTerm, includeInactive);

                var response = new StudentsApiResponse
                {
                    Students = students,
                    Pagination = new StudentsPaginationInfo
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
                return StatusCode(500, new { message = "An error occurred while retrieving students", error = ex.Message });
            }
        }

        [HttpGet("StudentWithManager")]
        [Authorize(Roles = "READ_SELF_MANAGED.Students")]
        public async Task<ActionResult<StudentsApiResponse>> GetAllStudentsManaged(
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

                var (students, totalCount) = await _studentRepository.GetAllStudentsAsync(pageNumber, pageSize, searchTerm, includeInactive, managerID);

                var response = new StudentsApiResponse
                {
                    Students = students,
                    Pagination = new StudentsPaginationInfo
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
                return StatusCode(500, new { message = "An error occurred while retrieving students", error = ex.Message });
            }
        }


        /// <summary>
        /// Search students with advanced filters
        /// </summary>
        /// <param name="searchDto">Search criteria</param>
        /// <returns>Filtered list of students</returns>
        [HttpPost("search")]
  
        [Authorize(Roles = "READ.Students,READ_SELF_MANAGED.Students")]

        public async Task<ActionResult<StudentsApiResponse>> SearchStudents([FromBody] StudentSearchDto searchDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var (students, totalCount) = await _studentRepository.SearchStudentsAsync(searchDto);

                var response = new StudentsApiResponse
                {
                    Students = students,
                    Pagination = new StudentsPaginationInfo
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
                return StatusCode(500, new { message = "An error occurred while searching students", error = ex.Message });
            }
        }

        /// <summary>
        /// Get student by ID
        /// </summary>
        /// <param name="studentId">Student ID</param>
        /// <returns>Student information</returns>
        [HttpGet("{studentId}")]
        [Authorize(Roles = "READ.Students,READ_SELF_MANAGED.Students")]

        public async Task<ActionResult<StudentInfoDto>> GetStudentById(string studentId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(studentId))
                {
                    return BadRequest(new { message = "Student ID is required" });
                }

                var student = await _studentRepository.GetStudentByIdAsync(studentId);
                if (student == null)
                {
                    return NotFound(new { message = "Student not found" });
                }

                return Ok(student);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving student", error = ex.Message });
            }
        }

        /// <summary>
        /// Get student with detailed information
        /// </summary>
        /// <param name="studentId">Student ID</param>
        /// <returns>Student detailed information</returns>
        [HttpGet("{studentId}/detail")]
        [Authorize(Roles = "READ.Students")]
        public async Task<ActionResult<StudentDetailDto>> GetStudentDetail(string studentId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(studentId))
                {
                    return BadRequest(new { message = "Student ID is required" });
                }

                var studentDetail = await _studentRepository.GetStudentDetailAsync(studentId);
                if (studentDetail == null)
                {
                    return NotFound(new { message = "Student not found" });
                }

                return Ok(studentDetail);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving student detail", error = ex.Message });
            }
        }

        /// <summary>
        /// Create new student
        /// </summary>
        /// <param name="createStudentDto">Student creation data</param>
        /// <returns>Created student information</returns>
        [HttpPost]
        [Authorize(Roles = "CREATE.Students")]
        public async Task<ActionResult<StudentInfoDto>> CreateStudent([FromBody] CreateStudentDto createStudentDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var createdStudent = await _studentRepository.CreateStudentAsync(createStudentDto);
                return CreatedAtAction(nameof(GetStudentById), new { studentId = createdStudent.StudentId }, createdStudent);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while creating student", error = ex.Message });
            }
        }

        /// <summary>
        /// Update student information
        /// </summary>
        /// <param name="studentId">Student ID</param>
        /// <param name="updateStudentDto">Updated student data</param>
        /// <returns>Updated student information</returns>
        [HttpPut("{studentId}")]
       
        [Authorize(Roles = "UPDATE.Students,UPDATE_SELF_MANAGED.Students")]

        public async Task<ActionResult<StudentInfoDto>> UpdateStudent(string studentId, [FromBody] UpdateStudentDto updateStudentDto)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(studentId))
                {
                    return BadRequest(new { message = "Student ID is required" });
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var updatedStudent = await _studentRepository.UpdateStudentAsync(studentId, updateStudentDto);
                if (updatedStudent == null)
                {
                    return NotFound(new { message = "Student not found" });
                }

                return Ok(updatedStudent);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating student", error = ex.Message });
            }
        }

        /// <summary>
        /// Delete student (soft delete)
        /// </summary>
        /// <param name="studentId">Student ID</param>
        /// <returns>Success message</returns>
        [HttpDelete("{studentId}")]
       
        [Authorize(Roles = "DELETE.Students,DELETE_SELF_MANAGED.Students")]

        public async Task<ActionResult> DeleteStudent(string studentId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(studentId))
                {
                    return BadRequest(new { message = "Student ID is required" });
                }

                var result = await _studentRepository.DeleteStudentAsync(studentId);
                if (!result)
                {
                    return NotFound(new { message = "Student not found" });
                }

                return Ok(new { message = "Student deleted successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while deleting student", error = ex.Message });
            }
        }

        /// <summary>
        /// Permanently delete student
        /// </summary>
        /// <param name="studentId">Student ID</param>
        /// <returns>Success message</returns>
        [HttpDelete("{studentId}/permanent")]
        [Authorize(Roles = "DELETE.Students")]
        public async Task<ActionResult> HardDeleteStudent(string studentId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(studentId))
                {
                    return BadRequest(new { message = "Student ID is required" });
                }

                var result = await _studentRepository.HardDeleteStudentAsync(studentId);
                if (!result)
                {
                    return NotFound(new { message = "Student not found" });
                }

                return Ok(new { message = "Student permanently deleted" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while permanently deleting student", error = ex.Message });
            }
        }

        /// <summary>
        /// Check if student ID exists
        /// </summary>
        /// <param name="studentId">Student ID to check</param>
        /// <returns>Boolean indicating if student ID exists</returns>
        [HttpGet("check-studentid/{studentId}")]
        [Authorize(Roles = "READ.Students")]
        public async Task<ActionResult> CheckStudentId(string studentId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(studentId))
                {
                    return BadRequest(new { message = "Student ID is required" });
                }

                var exists = await _studentRepository.StudentIdExistsAsync(studentId);
                return Ok(new { studentId = studentId, exists = exists });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while checking student ID", error = ex.Message });
            }
        }

        /// <summary>
        /// Get students by department
        /// </summary>
        /// <param name="department">Department name</param>
        /// <returns>List of students in the department</returns>
        [HttpGet("department/{department}")]
        [Authorize(Roles = "READ.Students")]
        public async Task<ActionResult<List<StudentInfoDto>>> GetStudentsByDepartment(string department)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(department))
                {
                    return BadRequest(new { message = "Department is required" });
                }

                var students = await _studentRepository.GetStudentsByDepartmentAsync(department);
                return Ok(new
                {
                    department = department,
                    students = students,
                    totalCount = students.Count
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving students by department", error = ex.Message });
            }
        }

        /// <summary>
        /// Get students managed by specific user
        /// </summary>
        /// <param name="managerId">Manager user ID</param>
        /// <returns>List of students managed by the user</returns>
        [HttpGet("manager/{managerId}")]
        [Authorize(Roles = "READ.Students")]
        public async Task<ActionResult<List<StudentInfoDto>>> GetStudentsByManager(Guid managerId)
        {
            try
            {
                var students = await _studentRepository.GetStudentsByManagerAsync(managerId);
                return Ok(new
                {
                    managerId = managerId,
                    students = students,
                    totalCount = students.Count
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving students by manager", error = ex.Message });
            }
        }

        /// <summary>
        /// Get students by status
        /// </summary>
        /// <param name="status">Student status</param>
        /// <returns>List of students with specified status</returns>
        [HttpGet("status/{status}")]
        [Authorize(Roles = "READ.Students")]
        public async Task<ActionResult<List<StudentInfoDto>>> GetStudentsByStatus(short status)
        {
            try
            {
                var students = await _studentRepository.GetStudentsByStatusAsync(status);
                return Ok(new
                {
                    status = status,
                    students = students,
                    totalCount = students.Count
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving students by status", error = ex.Message });
            }
        }

        /// <summary>
        /// Get student statistics
        /// </summary>
        /// <returns>Student statistics summary</returns>
        [HttpGet("statistics")]
        [Authorize(Roles = "READ.Students,READ_SELF_MANAGED.Students")]
        public async Task<ActionResult<StudentStatisticsDto>> GetStudentStatistics([FromQuery]string manageId =null)
        {
            try
            {
                var statistics = await _studentRepository.GetStudentStatisticsAsync(manageId);
                return Ok(statistics);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving student statistics", error = ex.Message });
            }
        }

        /// <summary>
        /// Bulk create students
        /// </summary>
        /// <param name="createStudentDtos">List of students to create</param>
        /// <returns>List of created students</returns>
        [HttpPost("bulk")]
        [Authorize(Roles = "CREATE.Students")]
        public async Task<ActionResult<List<StudentInfoDto>>> BulkCreateStudents([FromBody] List<CreateStudentDto> createStudentDtos)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                if (createStudentDtos == null || !createStudentDtos.Any())
                {
                    return BadRequest(new { message = "No students provided for creation" });
                }

                var createdStudents = await _studentRepository.BulkCreateStudentsAsync(createStudentDtos);
                return Ok(new
                {
                    message = "Students created successfully",
                    totalCreated = createdStudents.Count,
                    students = createdStudents
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while bulk creating students", error = ex.Message });
            }
        }

        /// <summary>
        /// Bulk update students
        /// </summary>
        /// <param name="updates">Dictionary of student ID and update data</param>
        /// <returns>Number of successfully updated students</returns>
        [HttpPut("bulk")]
        [Authorize(Roles = "UPDATE.Students")]
        public async Task<ActionResult> BulkUpdateStudents([FromBody] Dictionary<string, UpdateStudentDto> updates)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                if (updates == null || !updates.Any())
                {
                    return BadRequest(new { message = "No updates provided" });
                }

                var updatedCount = await _studentRepository.BulkUpdateStudentsAsync(updates);
                return Ok(new
                {
                    message = "Students updated successfully",
                    totalUpdated = updatedCount,
                    totalRequested = updates.Count
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while bulk updating students", error = ex.Message });
            }
        }

        /// <summary>
        /// Assign manager to students
        /// </summary>
        /// <param name="assignmentDto">Student IDs and manager ID</param>
        /// <returns>Success message</returns>
        [HttpPost("assign-manager")]
        [Authorize(Roles = "UPDATE.Students")]
        public async Task<ActionResult> AssignManagerToStudents([FromBody] StudentManagerAssignmentDto assignmentDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                if (assignmentDto.StudentIds == null || !assignmentDto.StudentIds.Any())
                {
                    return BadRequest(new { message = "No student IDs provided" });
                }

                var result = await _studentRepository.AssignManagerToStudentsAsync(assignmentDto.StudentIds, assignmentDto.ManagerId);
                if (!result)
                {
                    return BadRequest(new { message = "Failed to assign manager to students" });
                }

                return Ok(new
                {
                    message = "Manager assigned successfully",
                    studentCount = assignmentDto.StudentIds.Count,
                    managerId = assignmentDto.ManagerId
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while assigning manager", error = ex.Message });
            }
        }

        /// <summary>
        /// Bulk operations on students
        /// </summary>
        /// <param name="operationDto">Bulk operation data</param>
        /// <returns>Success message</returns>
        [HttpPost("bulk-operation")]
        [Authorize(Roles = "UPDATE.Students,DELETE.Students")]
        public async Task<ActionResult> BulkOperation([FromBody] BulkStudentOperationDto operationDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                if (operationDto.StudentIds == null || !operationDto.StudentIds.Any())
                {
                    return BadRequest(new { message = "No student IDs provided" });
                }

                var result = false;
                var message = "";

                switch (operationDto.Operation?.ToLower())
                {
                    case "activate":
                        var activateUpdates = operationDto.StudentIds.ToDictionary(
                            id => id,
                            id => new UpdateStudentDto { Status = 1 }
                        );
                        var activatedCount = await _studentRepository.BulkUpdateStudentsAsync(activateUpdates);
                        result = activatedCount > 0;
                        message = $"Activated {activatedCount} students";
                        break;

                    case "deactivate":
                        var deactivateUpdates = operationDto.StudentIds.ToDictionary(
                            id => id,
                            id => new UpdateStudentDto { Status = 0 }
                        );
                        var deactivatedCount = await _studentRepository.BulkUpdateStudentsAsync(deactivateUpdates);
                        result = deactivatedCount > 0;
                        message = $"Deactivated {deactivatedCount} students";
                        break;

                    case "update_department":
                        if (string.IsNullOrWhiteSpace(operationDto.NewDepartment))
                        {
                            return BadRequest(new { message = "New department is required for update_department operation" });
                        }
                        var deptUpdates = operationDto.StudentIds.ToDictionary(
                            id => id,
                            id => new UpdateStudentDto { Department = operationDto.NewDepartment }
                        );
                        var deptUpdatedCount = await _studentRepository.BulkUpdateStudentsAsync(deptUpdates);
                        result = deptUpdatedCount > 0;
                        message = $"Updated department for {deptUpdatedCount} students";
                        break;

                    case "assign_manager":
                        if (operationDto.NewManageBy == null)
                        {
                            return BadRequest(new { message = "New manager ID is required for assign_manager operation" });
                        }
                        result = await _studentRepository.AssignManagerToStudentsAsync(operationDto.StudentIds, operationDto.NewManageBy.Value);
                        message = $"Assigned manager to {operationDto.StudentIds.Count} students";
                        break;

                    default:
                        return BadRequest(new { message = "Invalid operation. Supported operations: activate, deactivate, update_department, assign_manager" });
                }

                if (!result)
                {
                    return BadRequest(new { message = "Operation failed" });
                }

                return Ok(new { message = message, operation = operationDto.Operation });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred during bulk operation", error = ex.Message });
            }
        }
    }

    /// <summary>
    /// DTO for assigning manager to students
    /// </summary>
    public class StudentManagerAssignmentDto
    {
        [Required(ErrorMessage = "Student IDs are required")]
        public List<string> StudentIds { get; set; } = new List<string>();

        [Required(ErrorMessage = "Manager ID is required")]
        public Guid ManagerId { get; set; }
    }
}
