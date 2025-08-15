using Microsoft.EntityFrameworkCore;

using Web_health_app.ApiService.Entities;
using Web_health_app.Models.Models;

namespace Web_health_app.ApiService.Repository
{
    public class AssessmentBatchRepository : IAssessmentBatchRepository
    {
        private readonly HealthDbContext _context;

        public AssessmentBatchRepository(HealthDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<(List<AssessmentBatchInfoDto> AssessmentBatches, int TotalCount)> GetAllAssessmentBatchesAsync(int pageNumber = 1, int pageSize = 10, string? searchTerm = null, bool includeInactive = false)
        {
            try
            {
                var query = _context.AssessmentBatches.AsQueryable();

                // Filter by active status
                if (!includeInactive)
                {
                    query = query.Where(ab => ab.Status >= 0);
                }

                // Apply search filter
                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    query = query.Where(ab =>
                        ab.BatchId.Contains(searchTerm) ||
                        (ab.CodeName != null && ab.CodeName.Contains(searchTerm)) ||
                        (ab.Description != null && ab.Description.Contains(searchTerm)));
                }

                var totalCount = await query.CountAsync();

                var assessmentBatches = await query
                    .Include(ab => ab.AssessmentBatchStudents)
                    .OrderBy(ab => ab.BatchId)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .Select(ab => new AssessmentBatchInfoDto
                    {
                        AssessmentBatchId = ab.BatchId,
                        CodeName = ab.CodeName ?? "",
                        Description = ab.Description,
                        StartDate = ab.ScheduledAt ?? DateTime.MinValue,
                        EndDate = ab.ScheduledAt ?? DateTime.MinValue,
                        Status = ab.Status,
                        StatusString = ab.GetAssessmentBatchStatusString(),
                        CreatedAt = ab.CreatedAt ?? DateTime.MinValue,
                        UpdatedAt = ab.UpdatedAt,
                        CreatedBy = ab.CreatedBy ?? Guid.Empty,
                        CreatedByName = null,
                        StudentCount = ab.AssessmentBatchStudents.Count(),
                        CompletedCount = 0,
                        PendingCount = ab.AssessmentBatchStudents.Count()
                    })
                    .ToListAsync();

                return (assessmentBatches, totalCount);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving assessment batches: {ex.Message}", ex);
            }
        }

        public async Task<(List<AssessmentBatchInfoDto> AssessmentBatches, int TotalCount)> SearchAssessmentBatchesAsync(AssessmentBatchSearchDto searchDto)
        {
            try
            {
                var query = _context.AssessmentBatches.AsQueryable();

                // Apply filters
                if (!string.IsNullOrWhiteSpace(searchDto.SearchTerm))
                {
                    query = query.Where(ab =>
                        ab.BatchId.Contains(searchDto.SearchTerm) ||
                        (ab.CodeName != null && ab.CodeName.Contains(searchDto.SearchTerm)) ||
                        (ab.Description != null && ab.Description.Contains(searchDto.SearchTerm)));
                }

                if (searchDto.Status.HasValue)
                {
                    query = query.Where(ab => ab.Status == searchDto.Status.Value);
                }
                if (searchDto.ManagerBy.HasValue)
                {
                    query = query.Where(ab => ab.ManagerBy == searchDto.ManagerBy.Value);
                }

                if (searchDto.CreatedBy.HasValue)
                {
                    query = query.Where(ab => ab.CreatedBy == searchDto.CreatedBy.Value);
                }

                if (searchDto.StartDateFrom.HasValue)
                {
                    query = query.Where(ab => ab.ScheduledAt >= searchDto.StartDateFrom.Value);
                }

                if (searchDto.StartDateTo.HasValue)
                {
                    query = query.Where(ab => ab.ScheduledAt <= searchDto.StartDateTo.Value);
                }

                if (searchDto.CreatedFrom.HasValue)
                {
                    query = query.Where(ab => ab.CreatedAt >= searchDto.CreatedFrom.Value);
                }

                if (searchDto.CreatedTo.HasValue)
                {
                    query = query.Where(ab => ab.CreatedAt <= searchDto.CreatedTo.Value);
                }

                // Apply sorting
                if (!string.IsNullOrWhiteSpace(searchDto.SortBy))
                {
                    var isDescending = searchDto.SortDirection?.ToLower() == "desc";
                    switch (searchDto.SortBy.ToLower())
                    {
                        case "assessmentbatchid":
                        case "batchid":
                            query = isDescending ? query.OrderByDescending(ab => ab.BatchId) : query.OrderBy(ab => ab.BatchId);
                            break;
                        case "batchname":
                        case "codename":
                            query = isDescending ? query.OrderByDescending(ab => ab.CodeName) : query.OrderBy(ab => ab.CodeName);
                            break;
                        case "scheduledat":
                        case "startdate":
                            query = isDescending ? query.OrderByDescending(ab => ab.ScheduledAt) : query.OrderBy(ab => ab.ScheduledAt);
                            break;
                        case "createdat":
                            query = isDescending ? query.OrderByDescending(ab => ab.CreatedAt) : query.OrderBy(ab => ab.CreatedAt);
                            break;
                        default:
                            query = query.OrderBy(ab => ab.BatchId);
                            break;
                    }
                }
                else
                {
                    query = query.OrderBy(ab => ab.BatchId);
                }

                var totalCount = await query.CountAsync();

                var assessmentBatches = await query
                    .Include(ab => ab.AssessmentBatchStudents)
                    .Skip((searchDto.Page - 1) * searchDto.PageSize)
                    .Take(searchDto.PageSize)
                    .Select(ab => new AssessmentBatchInfoDto
                    {
                        AssessmentBatchId = ab.BatchId,
                        CodeName = ab.CodeName ?? "",
                        Description = ab.Description,
                        StartDate = ab.ScheduledAt ?? DateTime.MinValue,
                        EndDate = ab.ScheduledAt ?? DateTime.MinValue,
                        Status = ab.Status,
                        StatusString = ab.GetAssessmentBatchStatusString(),
                        CreatedAt = ab.CreatedAt ?? DateTime.MinValue,
                        UpdatedAt = ab.UpdatedAt,
                        CreatedBy = ab.CreatedBy ?? Guid.Empty,
                        CreatedByName = null,
                        StudentCount = ab.AssessmentBatchStudents.Count(),
                        CompletedCount = CompletedCountAssessmentBatch(ab.BatchId),
                        PendingCount = ab.AssessmentBatchStudents.Count()
                    })
                    .ToListAsync();

                return (assessmentBatches, totalCount);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error searching assessment batches: {ex.Message}", ex);
            }
        }

        public async Task<AssessmentBatchInfoDto?> GetAssessmentBatchByIdAsync(string assessmentBatchId)
        {
            try
            {
                var assessmentBatch = await _context.AssessmentBatches
                    .Include(ab => ab.AssessmentBatchStudents)
                    .FirstOrDefaultAsync(ab => ab.BatchId == assessmentBatchId);

                if (assessmentBatch == null)
                    return null;

                return new AssessmentBatchInfoDto
                {
                    AssessmentBatchId = assessmentBatch.BatchId,
                    CodeName = assessmentBatch.CodeName ?? "",
                    Description = assessmentBatch.Description,
                    StartDate = assessmentBatch.ScheduledAt ?? DateTime.MinValue,
                    EndDate = assessmentBatch.ScheduledAt ?? DateTime.MinValue,
                    Status = assessmentBatch.Status,
                    StatusString = assessmentBatch.GetAssessmentBatchStatusString(),
                    CreatedAt = assessmentBatch.CreatedAt ?? DateTime.MinValue,
                    UpdatedAt = assessmentBatch.UpdatedAt,
                    ManagerBy = assessmentBatch.ManagerBy,
                    CreatedBy = assessmentBatch.CreatedBy ?? Guid.Empty,
                    CreatedByName = null,
                    StudentCount = assessmentBatch.AssessmentBatchStudents.Count(),
                    ///////
                    CompletedCount = CompletedCountAssessmentBatch(assessmentBatchId),
                    PendingCount = assessmentBatch.AssessmentBatchStudents.Count()
                };
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving assessment batch: {ex.Message}", ex);
            }
        }

        private int CompletedCountAssessmentBatch(string assessmentBatchId)
        {
            try
            {
                int count = 0;

                var countABS = _context.AssessmentBatchStudents.Where(x => x.BatchId == assessmentBatchId).ToList();


                foreach (var i in countABS)
                {

                    if (_context.AssessmentTests.Where(x => x.AbsId == i.AbsId).Count() >= 5)
                    {

                        count++;
                    }



                }

                return count;
            }
            catch (Exception ex) { 
            
                throw new Exception($"Error  detail: {ex.Message}", ex);


            }
        }

        public async Task<AssessmentBatchDetailDto?> GetAssessmentBatchDetailAsync(string assessmentBatchId)
        {
            try
            {
                var assessmentBatch = await _context.AssessmentBatches
                    .Include(ab => ab.AssessmentBatchStudents)
                        .ThenInclude(abs => abs.Student)
                    .FirstOrDefaultAsync(ab => ab.BatchId == assessmentBatchId);

                if (assessmentBatch == null)
                    return null;

                return new AssessmentBatchDetailDto
                {
                    AssessmentBatchId = assessmentBatch.BatchId,
                    CodeName = assessmentBatch.CodeName ?? "",
                    Description = assessmentBatch.Description,
                    StartDate = assessmentBatch.ScheduledAt ?? DateTime.MinValue,
                    EndDate = assessmentBatch.ScheduledAt ?? DateTime.MinValue,
                    Status = assessmentBatch.Status,
                    StatusString = assessmentBatch.GetAssessmentBatchStatusString(),
                    CreatedAt = assessmentBatch.CreatedAt ?? DateTime.MinValue,
                    UpdatedAt = assessmentBatch.UpdatedAt,
                    CreatedBy = assessmentBatch.CreatedBy ?? Guid.Empty,
                    CreatedByName = null,
                    StudentCount = assessmentBatch.AssessmentBatchStudents.Count(),
                    CompletedCount = CompletedCountAssessmentBatch(assessmentBatchId),
                    PendingCount = assessmentBatch.AssessmentBatchStudents.Count(),
                    Students = assessmentBatch.AssessmentBatchStudents.Select(abs => new AssessmentBatchStudentDto
                    {
                        AssessmentBatchId = abs.BatchId ?? "",
                        StudentId = abs.StudentId ?? "",
                        AssignedDate = DateTime.UtcNow
                    }).ToList(),
                    Tests = new List<AssessmentTestDto>()
                };
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving assessment batch detail: {ex.Message}", ex);
            }
        }

        public async Task<AssessmentBatchInfoDto?> CreateAssessmentBatchAsync(CreateAssessmentBatchDto createDto)
        {
            try
            {
                var assessmentBatchId = await GenerateAssessmentBatchIdAsync(_context, createDto.CodeName);

                var assessmentBatch = new AssessmentBatch
                {
                    BatchId = assessmentBatchId,
                    CodeName = createDto.CodeName,
                    Description = createDto.Description,
                    ScheduledAt = createDto.StartDate,
                    Status = createDto.Status,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = createDto.CreatedBy,
                    ManagerBy = createDto.ManagerBy

                };

                _context.AssessmentBatches.Add(assessmentBatch);
                await _context.SaveChangesAsync();

                return await GetAssessmentBatchByIdAsync(assessmentBatchId);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error creating assessment batch: {ex.Message}", ex);
            }
        }

        public async Task<AssessmentBatchInfoDto?> UpdateAssessmentBatchAsync(string assessmentBatchId, UpdateAssessmentBatchDto updateDto)
        {
            try
            {
                var assessmentBatch = await _context.AssessmentBatches
                    .FirstOrDefaultAsync(ab => ab.BatchId == assessmentBatchId);

                if (assessmentBatch == null)
                    return null;

                // Update only provided fields
                if (!string.IsNullOrWhiteSpace(updateDto.CodeName))
                    assessmentBatch.CodeName = updateDto.CodeName;

                if (updateDto.Description != null)
                    assessmentBatch.Description = updateDto.Description;

                if (updateDto.StartDate.HasValue)
                    assessmentBatch.ScheduledAt = updateDto.StartDate.Value;

                if (updateDto.Status.HasValue)
                    assessmentBatch.Status = updateDto.Status.Value;

                assessmentBatch.ManagerBy = updateDto.ManagerBy;
                
                
                

                assessmentBatch.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return await GetAssessmentBatchByIdAsync(assessmentBatchId);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error updating assessment batch: {ex.Message}", ex);
            }
        }

        public async Task<bool> DeleteAssessmentBatchAsync(string assessmentBatchId)
        {
            try
            {
                var assessmentBatch = await _context.AssessmentBatches
                    .FirstOrDefaultAsync(ab => ab.BatchId == assessmentBatchId);

                if (assessmentBatch == null)
                    return false;

                _context.AssessmentBatches.Remove(assessmentBatch);
                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error deleting assessment batch: {ex.Message}", ex);
            }
        }

        public async Task<bool> SoftDeleteAssessmentBatchAsync(string assessmentBatchId)
        {
            try
            {
                var assessmentBatch = await _context.AssessmentBatches
                    .FirstOrDefaultAsync(ab => ab.BatchId == assessmentBatchId);

                if (assessmentBatch == null)
                    return false;

                assessmentBatch.Status = -2; // Mark as deleted
                assessmentBatch.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error soft deleting assessment batch: {ex.Message}", ex);
            }
        }

        public async Task<bool> AssessmentBatchExistsAsync(string assessmentBatchId)
        {
            try
            {
                return await _context.AssessmentBatches
                    .AnyAsync(ab => ab.BatchId == assessmentBatchId);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error checking assessment batch existence: {ex.Message}", ex);
            }
        }

        public async Task<AssessmentBatchStatisticsDto> GetAssessmentBatchStatisticsAsync()
        {
            try
            {
                var totalBatches = await _context.AssessmentBatches.CountAsync();
                var pendingBatches = await _context.AssessmentBatches.CountAsync(ab => ab.Status == 0);

                var activeBatches = await _context.AssessmentBatches.CountAsync(ab => ab.Status == 1);
                var runningBatches = await _context.AssessmentBatches.CountAsync(ab => ab.Status == 2);

                var completedBatches = await _context.AssessmentBatches.CountAsync(ab => ab.Status == 3);
                var enrolledBatches = await _context.AssessmentBatches.CountAsync(ab => ab.Status == -1);


                var totalStudentsInBatches = await _context.AssessmentBatchStudents.CountAsync();

                return new AssessmentBatchStatisticsDto
                {
                    TotalBatches = totalBatches,
                    ActiveBatches = activeBatches,
                    CompletedBatches = completedBatches,
                    PendingBatches = pendingBatches,
                    TotalStudentsInBatches = totalStudentsInBatches,
                    TotalCompletedAssessments = 0,
                    LastUpdated = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving assessment batch statistics: {ex.Message}", ex);
            }
        }

        public async Task<int> AssignStudentsToAssessmentBatchAsync(string assessmentBatchId, List<string> studentIds)
        {
            try
            {
                var existingAssignments = await _context.AssessmentBatchStudents
                    .Where(abs => abs.BatchId == assessmentBatchId && studentIds.Contains(abs.StudentId))
                    .Select(abs => abs.StudentId)
                    .ToListAsync();

                var newStudentIds = studentIds.Except(existingAssignments).ToList();

                foreach (var studentId in newStudentIds)
                {
                    var assignment = new AssessmentBatchStudent
                    {
                        AbsId = GenerateAssessmentBatchStudentId(studentId, assessmentBatchId),
                        BatchId = assessmentBatchId,
                        StudentId = studentId
                    };

                    _context.AssessmentBatchStudents.Add(assignment);
                }

                await _context.SaveChangesAsync();

                return newStudentIds.Count;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error assigning students to assessment batch: {ex.Message}", ex);
            }
        }

        public async Task<int> RemoveStudentsFromAssessmentBatchAsync(string assessmentBatchId, List<string> studentIds)
        {
            try
            {
                var assignments = await _context.AssessmentBatchStudents
                    .Where(abs => abs.BatchId == assessmentBatchId && studentIds.Contains(abs.StudentId))
                    .ToListAsync();

                _context.AssessmentBatchStudents.RemoveRange(assignments);
                await _context.SaveChangesAsync();

                return assignments.Count;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error removing students from assessment batch: {ex.Message}", ex);
            }
        }

        public async Task<(List<StudentInfoDto> Students, int TotalCount)> GetStudentsInAssessmentBatchAsync(string assessmentBatchId, int pageNumber = 1, int pageSize = 10)
        {
            try
            {
                var query = _context.AssessmentBatchStudents
                    .Where(abs => abs.BatchId == assessmentBatchId)
                    .Include(abs => abs.Student);

                var totalCount = await query.CountAsync();

                var students = await query
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .Where(abs => abs.Student != null)
                    .Select(abs => new StudentInfoDto
                    {
                        StudentId = abs.Student!.StudentId,
                        Name = abs.Student.Name,
                        Dob = abs.Student.Dob,
                        Gender = abs.Student.Gender,
                        Phone = abs.Student.Phone,
                        Email = abs.Student.Email,
                        Status = abs.Student.Status,
                        StatusString = abs.Student.GetStudentStatusString(),
                        CreatedAt = abs.Student.CreatedAt,
                        UpdateAt = abs.Student.UpdateAt,
                        CreatedBy = abs.Student.CreatedBy,
                        ManageBy = abs.Student.ManageBy,
                        Department = abs.Student.Department
                    })
                    .ToListAsync();

                return (students, totalCount);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving students in assessment batch: {ex.Message}", ex);
            }
        }

        public async Task<(List<AssessmentBatchInfoDto> AssessmentBatches, int TotalCount)> GetAssessmentBatchesByCreatorAsync(Guid createdBy, int pageNumber = 1, int pageSize = 10)
        {
            try
            {
                var query = _context.AssessmentBatches
                    .Where(ab => ab.CreatedBy == createdBy);

                var totalCount = await query.CountAsync();

                var assessmentBatches = await query
                    .Include(ab => ab.AssessmentBatchStudents)
                    .OrderByDescending(ab => ab.CreatedAt)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .Select(ab => new AssessmentBatchInfoDto
                    {
                        AssessmentBatchId = ab.BatchId,
                        CodeName = ab.CodeName ?? "",
                        Description = ab.Description,
                        StartDate = ab.ScheduledAt ?? DateTime.MinValue,
                        EndDate = ab.ScheduledAt ?? DateTime.MinValue,
                        Status = ab.Status,
                        StatusString = ab.GetAssessmentBatchStatusString(),
                        CreatedAt = ab.CreatedAt ?? DateTime.MinValue,
                        UpdatedAt = ab.UpdatedAt,
                        CreatedBy = ab.CreatedBy ?? Guid.Empty,
                        CreatedByName = null,
                        StudentCount = ab.AssessmentBatchStudents.Count(),
                        CompletedCount = CompletedCountAssessmentBatch(ab.BatchId),
                        PendingCount = ab.AssessmentBatchStudents.Count()
                    })
                    .ToListAsync();

                return (assessmentBatches, totalCount);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving assessment batches by creator: {ex.Message}", ex);
            }
        }

        public async Task<(List<AssessmentBatchInfoDto> AssessmentBatches, int TotalCount)> GetActiveAssessmentBatchesAsync(int pageNumber = 1, int pageSize = 10)
        {
            try
            {
                var query = _context.AssessmentBatches
                    .Where(ab => ab.Status == 1); // Active status

                var totalCount = await query.CountAsync();

                var assessmentBatches = await query
                    .Include(ab => ab.AssessmentBatchStudents)
                    .OrderByDescending(ab => ab.CreatedAt)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .Select(ab => new AssessmentBatchInfoDto
                    {
                        AssessmentBatchId = ab.BatchId,
                        CodeName = ab.CodeName ?? "",
                        Description = ab.Description,
                        StartDate = ab.ScheduledAt ?? DateTime.MinValue,
                        EndDate = ab.ScheduledAt ?? DateTime.MinValue,
                        Status = ab.Status,
                        StatusString = ab.GetAssessmentBatchStatusString(),
                        CreatedAt = ab.CreatedAt ?? DateTime.MinValue,
                        UpdatedAt = ab.UpdatedAt,
                        CreatedBy = ab.CreatedBy ?? Guid.Empty,
                        CreatedByName = null,
                        StudentCount = ab.AssessmentBatchStudents.Count(),
                        CompletedCount = CompletedCountAssessmentBatch(ab.BatchId),
                        PendingCount = ab.AssessmentBatchStudents.Count()
                    })
                    .ToListAsync();

                return (assessmentBatches, totalCount);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving active assessment batches: {ex.Message}", ex);
            }
        }

        public async Task<int> BulkOperationAsync(BulkAssessmentBatchOperationDto operationDto)
        {
            try
            {
                var assessmentBatches = await _context.AssessmentBatches
                    .Where(ab => operationDto.AssessmentBatchIds.Contains(ab.BatchId))
                    .ToListAsync();

                var affectedCount = 0;

                foreach (var assessmentBatch in assessmentBatches)
                {
                    switch (operationDto.Operation?.ToLower())
                    {
                        case "activate":
                            assessmentBatch.Status = 1;
                            break;
                        case "deactivate":
                            assessmentBatch.Status = 0;
                            break;
                        case "delete":
                            assessmentBatch.Status = -2;
                            break;
                        case "close":
                            assessmentBatch.Status = 3; // Completed
                            break;
                        default:
                            continue;
                    }

                    assessmentBatch.UpdatedAt = DateTime.UtcNow;
                    affectedCount++;
                }

                if (affectedCount > 0)
                {
                    await _context.SaveChangesAsync();
                }

                return affectedCount;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error performing bulk operation: {ex.Message}", ex);
            }
        }

        #region Helper Methods

        private async Task<string> GenerateAssessmentBatchIdAsync(HealthDbContext _context, string codeName)
        {

            var generatedId = await _context.Database
                    .SqlQueryRaw<string>("SELECT dbo.fn_RemoveVietnameseDiacritics(REPLACE({0}, ' ', '')) AS Value", codeName)
                    .FirstOrDefaultAsync();

            return generatedId+ "_" + DateTime.UtcNow.ToString("yyyyMMddHHmmss") ;
        }

        private string GenerateAssessmentBatchStudentId(string studentId, string assessmentBatchId)
        {
            return studentId+ "_"+ assessmentBatchId;
        }

       

        #endregion
    }
}
