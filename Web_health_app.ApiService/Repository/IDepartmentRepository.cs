using Web_health_app.Models.Models;

namespace Web_health_app.ApiService.Repository
{
    public interface IDepartmentRepository
    {
        /// <summary>
        /// Get all departments with pagination and search
        /// </summary>
        /// <param name="pageNumber">Page number (default: 1)</param>
        /// <param name="pageSize">Page size (default: 20)</param>
        /// <param name="searchTerm">Search term for filtering</param>
        /// <returns>Paginated list of departments</returns>
        Task<(List<DepartmentInfoDto> Departments, int TotalCount)> GetAllDepartmentsAsync(
            int pageNumber = 1, 
            int pageSize = 20, 
            string? searchTerm = null);

        /// <summary>
        /// Search departments with advanced filters
        /// </summary>
        /// <param name="searchDto">Search criteria</param>
        /// <returns>Filtered list of departments</returns>
        Task<(List<DepartmentInfoDto> Departments, int TotalCount)> SearchDepartmentsAsync(
            DepartmentSearchDto searchDto);

        /// <summary>
        /// Get department by code
        /// </summary>
        /// <param name="departmentCode">Department code</param>
        /// <returns>Department information</returns>
        Task<DepartmentInfoDto?> GetDepartmentByCodeAsync(string departmentCode);

        /// <summary>
        /// Get all departments with student count
        /// </summary>
        /// <param name="pageNumber">Page number (default: 1)</param>
        /// <param name="pageSize">Page size (default: 20)</param>
        /// <param name="searchTerm">Search term for filtering</param>
        /// <returns>Paginated list of departments with student count</returns>
        Task<(List<DepartmentSummaryDto> Departments, int TotalCount)> GetDepartmentsWithStudentCountAsync(
            int pageNumber = 1, 
            int pageSize = 20, 
            string? searchTerm = null);

        /// <summary>
        /// Get department statistics
        /// </summary>
        /// <returns>Department statistics</returns>
        Task<DepartmentStatisticsDto> GetDepartmentStatisticsAsync();

        /// <summary>
        /// Get all departments (simple list without pagination)
        /// </summary>
        /// <returns>All departments</returns>
        Task<List<DepartmentInfoDto>> GetAllDepartmentsSimpleAsync();

        /// <summary>
        /// Get departments by battalion
        /// </summary>
        /// <param name="battalion">Battalion name</param>
        /// <returns>List of departments in the battalion</returns>
        Task<List<DepartmentInfoDto>> GetDepartmentsByBattalionAsync(string battalion);

        /// <summary>
        /// Get departments by course
        /// </summary>
        /// <param name="course">Course name</param>
        /// <returns>List of departments in the course</returns>
        Task<List<DepartmentInfoDto>> GetDepartmentsByCourseAsync(string course);

        /// <summary>
        /// Get distinct battalions
        /// </summary>
        /// <returns>List of unique battalion names</returns>
        Task<List<string>> GetBattalionsAsync();

        /// <summary>
        /// Get distinct courses
        /// </summary>
        /// <returns>List of unique course names</returns>
        Task<List<string>> GetCoursesAsync();

        /// <summary>
        /// Get distinct character codes
        /// </summary>
        /// <returns>List of unique character codes</returns>
        Task<List<string>> GetCharacterCodesAsync();
    }
}
