using Web_health_app.Models.Models;

namespace Web_health_app.ApiService.Repository
{
    public interface IStudentRepository
    {
        /// <summary>
        /// Get all students with pagination and search
        /// </summary>
        /// <param name="pageNumber">Page number (starting from 1)</param>
        /// <param name="pageSize">Number of items per page</param>
        /// <param name="searchTerm">Optional search term for filtering</param>
        /// <param name="includeInactive">Include inactive students in results</param>
        /// <returns>Paginated list of students</returns>
        Task<(List<StudentInfoDto> Students, int TotalCount)> GetAllStudentsAsync(int pageNumber = 1, int pageSize = 10, string? searchTerm = null, bool includeInactive = false, string managerId = "");

        /// <summary>
        /// Get students with advanced search and filtering
        /// </summary>
        /// <param name="searchDto">Search criteria</param>
        /// <returns>Paginated list of students</returns>
        Task<(List<StudentInfoDto> Students, int TotalCount)> SearchStudentsAsync(StudentSearchDto searchDto);

        /// <summary>
        /// Get student by ID
        /// </summary>
        /// <param name="studentId">Student ID</param>
        /// <returns>Student information or null if not found</returns>
        Task<StudentInfoDto?> GetStudentByIdAsync(string studentId);

        /// <summary>
        /// Get student with detailed information (including related data)
        /// </summary>
        /// <param name="studentId">Student ID</param>
        /// <returns>Student detailed information or null if not found</returns>
        Task<StudentDetailDto?> GetStudentDetailAsync(string studentId);

        /// <summary>
        /// Create new student
        /// </summary>
        /// <param name="createStudentDto">Student creation data</param>
        /// <returns>Created student information</returns>
        Task<StudentInfoDto> CreateStudentAsync(CreateStudentDto createStudentDto);

        /// <summary>
        /// Update student information
        /// </summary>
        /// <param name="studentId">Student ID to update</param>
        /// <param name="updateStudentDto">Updated student data</param>
        /// <returns>Updated student information or null if not found</returns>
        Task<StudentInfoDto?> UpdateStudentAsync(string studentId, UpdateStudentDto updateStudentDto);

        /// <summary>
        /// Delete student (soft delete by changing status)
        /// </summary>
        /// <param name="studentId">Student ID to delete</param>
        /// <returns>True if deleted successfully, false if not found</returns>
        Task<bool> DeleteStudentAsync(string studentId);

        /// <summary>
        /// Permanently delete student
        /// </summary>
        /// <param name="studentId">Student ID to delete permanently</param>
        /// <returns>True if deleted successfully, false if not found</returns>
        Task<bool> HardDeleteStudentAsync(string studentId);

        /// <summary>
        /// Check if student ID already exists
        /// </summary>
        /// <param name="studentId">Student ID to check</param>
        /// <param name="excludeStudentId">Student ID to exclude from check (for updates)</param>
        /// <returns>True if student ID exists, false otherwise</returns>
        Task<bool> StudentIdExistsAsync(string studentId, string? excludeStudentId = null);

        /// <summary>
        /// Get students by department
        /// </summary>
        /// <param name="department">Department name</param>
        /// <returns>List of students in the department</returns>
        Task<List<StudentInfoDto>> GetStudentsByDepartmentAsync(string department);

        /// <summary>
        /// Get students managed by specific user
        /// </summary>
        /// <param name="managerId">Manager user ID</param>
        /// <returns>List of students managed by the user</returns>
        Task<List<StudentInfoDto>> GetStudentsByManagerAsync(Guid managerId);

        /// <summary>
        /// Get students by status
        /// </summary>
        /// <param name="status">Student status</param>
        /// <returns>List of students with specified status</returns>
        Task<List<StudentInfoDto>> GetStudentsByStatusAsync(short status);

        /// <summary>
        /// Get student statistics
        /// </summary>
        /// <returns>Student statistics summary</returns>
        Task<StudentStatisticsDto> GetStudentStatisticsAsync();

        /// <summary>
        /// Bulk create students
        /// </summary>
        /// <param name="createStudentDtos">List of students to create</param>
        /// <returns>List of created students</returns>
        Task<List<StudentInfoDto>> BulkCreateStudentsAsync(List<CreateStudentDto> createStudentDtos);

        /// <summary>
        /// Bulk update students
        /// </summary>
        /// <param name="updates">Dictionary of student ID and update data</param>
        /// <returns>Number of successfully updated students</returns>
        Task<int> BulkUpdateStudentsAsync(Dictionary<string, UpdateStudentDto> updates);

        /// <summary>
        /// Assign manager to students
        /// </summary>
        /// <param name="studentIds">List of student IDs</param>
        /// <param name="managerId">Manager user ID</param>
        /// <returns>True if assignment successful</returns>
        Task<bool> AssignManagerToStudentsAsync(List<string> studentIds, Guid managerId);
    }

    /// <summary>
    /// DTO for student statistics
    /// </summary>
    //public class StudentStatisticsDto
    //{
    //    public int TotalStudents { get; set; }
    //    public int ActiveStudents { get; set; }
    //    public int InactiveStudents { get; set; }
    //    public int StudentsWithSyncData { get; set; }
    //    public int StudentsOffline { get; set; }
    //    public int StudentsOnline { get; set; }
    //    public Dictionary<string, int> StudentsByDepartment { get; set; } = new Dictionary<string, int>();
    //    public Dictionary<string, int> StudentsByGender { get; set; } = new Dictionary<string, int>();
    //    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    //}
}
