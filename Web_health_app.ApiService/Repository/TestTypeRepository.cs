using Microsoft.EntityFrameworkCore;
using Web_health_app.ApiService.Entities;
using Web_health_app.ApiService.Repository.Interface;
using Web_health_app.Models.Models;

namespace Web_health_app.ApiService.Repository
{
    public class TestTypeRepository : ITestTypeRepository
    {
        private readonly HealthDbContext _context;

        public TestTypeRepository(HealthDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<(List<TestTypeInfoDto> TestTypes, int TotalCount)> GetAllTestTypesAsync(
            int pageNumber = 1, int pageSize = 10, string? searchTerm = null)
        {
            try
            {
                var query = _context.TestTypes.AsQueryable();

                // Apply search filter
                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    query = query.Where(tt =>
                        (tt.TesttypeId != null && tt.TesttypeId.Contains(searchTerm)) ||
                        (tt.Code != null && tt.Code.Contains(searchTerm)) ||
                        (tt.Name != null && tt.Name.Contains(searchTerm)) ||
                        (tt.Description != null && tt.Description.Contains(searchTerm)) ||
                        (tt.Unit != null && tt.Unit.Contains(searchTerm)));
                }

                var totalCount = await query.CountAsync();

                var testTypes = await query
                    .OrderBy(tt => tt.Name)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .Select(tt => new TestTypeInfoDto
                    {
                        TestTypeId = tt.TesttypeId,
                        Code = tt.Code,
                        Name = tt.Name,
                        Description = tt.Description,
                        Unit = tt.Unit
                    })
                    .ToListAsync();

                return (testTypes, totalCount);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting test types: {ex.Message}", ex);
            }
        }

        public async Task<(List<TestTypeInfoDto> TestTypes, int TotalCount)> SearchTestTypesAsync(
            TestTypeSearchDto searchDto)
        {
            try
            {
                var query = _context.TestTypes.AsQueryable();

                // Apply filters
                if (!string.IsNullOrWhiteSpace(searchDto.TestTypeId))
                {
                    query = query.Where(tt => tt.TesttypeId == searchDto.TestTypeId);
                }

                if (!string.IsNullOrWhiteSpace(searchDto.Code))
                {
                    query = query.Where(tt => tt.Code != null && tt.Code.Contains(searchDto.Code));
                }

                if (!string.IsNullOrWhiteSpace(searchDto.Name))
                {
                    query = query.Where(tt => tt.Name != null && tt.Name.Contains(searchDto.Name));
                }

                if (!string.IsNullOrWhiteSpace(searchDto.Unit))
                {
                    query = query.Where(tt => tt.Unit == searchDto.Unit);
                }

                if (!string.IsNullOrWhiteSpace(searchDto.SearchTerm))
                {
                    query = query.Where(tt =>
                        (tt.TesttypeId != null && tt.TesttypeId.Contains(searchDto.SearchTerm)) ||
                        (tt.Code != null && tt.Code.Contains(searchDto.SearchTerm)) ||
                        (tt.Name != null && tt.Name.Contains(searchDto.SearchTerm)) ||
                        (tt.Description != null && tt.Description.Contains(searchDto.SearchTerm)));
                }

                // Apply sorting
                if (!string.IsNullOrWhiteSpace(searchDto.SortBy))
                {
                    switch (searchDto.SortBy.ToLower())
                    {
                        case "testtypeid":
                            query = searchDto.SortDirection?.ToLower() == "desc"
                                ? query.OrderByDescending(tt => tt.TesttypeId)
                                : query.OrderBy(tt => tt.TesttypeId);
                            break;
                        case "code":
                            query = searchDto.SortDirection?.ToLower() == "desc"
                                ? query.OrderByDescending(tt => tt.Code)
                                : query.OrderBy(tt => tt.Code);
                            break;
                        case "name":
                            query = searchDto.SortDirection?.ToLower() == "desc"
                                ? query.OrderByDescending(tt => tt.Name)
                                : query.OrderBy(tt => tt.Name);
                            break;
                        case "unit":
                            query = searchDto.SortDirection?.ToLower() == "desc"
                                ? query.OrderByDescending(tt => tt.Unit)
                                : query.OrderBy(tt => tt.Unit);
                            break;
                        default:
                            query = query.OrderBy(tt => tt.Name);
                            break;
                    }
                }
                else
                {
                    query = query.OrderBy(tt => tt.Name);
                }

                var totalCount = await query.CountAsync();

                var testTypes = await query
                    .Skip((searchDto.Page - 1) * searchDto.PageSize)
                    .Take(searchDto.PageSize)
                    .Select(tt => new TestTypeInfoDto
                    {
                        TestTypeId = tt.TesttypeId,
                        Code = tt.Code,
                        Name = tt.Name,
                        Description = tt.Description,
                        Unit = tt.Unit
                    })
                    .ToListAsync();

                return (testTypes, totalCount);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error searching test types: {ex.Message}", ex);
            }
        }

        public async Task<TestTypeInfoDto?> GetTestTypeByIdAsync(string testTypeId)
        {
            try
            {
                var testType = await _context.TestTypes
                    .FirstOrDefaultAsync(tt => tt.TesttypeId == testTypeId);

                if (testType == null)
                    return null;

                return new TestTypeInfoDto
                {
                    TestTypeId = testType.TesttypeId,
                    Code = testType.Code,
                    Name = testType.Name,
                    Description = testType.Description,
                    Unit = testType.Unit
                };
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting test type by ID: {ex.Message}", ex);
            }
        }

        public async Task<List<TestTypeSelectDto>> GetTestTypeSelectOptionsAsync()
        {
            try
            {
                var testTypes = await _context.TestTypes
                    .OrderBy(tt => tt.Name)
                    .Select(tt => new TestTypeSelectDto
                    {
                        TestTypeId = tt.TesttypeId,
                        Name = tt.Name,
                        Code = tt.Code,
                        Unit = tt.Unit
                    })
                    .ToListAsync();

                return testTypes;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting test type select options: {ex.Message}", ex);
            }
        }

        public async Task<List<TestTypeInfoDto>> GetTestTypesByUnitAsync(string unit)
        {
            try
            {
                var testTypes = await _context.TestTypes
                    .Where(tt => tt.Unit == unit)
                    .OrderBy(tt => tt.Name)
                    .Select(tt => new TestTypeInfoDto
                    {
                        TestTypeId = tt.TesttypeId,
                        Code = tt.Code,
                        Name = tt.Name,
                        Description = tt.Description,
                        Unit = tt.Unit
                    })
                    .ToListAsync();

                return testTypes;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting test types by unit: {ex.Message}", ex);
            }
        }

        public async Task<List<TestTypeInfoDto>> GetTestTypesByCodePatternAsync(string codePattern)
        {
            try
            {
                var testTypes = await _context.TestTypes
                    .Where(tt => tt.Code != null && tt.Code.Contains(codePattern))
                    .OrderBy(tt => tt.Code)
                    .Select(tt => new TestTypeInfoDto
                    {
                        TestTypeId = tt.TesttypeId,
                        Code = tt.Code,
                        Name = tt.Name,
                        Description = tt.Description,
                        Unit = tt.Unit
                    })
                    .ToListAsync();

                return testTypes;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting test types by code pattern: {ex.Message}", ex);
            }
        }

        public async Task<bool> TestTypeExistsAsync(string testTypeId)
        {
            try
            {
                return await _context.TestTypes
                    .AnyAsync(tt => tt.TesttypeId == testTypeId);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error checking test type existence: {ex.Message}", ex);
            }
        }

        public async Task<int> GetTotalTestTypesCountAsync()
        {
            try
            {
                return await _context.TestTypes.CountAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting total test types count: {ex.Message}", ex);
            }
        }

        public async Task<List<string>> GetUniqueUnitsAsync()
        {
            try
            {
                var units = await _context.TestTypes
                    .Where(tt => tt.Unit != null)
                    .Select(tt => tt.Unit!)
                    .Distinct()
                    .OrderBy(u => u)
                    .ToListAsync();

                return units;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting unique units: {ex.Message}", ex);
            }
        }
    }
}
