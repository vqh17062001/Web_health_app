using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Text.RegularExpressions;
using Web_health_app.ApiService.Entities;
using Web_health_app.Models.Models;

namespace Web_health_app.ApiService.Repository
{
    public class StudentRepository : IStudentRepository
    {
        private readonly HealthDbContext _context;

        public StudentRepository(HealthDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<(List<StudentInfoDto> Students, int TotalCount)> GetAllStudentsAsync(int pageNumber = 1, int pageSize = 10, string? searchTerm = null, bool includeInactive = false, string? managerId = "")
        {
            try
            {
                var query = _context.Students.AsQueryable();

                // Filter by active status
                if (!includeInactive)
                {
                    query = query.Where(s => s.Status >= 0);
                }
                if (managerId != "")
                {
                    query = query.Where(s => s.ManageBy.ToString() == managerId);
                }

                // Apply search filter
                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    query = query.Where(s =>
                        s.StudentId.Contains(searchTerm) ||
                        s.Name.Contains(searchTerm) ||
                        (s.Email != null && s.Email.Contains(searchTerm)) ||
                        (s.Phone != null && s.Phone.Contains(searchTerm)) ||
                        (s.Department != null && s.Department.Contains(searchTerm)));
                }

                var totalCount = await query.CountAsync();

                var students = await query
                    .Include(s => s.CreatedByNavigation)
                    .Include(s => s.ManageByNavigation)
                    .OrderBy(s => s.StudentId)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .Select(s => new StudentInfoDto
                    {
                        StudentId = s.StudentId,
                        Name = s.Name,
                        Dob = s.Dob,
                        Gender = s.Gender,
                        Phone = s.Phone,
                        Email = s.Email,
                        Status = s.Status,
                        StatusString = s.GetStudentStatusString(),
                        CreatedAt = s.CreatedAt,
                        UpdateAt = s.UpdateAt,
                        CreatedBy = s.CreatedBy,
                        CreatedByName = s.CreatedByNavigation != null ? s.CreatedByNavigation.FullName : null,
                        ManageBy = s.ManageBy,
                        ManageByName = s.ManageByNavigation != null ? s.ManageByNavigation.FullName : null,
                        Department = s.Department,
                        BodyMetricsCount = s.BodyMetrics.Count,
                        PhysiologicalMetricsCount = s.PhysiologicalMetrics.Count,
                        DailyActivitiesCount = s.DailyActivities.Count,
                        SleepSessionsCount = s.SleepSessions.Count,
                        ExercisesCount = s.Exercises.Count,
                        AssessmentBatchCount = s.AssessmentBatchStudents.Count
                    })
                    .ToListAsync();

                return (students, totalCount);
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving students", ex);
            }
        }

        public async Task<(List<StudentInfoDto> Students, int TotalCount)> SearchStudentsAsync(StudentSearchDto searchDto)
        {
            try
            {
                var query = _context.Students.AsQueryable();

                // Apply filters
                if (!string.IsNullOrWhiteSpace(searchDto.SearchTerm))
                {
                    query = query.Where(s =>
                        s.StudentId.Contains(searchDto.SearchTerm) ||
                        s.Name.Contains(searchDto.SearchTerm) ||
                        (s.Email != null && s.Email.Contains(searchDto.SearchTerm)) ||
                        (s.Phone != null && s.Phone.Contains(searchDto.SearchTerm)));
                }

                if (!string.IsNullOrWhiteSpace(searchDto.Department))
                {
                    query = query.Where(s => s.Department == searchDto.Department);
                }

                if (searchDto.Status.HasValue)
                {
                    query = query.Where(s => s.Status == searchDto.Status.Value);
                }

                if (!string.IsNullOrWhiteSpace(searchDto.Gender))
                {
                    query = query.Where(s => s.Gender == searchDto.Gender);
                }

                if (searchDto.ManageBy.HasValue)
                {
                    query = query.Where(s => s.ManageBy == searchDto.ManageBy.Value);
                }

                if (searchDto.CreatedFrom.HasValue)
                {
                    query = query.Where(s => s.CreatedAt >= searchDto.CreatedFrom.Value);
                }

                if (searchDto.CreatedTo.HasValue)
                {
                    query = query.Where(s => s.CreatedAt <= searchDto.CreatedTo.Value);
                }

                // Filter theo khoảng Dob - sử dụng so sánh chuỗi với format yyyy-MM-dd
                if (!string.IsNullOrWhiteSpace(searchDto.DobFrom))
                {
                    // Validate và format lại string để đảm bảo format đúng
                    if (DateTime.TryParseExact(searchDto.DobFrom, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var fromDate))
                    {
                        var fromStr = fromDate.ToString("yyyy-MM-dd");
                        query = query.Where(s => s.Dob != null && string.Compare(s.Dob, fromStr) >= 0);
                    }
                }

                if (!string.IsNullOrWhiteSpace(searchDto.DobTo))
                {
                    // Validate và format lại string để đảm bảo format đúng
                    if (DateTime.TryParseExact(searchDto.DobTo, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var toDate))
                    {
                        var toStr = toDate.ToString("yyyy-MM-dd");
                        query = query.Where(s => s.Dob != null && string.Compare(s.Dob, toStr) <= 0);
                    }
                }


                var totalCount = await query.CountAsync();

                var students = await query
                    .Include(s => s.CreatedByNavigation)
                    .Include(s => s.ManageByNavigation)
                    .OrderBy(s => s.StudentId)
                    .Skip((searchDto.Page - 1) * searchDto.PageSize)
                    .Take(searchDto.PageSize)
                    .ToListAsync();

                var studentDtos = students.Select(s => new StudentInfoDto
                {
                    StudentId = s.StudentId,
                    Name = s.Name,
                    Dob = s.Dob,
                    Gender = s.Gender,
                    Phone = s.Phone,
                    Email = s.Email,
                    Status = s.Status,
                    StatusString = s.GetStudentStatusString(),
                    CreatedAt = s.CreatedAt,
                    UpdateAt = s.UpdateAt,
                    CreatedBy = s.CreatedBy,
                    CreatedByName = s.CreatedByNavigation != null ? s.CreatedByNavigation.FullName : null,
                    ManageBy = s.ManageBy,
                    ManageByName = s.ManageByNavigation != null ? s.ManageByNavigation.FullName : null,
                    Department = s.Department,
                    BodyMetricsCount = s.BodyMetrics.Count,
                    PhysiologicalMetricsCount = s.PhysiologicalMetrics.Count,
                    DailyActivitiesCount = s.DailyActivities.Count,
                    SleepSessionsCount = s.SleepSessions.Count,
                    ExercisesCount = s.Exercises.Count,
                    AssessmentBatchCount = s.AssessmentBatchStudents.Count
                }).ToList();

                return (studentDtos, totalCount);
            }
            catch (Exception ex)
            {
                throw new Exception("Error searching students", ex);
            }
        }

        public async Task<StudentInfoDto?> GetStudentByIdAsync(string studentId)
        {
            try
            {
                var student = await _context.Students
                    .Include(s => s.CreatedByNavigation)
                    .Include(s => s.ManageByNavigation)
                    .FirstOrDefaultAsync(s => s.StudentId == studentId);

                if (student == null)
                    return null;

                return new StudentInfoDto
                {
                    StudentId = student.StudentId,
                    Name = student.Name,
                    Dob = student.Dob,
                    Gender = student.Gender,
                    Phone = student.Phone,
                    Email = student.Email,
                    Status = student.Status,
                    StatusString = student.GetStudentStatusString(),
                    CreatedAt = student.CreatedAt,
                    UpdateAt = student.UpdateAt,
                    CreatedBy = student.CreatedBy,
                    CreatedByName = student.CreatedByNavigation?.FullName,
                    ManageBy = student.ManageBy,
                    ManageByName = student.ManageByNavigation?.FullName,
                    Department = student.Department,
                    BodyMetricsCount = await _context.BodyMetrics.CountAsync(bm => bm.StudentId == studentId),
                    PhysiologicalMetricsCount = await _context.PhysiologicalMetrics.CountAsync(pm => pm.StudentId == studentId),
                    DailyActivitiesCount = await _context.DailyActivities.CountAsync(da => da.StudentId == studentId),
                    SleepSessionsCount = await _context.SleepSessions.CountAsync(ss => ss.StudentId == studentId),
                    ExercisesCount = await _context.Exercises.CountAsync(e => e.StudentId == studentId),
                    AssessmentBatchCount = await _context.AssessmentBatchStudents.CountAsync(abs => abs.StudentId == studentId)
                };
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving student with ID {studentId}", ex);
            }
        }

        public async Task<StudentDetailDto?> GetStudentDetailAsync(string studentId)
        {
            try
            {
                var student = await _context.Students
                    .Include(s => s.CreatedByNavigation)
                    .Include(s => s.ManageByNavigation)
                    .FirstOrDefaultAsync(s => s.StudentId == studentId);

                if (student == null)
                    return null;

                return new StudentDetailDto
                {
                    StudentId = student.StudentId,
                    Name = student.Name,
                    Dob = student.Dob,
                    Gender = student.Gender,
                    Phone = student.Phone,
                    Email = student.Email,
                    Status = student.Status,
                    StatusString = student.GetStudentStatusString(),
                    CreatedAt = student.CreatedAt,
                    UpdateAt = student.UpdateAt,
                    CreatedBy = student.CreatedBy,
                    CreatedByName = student.CreatedByNavigation?.FullName,
                    ManageBy = student.ManageBy,
                    ManageByName = student.ManageByNavigation?.FullName,
                    Department = student.Department,
                    BodyMetricsCount = await _context.BodyMetrics.CountAsync(bm => bm.StudentId == studentId),
                    PhysiologicalMetricsCount = await _context.PhysiologicalMetrics.CountAsync(pm => pm.StudentId == studentId),
                    DailyActivitiesCount = await _context.DailyActivities.CountAsync(da => da.StudentId == studentId),
                    SleepSessionsCount = await _context.SleepSessions.CountAsync(ss => ss.StudentId == studentId),
                    ExercisesCount = await _context.Exercises.CountAsync(e => e.StudentId == studentId),
                    AssessmentBatchCount = await _context.AssessmentBatchStudents.CountAsync(abs => abs.StudentId == studentId),
                    // For now, we'll initialize empty lists - these can be populated later based on actual entity structures
                    BodyMetrics = new List<BodyMetricDto>(),
                    PhysiologicalMetrics = new List<PhysiologicalMetricDto>(),
                    DailyActivities = new List<DailyActivityDto>(),
                    SleepSessions = new List<SleepSessionDto>(),
                    Exercises = new List<ExerciseDto>(),
                    AssessmentBatches = new List<AssessmentBatchStudentDto>()
                };
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving student detail with ID {studentId}", ex);
            }
        }

        public async Task<StudentInfoDto> CreateStudentAsync(CreateStudentDto createStudentDto)
        {
            try
            {
                // Generate new student ID
                var studentId = await GenerateStudentIdAsync();

                // Check if student ID already exists (just in case)
                if (await StudentIdExistsAsync(studentId))
                {
                    throw new InvalidOperationException("Generated student ID already exists");
                }

                var student = new Student
                {
                    StudentId = studentId,
                    Name = createStudentDto.Name,
                    Dob = createStudentDto.Dob,
                    Gender = createStudentDto.Gender,
                    Phone = createStudentDto.Phone,
                    Email = createStudentDto.Email,
                    Department = createStudentDto.Department,
                    CreatedBy = createStudentDto.CreatedBy,
                    ManageBy = createStudentDto.ManageBy,
                    Status = createStudentDto.Status,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Students.Add(student);
                await _context.SaveChangesAsync();

                return await GetStudentByIdAsync(studentId) ?? throw new InvalidOperationException("Failed to retrieve created student");
            }
            catch (Exception ex)
            {
                throw new Exception("Error creating student", ex);
            }
        }

        public async Task<StudentInfoDto?> UpdateStudentAsync(string studentId, UpdateStudentDto updateStudentDto)
        {
            try
            {
                var student = await _context.Students.FirstOrDefaultAsync(s => s.StudentId == studentId);
                if (student == null)
                    return null;

                // Update properties only if they are provided
                if (!string.IsNullOrEmpty(updateStudentDto.Name))
                    student.Name = updateStudentDto.Name;

                if (updateStudentDto.Dob != null)
                    student.Dob = updateStudentDto.Dob;

                if (updateStudentDto.Gender != null)
                    student.Gender = updateStudentDto.Gender;

                if (updateStudentDto.Phone != null)
                    student.Phone = updateStudentDto.Phone;

                if (updateStudentDto.Email != null)
                    student.Email = updateStudentDto.Email;

                if (updateStudentDto.Department != null)
                    student.Department = updateStudentDto.Department;

                if (updateStudentDto.ManageBy.HasValue)
                    student.ManageBy = updateStudentDto.ManageBy.Value;

                if (updateStudentDto.Status.HasValue)
                    student.Status = updateStudentDto.Status.Value;

                student.UpdateAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return await GetStudentByIdAsync(studentId);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error updating student with ID {studentId}", ex);
            }
        }

        public async Task<bool> DeleteStudentAsync(string studentId)
        {
            try
            {
                var student = await _context.Students.FirstOrDefaultAsync(s => s.StudentId == studentId);
                if (student == null)
                    return false;

                // Soft delete by setting status to -1
                student.Status = -1;
                student.UpdateAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error deleting student with ID {studentId}", ex);
            }
        }

        public async Task<bool> HardDeleteStudentAsync(string studentId)
        {
            try
            {
                var student = await _context.Students.FirstOrDefaultAsync(s => s.StudentId == studentId);
                if (student == null)
                    return false;

                _context.Students.Remove(student);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error permanently deleting student with ID {studentId}", ex);
            }
        }

        public async Task<bool> StudentIdExistsAsync(string studentId, string? excludeStudentId = null)
        {
            try
            {
                var query = _context.Students.Where(s => s.StudentId == studentId);

                if (!string.IsNullOrEmpty(excludeStudentId))
                {
                    query = query.Where(s => s.StudentId != excludeStudentId);
                }

                return await query.AnyAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error checking if student ID {studentId} exists", ex);
            }
        }

        public async Task<List<StudentInfoDto>> GetStudentsByDepartmentAsync(string department)
        {
            try
            {
                var students = await _context.Students
                    .Where(s => s.Department == department && s.Status >= 0)
                    .Include(s => s.CreatedByNavigation)
                    .Include(s => s.ManageByNavigation)
                    .Select(s => new StudentInfoDto
                    {
                        StudentId = s.StudentId,
                        Name = s.Name,
                        Dob = s.Dob,
                        Gender = s.Gender,
                        Phone = s.Phone,
                        Email = s.Email,
                        Status = s.Status,
                        StatusString = s.GetStudentStatusString(),
                        CreatedAt = s.CreatedAt,
                        UpdateAt = s.UpdateAt,
                        CreatedBy = s.CreatedBy,
                        CreatedByName = s.CreatedByNavigation != null ? s.CreatedByNavigation.FullName : null,
                        ManageBy = s.ManageBy,
                        ManageByName = s.ManageByNavigation != null ? s.ManageByNavigation.FullName : null,
                        Department = s.Department
                    })
                    .ToListAsync();

                return students;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving students by department {department}", ex);
            }
        }

        public async Task<List<StudentInfoDto>> GetStudentsByManagerAsync(Guid managerId)
        {
            try
            {
                var students = await _context.Students
                    .Where(s => s.ManageBy == managerId && s.Status >= 0)
                    .Include(s => s.CreatedByNavigation)
                    .Include(s => s.ManageByNavigation)
                    .Select(s => new StudentInfoDto
                    {
                        StudentId = s.StudentId,
                        Name = s.Name,
                        Dob = s.Dob,
                        Gender = s.Gender,
                        Phone = s.Phone,
                        Email = s.Email,
                        Status = s.Status,
                        StatusString = s.GetStudentStatusString(),
                        CreatedAt = s.CreatedAt,
                        UpdateAt = s.UpdateAt,
                        CreatedBy = s.CreatedBy,
                        CreatedByName = s.CreatedByNavigation != null ? s.CreatedByNavigation.FullName : null,
                        ManageBy = s.ManageBy,
                        ManageByName = s.ManageByNavigation != null ? s.ManageByNavigation.FullName : null,
                        Department = s.Department
                    })
                    .ToListAsync();

                return students;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving students by manager {managerId}", ex);
            }
        }

        public async Task<List<StudentInfoDto>> GetStudentsByStatusAsync(short status)
        {
            try
            {
                var students = await _context.Students
                    .Where(s => s.Status == status)
                    .Include(s => s.CreatedByNavigation)
                    .Include(s => s.ManageByNavigation)
                    .Select(s => new StudentInfoDto
                    {
                        StudentId = s.StudentId,
                        Name = s.Name,
                        Dob = s.Dob,
                        Gender = s.Gender,
                        Phone = s.Phone,
                        Email = s.Email,
                        Status = s.Status,
                        StatusString = s.GetStudentStatusString(),
                        CreatedAt = s.CreatedAt,
                        UpdateAt = s.UpdateAt,
                        CreatedBy = s.CreatedBy,
                        CreatedByName = s.CreatedByNavigation != null ? s.CreatedByNavigation.FullName : null,
                        ManageBy = s.ManageBy,
                        ManageByName = s.ManageByNavigation != null ? s.ManageByNavigation.FullName : null,
                        Department = s.Department
                    })
                    .ToListAsync();

                return students;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving students by status {status}", ex);
            }
        }

        public async Task<StudentStatisticsDto> GetStudentStatisticsAsync()
        {
            try
            {
                var totalStudents = await _context.Students.CountAsync();
                var activeStudents = await _context.Students.CountAsync(s => s.Status >= 0);
                var inactiveStudents = await _context.Students.CountAsync(s => s.Status < 0);
                var studentsWithSyncData = await _context.Students.CountAsync(s => s.Status == 1 || s.Status == 10 || s.Status == 11);
                var studentsOffline = await _context.Students.CountAsync(s => s.Status == 10);
                var studentsOnline = await _context.Students.CountAsync(s => s.Status == 11);

                var studentsByDepartment = await _context.Students
                    .Where(s => s.Status >= 0 && !string.IsNullOrEmpty(s.Department))
                    .GroupBy(s => s.Department)
                    .Select(g => new { Department = g.Key, Count = g.Count() })
                    .ToDictionaryAsync(x => x.Department!, x => x.Count);

                var studentsByGender = await _context.Students
                    .Where(s => s.Status >= 0 && !string.IsNullOrEmpty(s.Gender))
                    .GroupBy(s => s.Gender)
                    .Select(g => new { Gender = g.Key, Count = g.Count() })
                    .ToDictionaryAsync(x => x.Gender!, x => x.Count);

                return new StudentStatisticsDto
                {
                    TotalStudents = totalStudents,
                    ActiveStudents = activeStudents,
                    InactiveStudents = inactiveStudents,
                    StudentsWithSyncData = studentsWithSyncData,
                    StudentsOffline = studentsOffline,
                    StudentsOnline = studentsOnline,
                    StudentsByDepartment = studentsByDepartment,
                    StudentsByGender = studentsByGender
                };
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving student statistics", ex);
            }
        }

        public async Task<List<StudentInfoDto>> BulkCreateStudentsAsync(List<CreateStudentDto> createStudentDtos)
        {
            try
            {
                var students = new List<Student>();

                foreach (var dto in createStudentDtos)
                {
                    var studentId = await GenerateStudentIdAsync();

                    students.Add(new Student
                    {
                        StudentId = studentId,
                        Name = dto.Name,
                        Dob = dto.Dob,
                        Gender = dto.Gender,
                        Phone = dto.Phone,
                        Email = dto.Email,
                        Department = dto.Department,
                        ManageBy = dto.ManageBy,
                        Status = dto.Status,
                        CreatedAt = DateTime.UtcNow
                    });
                }

                _context.Students.AddRange(students);
                await _context.SaveChangesAsync();

                var studentIds = students.Select(s => s.StudentId).ToList();
                var createdStudents = await _context.Students
                    .Where(s => studentIds.Contains(s.StudentId))
                    .Include(s => s.CreatedByNavigation)
                    .Include(s => s.ManageByNavigation)
                    .Select(s => new StudentInfoDto
                    {
                        StudentId = s.StudentId,
                        Name = s.Name,
                        Dob = s.Dob,
                        Gender = s.Gender,
                        Phone = s.Phone,
                        Email = s.Email,
                        Status = s.Status,
                        StatusString = s.GetStudentStatusString(),
                        CreatedAt = s.CreatedAt,
                        UpdateAt = s.UpdateAt,
                        CreatedBy = s.CreatedBy,
                        CreatedByName = s.CreatedByNavigation != null ? s.CreatedByNavigation.FullName : null,
                        ManageBy = s.ManageBy,
                        ManageByName = s.ManageByNavigation != null ? s.ManageByNavigation.FullName : null,
                        Department = s.Department
                    })
                    .ToListAsync();

                return createdStudents;
            }
            catch (Exception ex)
            {
                throw new Exception("Error bulk creating students", ex);
            }
        }

        public async Task<int> BulkUpdateStudentsAsync(Dictionary<string, UpdateStudentDto> updates)
        {
            try
            {
                var studentIds = updates.Keys.ToList();
                var students = await _context.Students
                    .Where(s => studentIds.Contains(s.StudentId))
                    .ToListAsync();

                int updatedCount = 0;

                foreach (var student in students)
                {
                    if (updates.TryGetValue(student.StudentId, out var updateDto))
                    {
                        if (!string.IsNullOrEmpty(updateDto.Name))
                            student.Name = updateDto.Name;

                        if (updateDto.Dob != null)
                            student.Dob = updateDto.Dob;

                        if (updateDto.Gender != null)
                            student.Gender = updateDto.Gender;

                        if (updateDto.Phone != null)
                            student.Phone = updateDto.Phone;

                        if (updateDto.Email != null)
                            student.Email = updateDto.Email;

                        if (updateDto.Department != null)
                            student.Department = updateDto.Department;

                        if (updateDto.ManageBy.HasValue)
                            student.ManageBy = updateDto.ManageBy.Value;

                        if (updateDto.Status.HasValue)
                            student.Status = updateDto.Status.Value;

                        student.UpdateAt = DateTime.UtcNow;
                        updatedCount++;
                    }
                }

                await _context.SaveChangesAsync();
                return updatedCount;
            }
            catch (Exception ex)
            {
                throw new Exception("Error bulk updating students", ex);
            }
        }

        public async Task<bool> AssignManagerToStudentsAsync(List<string> studentIds, Guid managerId)
        {
            try
            {
                var students = await _context.Students
                    .Where(s => studentIds.Contains(s.StudentId))
                    .ToListAsync();

                if (!students.Any())
                    return false;

                // Verify manager exists
                var managerExists = await _context.Users.AnyAsync(u => u.UserId == managerId);
                if (!managerExists)
                    return false;

                foreach (var student in students)
                {
                    student.ManageBy = managerId;
                    student.UpdateAt = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception("Error assigning manager to students", ex);
            }
        }

        #region Private Helper Methods

        //private static string GetStatusString(short status)
        //{
        //    return status switch
        //    {
        //        1 => "Có dữ liệu đồng bộ với Atlas",
        //        10 => "Offline",
        //        11 => "Online",
        //        -1 => "Do user tạo ra",
        //        _ => "Không xác định"
        //    };
        //}

        private async Task<string> GenerateStudentIdAsync()
        {
            var prefix = "STU";
            var year = DateTime.Now.Year.ToString();

            // Get the last student ID for current year
            var lastStudent = await _context.Students
                .Where(s => s.StudentId.StartsWith(prefix + year))
                .OrderByDescending(s => s.StudentId)
                .FirstOrDefaultAsync();

            int nextNumber = 1;

            if (lastStudent != null)
            {
                var numberPart = lastStudent.StudentId.Substring((prefix + year).Length);
                if (int.TryParse(numberPart, out int lastNumber))
                {
                    nextNumber = lastNumber + 1;
                }
            }

            return $"{prefix}{year}{nextNumber:D4}"; // Format: STU20240001
        }

        #endregion
    }
}
