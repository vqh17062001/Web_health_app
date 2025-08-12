using Microsoft.EntityFrameworkCore;
using Web_health_app.ApiService.Entities;
using Web_health_app.ApiService.Repository.Interface;
using Web_health_app.Models.Models;

namespace Web_health_app.ApiService.Repository
{
    public class DepartmentRepository : IDepartmentRepository
    {
        private readonly HealthDbContext _context;

        public DepartmentRepository(HealthDbContext context)
        {
            _context = context;
        }

        public async Task<(List<DepartmentInfoDto> Departments, int TotalCount)> GetAllDepartmentsAsync(
            int pageNumber = 1,
            int pageSize = 20,
            string? searchTerm = null)
        {
            var query = _context.Departments.AsQueryable();

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var searchLower = searchTerm.ToLower();
                query = query.Where(d =>
                    d.DepartmentCode.ToLower().Contains(searchLower) ||
                    (d.Battalion != null && d.Battalion.ToLower().Contains(searchLower)) ||
                    (d.Course != null && d.Course.ToLower().Contains(searchLower)) ||
                    (d.CharacterCode != null && d.CharacterCode.ToLower().Contains(searchLower)));
            }

            var totalCount = await query.CountAsync();

            var departments = await query
                .OrderBy(d => d.DepartmentCode)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(d => new DepartmentInfoDto
                {
                    DepartmentCode = d.DepartmentCode,
                    Battalion = d.Battalion,
                    Course = d.Course,
                    CharacterCode = d.CharacterCode
                })
                .ToListAsync();

            return (departments, totalCount);
        }

        public async Task<(List<DepartmentInfoDto> Departments, int TotalCount)> SearchDepartmentsAsync(
            DepartmentSearchDto searchDto)
        {
            var query = _context.Departments.AsQueryable();

            // Apply filters
            if (!string.IsNullOrWhiteSpace(searchDto.SearchTerm))
            {
                var searchLower = searchDto.SearchTerm.ToLower();
                query = query.Where(d =>
                    d.DepartmentCode.ToLower().Contains(searchLower) ||
                    (d.Battalion != null && d.Battalion.ToLower().Contains(searchLower)) ||
                    (d.Course != null && d.Course.ToLower().Contains(searchLower)) ||
                    (d.CharacterCode != null && d.CharacterCode.ToLower().Contains(searchLower)));
            }

            if (!string.IsNullOrWhiteSpace(searchDto.Battalion))
            {
                query = query.Where(d => d.Battalion == searchDto.Battalion);
            }

            if (!string.IsNullOrWhiteSpace(searchDto.Course))
            {
                query = query.Where(d => d.Course == searchDto.Course);
            }

            if (!string.IsNullOrWhiteSpace(searchDto.CharacterCode))
            {
                query = query.Where(d => d.CharacterCode == searchDto.CharacterCode);
            }

            // Apply sorting
            if (!string.IsNullOrWhiteSpace(searchDto.SortBy))
            {
                var sortDirection = searchDto.SortDirection?.ToLower() == "desc" ? "desc" : "asc";

                query = searchDto.SortBy.ToLower() switch
                {
                    "departmentcode" => sortDirection == "desc" ?
                        query.OrderByDescending(d => d.DepartmentCode) :
                        query.OrderBy(d => d.DepartmentCode),
                    "battalion" => sortDirection == "desc" ?
                        query.OrderByDescending(d => d.Battalion) :
                        query.OrderBy(d => d.Battalion),
                    "course" => sortDirection == "desc" ?
                        query.OrderByDescending(d => d.Course) :
                        query.OrderBy(d => d.Course),
                    "charactercode" => sortDirection == "desc" ?
                        query.OrderByDescending(d => d.CharacterCode) :
                        query.OrderBy(d => d.CharacterCode),
                    _ => query.OrderBy(d => d.DepartmentCode)
                };
            }
            else
            {
                query = query.OrderBy(d => d.DepartmentCode);
            }

            var totalCount = await query.CountAsync();

            var departments = await query
                .Skip((searchDto.Page - 1) * searchDto.PageSize)
                .Take(searchDto.PageSize)
                .Select(d => new DepartmentInfoDto
                {
                    DepartmentCode = d.DepartmentCode,
                    Battalion = d.Battalion,
                    Course = d.Course,
                    CharacterCode = d.CharacterCode
                })
                .ToListAsync();

            return (departments, totalCount);
        }

        public async Task<DepartmentInfoDto?> GetDepartmentByCodeAsync(string departmentCode)
        {
            var department = await _context.Departments
                .Where(d => d.DepartmentCode == departmentCode)
                .Select(d => new DepartmentInfoDto
                {
                    DepartmentCode = d.DepartmentCode,
                    Battalion = d.Battalion,
                    Course = d.Course,
                    CharacterCode = d.CharacterCode
                })
                .FirstOrDefaultAsync();

            return department;
        }

        public async Task<(List<DepartmentSummaryDto> Departments, int TotalCount)> GetDepartmentsWithStudentCountAsync(
            int pageNumber = 1,
            int pageSize = 20,
            string? searchTerm = null)
        {
            var query = from dept in _context.Departments
                        join student in _context.Students on dept.DepartmentCode equals student.Department into studentGroup
                        select new
                        {
                            Department = dept,
                            StudentCount = studentGroup.Count()
                        };

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var searchLower = searchTerm.ToLower();
                query = query.Where(x =>
                    x.Department.DepartmentCode.ToLower().Contains(searchLower) ||
                    (x.Department.Battalion != null && x.Department.Battalion.ToLower().Contains(searchLower)) ||
                    (x.Department.Course != null && x.Department.Course.ToLower().Contains(searchLower)) ||
                    (x.Department.CharacterCode != null && x.Department.CharacterCode.ToLower().Contains(searchLower)));
            }

            var totalCount = await query.CountAsync();

            var departments = await query
                .OrderBy(x => x.Department.DepartmentCode)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new DepartmentSummaryDto
                {
                    DepartmentCode = x.Department.DepartmentCode,
                    Battalion = x.Department.Battalion,
                    Course = x.Department.Course,
                    CharacterCode = x.Department.CharacterCode,
                    StudentCount = x.StudentCount
                })
                .ToListAsync();

            return (departments, totalCount);
        }

        public async Task<DepartmentStatisticsDto> GetDepartmentStatisticsAsync()
        {
            var totalDepartments = await _context.Departments.CountAsync();
            var totalStudents = await _context.Students.CountAsync();

            var departmentStats = await _context.Students
                .Where(s => !string.IsNullOrEmpty(s.Department))
                .GroupBy(s => s.Department)
                .Select(g => new { Department = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Department!, x => x.Count);

            var battalionStats = await _context.Students
                .Join(_context.Departments, s => s.Department, d => d.DepartmentCode, (s, d) => d)
                .Where(d => !string.IsNullOrEmpty(d.Battalion))
                .GroupBy(d => d.Battalion)
                .Select(g => new { Battalion = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Battalion!, x => x.Count);

            var courseStats = await _context.Students
                .Join(_context.Departments, s => s.Department, d => d.DepartmentCode, (s, d) => d)
                .Where(d => !string.IsNullOrEmpty(d.Course))
                .GroupBy(d => d.Course)
                .Select(g => new { Course = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Course!, x => x.Count);

            return new DepartmentStatisticsDto
            {
                TotalDepartments = totalDepartments,
                TotalStudents = totalStudents,
                DepartmentsWithStudents = departmentStats.Count,
                DepartmentsWithoutStudents = totalDepartments - departmentStats.Count,
                StudentsByDepartment = departmentStats,
                StudentsByBattalion = battalionStats,
                StudentsByCourse = courseStats
            };
        }

        public async Task<List<DepartmentInfoDto>> GetAllDepartmentsSimpleAsync()
        {
            return await _context.Departments
                .OrderBy(d => d.DepartmentCode)
                .Select(d => new DepartmentInfoDto
                {
                    DepartmentCode = d.DepartmentCode,
                    Battalion = d.Battalion,
                    Course = d.Course,
                    CharacterCode = d.CharacterCode
                })
                .ToListAsync();
        }

        public async Task<List<DepartmentInfoDto>> GetDepartmentsByBattalionAsync(string battalion)
        {
            return await _context.Departments
                .Where(d => d.Battalion == battalion)
                .OrderBy(d => d.DepartmentCode)
                .Select(d => new DepartmentInfoDto
                {
                    DepartmentCode = d.DepartmentCode,
                    Battalion = d.Battalion,
                    Course = d.Course,
                    CharacterCode = d.CharacterCode
                })
                .ToListAsync();
        }

        public async Task<List<DepartmentInfoDto>> GetDepartmentsByCourseAsync(string course)
        {
            return await _context.Departments
                .Where(d => d.Course == course)
                .OrderBy(d => d.DepartmentCode)
                .Select(d => new DepartmentInfoDto
                {
                    DepartmentCode = d.DepartmentCode,
                    Battalion = d.Battalion,
                    Course = d.Course,
                    CharacterCode = d.CharacterCode
                })
                .ToListAsync();
        }

        public async Task<List<string>> GetBattalionsAsync()
        {
            return await _context.Departments
                .Where(d => !string.IsNullOrEmpty(d.Battalion))
                .Select(d => d.Battalion!)
                .Distinct()
                .OrderBy(b => b)
                .ToListAsync();
        }

        public async Task<List<string>> GetCoursesAsync()
        {
            return await _context.Departments
                .Where(d => !string.IsNullOrEmpty(d.Course))
                .Select(d => d.Course!)
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync();
        }

        public async Task<List<string>> GetCharacterCodesAsync()
        {
            return await _context.Departments
                .Where(d => !string.IsNullOrEmpty(d.CharacterCode))
                .Select(d => d.CharacterCode!)
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync();
        }
    }
}
