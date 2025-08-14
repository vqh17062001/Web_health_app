using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Web_health_app.ApiService.Entities;
using Web_health_app.ApiService.Repository.Interface;
using Web_health_app.Models.Models;

namespace Web_health_app.ApiService.Repository
{
    public class AssessmentTestRepository : IAssessmentTestRepository
    {
        private readonly HealthDbContext _context;

        public AssessmentTestRepository(HealthDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<(List<AssessmentTestInfoDto> AssessmentTests, int TotalCount)> GetAllAssessmentTestsAsync(
            int pageNumber = 1, int pageSize = 10, string? searchTerm = null)
        {
            try
            {
                var query = _context.AssessmentTests.AsQueryable();

                // Apply search filter
                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    query = query.Where(at =>
                        (at.TesttypeId != null && at.TesttypeId.Contains(searchTerm)) ||
                        (at.Code != null && at.Code.Contains(searchTerm)) ||
                        (at.ResultValue != null && at.ResultValue.Contains(searchTerm)));
                }

                var totalCount = await query.CountAsync();

                var assessmentTests = await query
                    .Include(at => at.Testtype)
                    .Include(at => at.Abs)
                        .ThenInclude(abs => abs!.Student)
                    .Include(at => at.Abs)
                        .ThenInclude(abs => abs!.Batch)
                    .OrderByDescending(at => at.RecordedAt)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .Select(at => new AssessmentTestInfoDto
                    {
                        TestTypeId = at.TesttypeId,
                        AbsId = at.AbsId,
                        Code = at.Code,
                        Unit = at.Unit,
                        ResultValue = at.ResultValue,
                        RecordedAt = at.RecordedAt,
                        RecordedBy = at.RecordedBy,
                        TestTypeName = at.Testtype != null ? at.Testtype.Name : null,
                        StudentId = at.Abs != null ? at.Abs.StudentId : null,
                        StudentName = at.Abs != null && at.Abs.Student != null ? at.Abs.Student.Name : null,
                        AssessmentBatchId = at.Abs != null ? at.Abs.BatchId : null
                    })
                    .ToListAsync();

                return (assessmentTests, totalCount);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting assessment tests: {ex.Message}", ex);
            }
        }

        public async Task<(List<AssessmentTestInfoDto> AssessmentTests, int TotalCount)> SearchAssessmentTestsAsync(
            AssessmentTestSearchDto searchDto)
        {
            try
            {
                var query = _context.AssessmentTests.AsQueryable();

                // Apply filters
                if (!string.IsNullOrWhiteSpace(searchDto.TestTypeId))
                {
                    query = query.Where(at => at.TesttypeId == searchDto.TestTypeId);
                }

                if (!string.IsNullOrWhiteSpace(searchDto.AbsId))
                {
                    query = query.Where(at => at.AbsId == searchDto.AbsId);
                }

                if (!string.IsNullOrWhiteSpace(searchDto.AssessmentBatchId))
                {
                    query = query.Where(at => at.Abs != null && at.Abs.BatchId == searchDto.AssessmentBatchId);
                }

                if (!string.IsNullOrWhiteSpace(searchDto.StudentId))
                {
                    query = query.Where(at => at.Abs != null && at.Abs.StudentId == searchDto.StudentId);
                }

                if (!string.IsNullOrWhiteSpace(searchDto.SearchTerm))
                {
                    query = query.Where(at =>
                        (at.Code != null && at.Code.Contains(searchDto.SearchTerm)) ||
                        (at.ResultValue != null && at.ResultValue.Contains(searchDto.SearchTerm)));
                }

                if (searchDto.RecordedFrom.HasValue)
                {
                    query = query.Where(at => at.RecordedAt >= searchDto.RecordedFrom.Value);
                }

                if (searchDto.RecordedTo.HasValue)
                {
                    query = query.Where(at => at.RecordedAt <= searchDto.RecordedTo.Value);
                }

                if (searchDto.RecordedBy.HasValue)
                {
                    query = query.Where(at => at.RecordedBy == searchDto.RecordedBy.Value);
                }

                // Apply sorting
                if (!string.IsNullOrWhiteSpace(searchDto.SortBy))
                {
                    switch (searchDto.SortBy.ToLower())
                    {
                        case "recordedat":
                            query = searchDto.SortDirection?.ToLower() == "asc"
                                ? query.OrderBy(at => at.RecordedAt)
                                : query.OrderByDescending(at => at.RecordedAt);
                            break;
                        case "testtypeid":
                            query = searchDto.SortDirection?.ToLower() == "asc"
                                ? query.OrderBy(at => at.TesttypeId)
                                : query.OrderByDescending(at => at.TesttypeId);
                            break;
                        case "resultvalue":
                            query = searchDto.SortDirection?.ToLower() == "asc"
                                ? query.OrderBy(at => at.ResultValue)
                                : query.OrderByDescending(at => at.ResultValue);
                            break;
                        default:
                            query = query.OrderByDescending(at => at.RecordedAt);
                            break;
                    }
                }
                else
                {
                    query = query.OrderByDescending(at => at.RecordedAt);
                }

                var totalCount = await query.CountAsync();

                var assessmentTests = await query
                    .Include(at => at.Testtype)
                    .Include(at => at.Abs)
                        .ThenInclude(abs => abs!.Student)
                    .Include(at => at.Abs)
                        .ThenInclude(abs => abs!.Batch)
                    .Skip((searchDto.Page - 1) * searchDto.PageSize)
                    .Take(searchDto.PageSize)
                    .Select(at => new AssessmentTestInfoDto
                    {
                        TestTypeId = at.TesttypeId,
                        AbsId = at.AbsId,
                        Code = at.Code,
                        Unit = at.Unit,
                        ResultValue = at.ResultValue,
                        RecordedAt = at.RecordedAt,
                        RecordedBy = at.RecordedBy,
                        TestTypeName = at.Testtype != null ? at.Testtype.Name : null,
                        StudentId = at.Abs != null ? at.Abs.StudentId : null,
                        StudentName = at.Abs != null && at.Abs.Student != null ? at.Abs.Student.Name : null,
                        AssessmentBatchId = at.Abs != null ? at.Abs.BatchId : null
                    })
                    .ToListAsync();

                return (assessmentTests, totalCount);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error searching assessment tests: {ex.Message}", ex);
            }
        }

        public async Task<AssessmentTestInfoDto?> GetAssessmentTestByIdAsync(string testTypeId, string absId)
        {
            try
            {
                var assessmentTest = await _context.AssessmentTests
                    .Include(at => at.Testtype)
                    .Include(at => at.Abs)
                        .ThenInclude(abs => abs!.Student)
                    .Include(at => at.Abs)
                        .ThenInclude(abs => abs!.Batch)
                    .FirstOrDefaultAsync(at => at.TesttypeId == testTypeId && at.AbsId == absId);

                if (assessmentTest == null)
                    return null;

                return new AssessmentTestInfoDto
                {
                    TestTypeId = assessmentTest.TesttypeId,
                    AbsId = assessmentTest.AbsId,
                    Code = assessmentTest.Code,
                    Unit = assessmentTest.Unit,
                    ResultValue = assessmentTest.ResultValue,
                    RecordedAt = assessmentTest.RecordedAt,
                    RecordedBy = assessmentTest.RecordedBy,
                    TestTypeName = assessmentTest.Testtype?.Name,
                    StudentId = assessmentTest.Abs?.StudentId,
                    StudentName = assessmentTest.Abs?.Student?.Name,
                    AssessmentBatchId = assessmentTest.Abs?.BatchId
                };
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting assessment test by ID: {ex.Message}", ex);
            }
        }

        public async Task<List<AssessmentTestInfoDto>> GetAssessmentTestsByAbsIdAsync(string absId)
        {
            try
            {
                var assessmentTests = await _context.AssessmentTests
                    .Include(at => at.Testtype)
                    .Include(at => at.Abs)
                        .ThenInclude(abs => abs!.Student)
                    .Include(at => at.Abs)
                        .ThenInclude(abs => abs!.Batch)
                    .Where(at => at.AbsId == absId)
                    .OrderBy(at => at.TesttypeId)
                    .Select(at => new AssessmentTestInfoDto
                    {
                        TestTypeId = at.TesttypeId,
                        AbsId = at.AbsId,
                        Code = at.Code,
                        Unit = at.Unit,
                        ResultValue = at.ResultValue,
                        RecordedAt = at.RecordedAt,
                        RecordedBy = at.RecordedBy,
                        TestTypeName = at.Testtype != null ? at.Testtype.Name : null,
                        StudentId = at.Abs != null ? at.Abs.StudentId : null,
                        StudentName = at.Abs != null && at.Abs.Student != null ? at.Abs.Student.Name : null,
                        AssessmentBatchId = at.Abs != null ? at.Abs.BatchId : null
                    })
                    .ToListAsync();

                return assessmentTests;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting assessment tests by ABS ID: {ex.Message}", ex);
            }
        }

        public async Task<(List<AssessmentTestInfoDto> AssessmentTests, int TotalCount)> GetAssessmentTestsByBatchIdAsync(
            string batchId, int pageNumber = 1, int pageSize = 10)
        {
            try
            {
                var query = _context.AssessmentTests
                    .Where(at => at.Abs != null && at.Abs.BatchId == batchId);

                var totalCount = await query.CountAsync();

                var assessmentTests = await query
                    .Include(at => at.Testtype)
                    .Include(at => at.Abs)
                        .ThenInclude(abs => abs!.Student)
                    .Include(at => at.Abs)
                        .ThenInclude(abs => abs!.Batch)
                    .OrderBy(at => at.Abs!.StudentId)
                    .ThenBy(at => at.TesttypeId)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .Select(at => new AssessmentTestInfoDto
                    {
                        TestTypeId = at.TesttypeId,
                        AbsId = at.AbsId,
                        Code = at.Code,
                        Unit = at.Unit,
                        ResultValue = at.ResultValue,
                        RecordedAt = at.RecordedAt,
                        RecordedBy = at.RecordedBy,
                        TestTypeName = at.Testtype != null ? at.Testtype.Name : null,
                        StudentId = at.Abs != null ? at.Abs.StudentId : null,
                        StudentName = at.Abs != null && at.Abs.Student != null ? at.Abs.Student.Name : null,
                        AssessmentBatchId = at.Abs != null ? at.Abs.BatchId : null
                    })
                    .ToListAsync();

                return (assessmentTests, totalCount);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting assessment tests by batch ID: {ex.Message}", ex);
            }
        }

        public async Task<AssessmentTestInfoDto?> CreateAssessmentTestAsync(CreateAssessmentTestDto createDto)
        {
            try
            {
                // 1) Kiểm tra trùng (sử dụng FromSqlRaw)
                var exists = await _context.AssessmentTests
                    .FromSqlInterpolated(
                    $@"
                     SELECT * FROM AssessmentTests
                     WHERE testtype_ID = {createDto.TestTypeId} AND ABS_ID = {createDto.AbsId}")
                    .AsNoTracking()
                    .AnyAsync();

                if (exists)
                    throw new InvalidOperationException("Assessment test already exists for this test type and student");

                // 2) Thêm mới bằng raw SQL
                var rows = await _context.Database.ExecuteSqlInterpolatedAsync(
                    $@"
                      INSERT INTO AssessmentTests
                      ( testtype_ID, ABS_ID, code, unit, result_value, recorded_at, recorded_by)
                      VALUES
                      ({createDto.TestTypeId}, {createDto.AbsId}, {createDto.Code}, {createDto.Unit},
                        {createDto.ResultValue}, {createDto.RecordedAt}, {createDto.RecordedBy});
                    ");

                return await GetAssessmentTestByIdAsync(createDto.TestTypeId, createDto.AbsId);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error creating assessment test: {ex.Message}", ex);
            }
        }

        public async Task<AssessmentTestInfoDto?> UpdateAssessmentTestAsync(
            string testTypeId, string absId, UpdateAssessmentTestDto updateDto)
        {
            try
            {
                var assessmentTest = await _context.AssessmentTests
                    .FirstOrDefaultAsync(at => at.TesttypeId == testTypeId && at.AbsId == absId);

                if (assessmentTest == null)
                    return null;

                // ❶ Tạo câu lệnh UPDATE dùng COALESCE
                //    – Nếu tham số truyền vào NULL     ➜ giữ nguyên giá trị cũ
                //    – Nếu tham số khác NULL           ➜ ghi đè
                var sql = $@"
                            UPDATE AssessmentTests
                            SET
                            code        = COALESCE({{0}}, code ),
                            unit        = COALESCE({{1}}, unit ),
                            result_value = COALESCE({{2}}, result_value),
                            recorded_at  = COALESCE({{3}}, recorded_at ),
                            recorded_by  = COALESCE({{4}}, recorded_by )
                        WHERE testtype_ID = {{5}}
                          AND ABS_ID     = {{6}};
                    ";

                // ❷ Gọi ExecuteSqlRawAsync với mảng parameters đúng thứ tự
                await _context.Database.ExecuteSqlRawAsync(
                    sql,
                    // 0-4: giá trị mới (có thể null) -- sẽ được COALESCE
                    string.IsNullOrWhiteSpace(updateDto.Code) ? null : updateDto.Code,
                    string.IsNullOrWhiteSpace(updateDto.Unit) ? null : updateDto.Unit,
                    string.IsNullOrWhiteSpace(updateDto.ResultValue) ? null : updateDto.ResultValue,
                    updateDto.RecordedAt,        // Nullable<DateTime>
                    updateDto.RecordedBy,        // Nullable<Guid>  (hoặc string tuỳ DB)
                                           // 5-6: khoá tìm dòng cần sửa
                    testTypeId,
                    absId);

                return await GetAssessmentTestByIdAsync(testTypeId, absId);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error updating assessment test: {ex.Message}", ex);
            }
        }

        public async Task<bool> DeleteAssessmentTestAsync(string testTypeId, string absId)
        {
            try
            {
                var exists = await _context.AssessmentTests
                  .FromSqlInterpolated(
                  $@"
                     SELECT * FROM AssessmentTests
                     WHERE testtype_ID = {testTypeId} AND ABS_ID = {absId}")
                  .AsNoTracking()
                  .AnyAsync();

                if (exists == null)
                    return false;


                // 2) Xoá bằng raw SQL (ExecuteSqlInterpolatedAsync tự dùng tham số, an toàn SQL-Injection)
                var affected = await _context.Database.ExecuteSqlInterpolatedAsync(
                    $@"
                    DELETE FROM AssessmentTests
                    WHERE  testtype_ID = {testTypeId} AND ABS_ID = {absId};");



                return affected > 0;

               
            }
            catch (Exception ex)
            {
                throw new Exception($"Error deleting assessment test: {ex.Message}", ex);
            }
        }

        public async Task<bool> AssessmentTestExistsAsync(string testTypeId, string absId)
        {
            try
            {
                return await _context.AssessmentTests
                    .AnyAsync(at => at.TesttypeId == testTypeId && at.AbsId == absId);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error checking assessment test existence: {ex.Message}", ex);
            }
        }

        public async Task<(List<AssessmentTestInfoDto> AssessmentTests, int TotalCount)> GetAssessmentTestsByRecorderAsync(
            Guid recordedBy, int pageNumber = 1, int pageSize = 10)
        {
            try
            {
                var query = _context.AssessmentTests
                    .Where(at => at.RecordedBy == recordedBy);

                var totalCount = await query.CountAsync();

                var assessmentTests = await query
                    .Include(at => at.Testtype)
                    .Include(at => at.Abs)
                        .ThenInclude(abs => abs!.Student)
                    .Include(at => at.Abs)
                        .ThenInclude(abs => abs!.Batch)
                    .OrderByDescending(at => at.RecordedAt)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .Select(at => new AssessmentTestInfoDto
                    {
                        TestTypeId = at.TesttypeId,
                        AbsId = at.AbsId,
                        Code = at.Code,
                        Unit = at.Unit,
                        ResultValue = at.ResultValue,
                        RecordedAt = at.RecordedAt,
                        RecordedBy = at.RecordedBy,
                        TestTypeName = at.Testtype != null ? at.Testtype.Name : null,
                        StudentId = at.Abs != null ? at.Abs.StudentId : null,
                        StudentName = at.Abs != null && at.Abs.Student != null ? at.Abs.Student.Name : null,
                        AssessmentBatchId = at.Abs != null ? at.Abs.BatchId : null
                    })
                    .ToListAsync();

                return (assessmentTests, totalCount);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting assessment tests by recorder: {ex.Message}", ex);
            }
        }

        public async Task<int> BulkOperationAsync(BulkAssessmentTestOperationDto operationDto)
        {
            try
            {
                var affectedCount = 0;

                switch (operationDto.Operation?.ToLower())
                {
                    case "delete":
                        foreach (var testId in operationDto.TestIds)
                        {
                            var parts = testId.Split('|');
                            if (parts.Length == 2)
                            {
                                var deleted = await DeleteAssessmentTestAsync(parts[0], parts[1]);
                                if (deleted) affectedCount++;
                            }
                        }
                        break;

                    case "update_recorder":
                        if (operationDto.NewRecordedBy.HasValue)
                        {
                            foreach (var testId in operationDto.TestIds)
                            {
                                var parts = testId.Split('|');
                                if (parts.Length == 2)
                                {
                                    var updateDto = new UpdateAssessmentTestDto
                                    {
                                        RecordedBy = operationDto.NewRecordedBy.Value,
                                        RecordedAt = DateTime.UtcNow
                                    };
                                    var updated = await UpdateAssessmentTestAsync(parts[0], parts[1], updateDto);
                                    if (updated != null) affectedCount++;
                                }
                            }
                        }
                        break;
                }

                return affectedCount;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error performing bulk operation: {ex.Message}", ex);
            }
        }
    }
}
