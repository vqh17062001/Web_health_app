using Microsoft.EntityFrameworkCore;
using Web_health_app.ApiService.Entities;
using Web_health_app.Models.Models;

namespace Web_health_app.ApiService.Repository
{
    public class RoleRepository : IRoleRepository
    {
        private readonly HealthDbContext _context;

        public RoleRepository(HealthDbContext context)
        {
            _context = context;
        }

        public async Task<(List<RoleInfoDto> Roles, int TotalCount)> GetAllRolesAsync(int pageNumber = 1, int pageSize = 10, string? searchTerm = null, bool includeInactive = false)
        {
            try
            {
                var query = _context.Roles
                    .Include(r => r.Permissions)
                    .AsQueryable();

                // Filter by active status
                if (!includeInactive)
                {
                    query = query.Where(r => r.IsActive);
                }

                // Apply search filter
                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    query = query.Where(r =>
                        r.RoleId.Contains(searchTerm) ||
                        r.RoleName.Contains(searchTerm));
                }

                var totalCount = await query.CountAsync();

                var roles = await query
                    .OrderBy(r => r.RoleId)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .Select(r => new RoleInfoDto
                    {
                        RoleId = r.RoleId,
                        RoleName = r.RoleName,
                        IsActive = r.IsActive,
                        Permissions = r.Permissions.Select(p => p.PermissionId).ToList(),
                        UserCount = _context.RoleUsers.Count(ru => ru.RoleId == r.RoleId)
                    })
                    .ToListAsync();

                return (roles, totalCount);
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving roles", ex);
            }
        }

        public async Task<RoleInfoDto?> GetRoleByIdAsync(string roleId)
        {
            try
            {
                var role = await _context.Roles
                    .Include(r => r.Permissions)
                    .Where(r => r.RoleId == roleId)
                    .Select(r => new RoleInfoDto
                    {
                        RoleId = r.RoleId,
                        RoleName = r.RoleName,
                        IsActive = r.IsActive,
                        Permissions = r.Permissions.Select(p => p.PermissionId).ToList(),
                        UserCount = _context.RoleUsers.Count(ru => ru.RoleId == r.RoleId)
                    })
                    .FirstOrDefaultAsync();

                return role;
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving role by ID", ex);
            }
        }

        public async Task<List<RoleInfoDto>> GetActiveRolesAsync()
        {
            try
            {
                var roles = await _context.Roles
                    .Include(r => r.Permissions)
                    .Where(r => r.IsActive)
                    .OrderBy(r => r.RoleName)
                    .Select(r => new RoleInfoDto
                    {
                        RoleId = r.RoleId,
                        RoleName = r.RoleName,
                        IsActive = r.IsActive,
                        Permissions = r.Permissions.Select(p => p.PermissionId).ToList(),
                        UserCount = _context.RoleUsers.Count(ru => ru.RoleId == r.RoleId)
                    })
                    .ToListAsync();

                return roles;
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving active roles", ex);
            }
        }

        public async Task<RoleInfoDto> CreateRoleAsync(CreateRoleDto createRoleDto)
        {
            try
            {
                // Auto-generate RoleId using SQL function to remove Vietnamese diacritics and spaces
                var generatedRoleId = await _context.Database
                    .SqlQueryRaw<string>("SELECT dbo.fn_RemoveVietnameseDiacritics(REPLACE({0}, ' ', '')) AS Value", createRoleDto.RoleName)
                    .FirstOrDefaultAsync();

                if (string.IsNullOrEmpty(generatedRoleId))
                {
                    throw new InvalidOperationException("Failed to generate Role ID from Role Name");
                }

                // Check if generated role ID already exists
                if (await RoleIdExistsAsync(generatedRoleId))
                {
                    throw new InvalidOperationException($"Generated Role ID '{generatedRoleId}' already exists");
                }

                var role = new Role
                {
                    RoleId = generatedRoleId,
                    RoleName = createRoleDto.RoleName,
                    IsActive = createRoleDto.IsActive
                };

                _context.Roles.Add(role);
                await _context.SaveChangesAsync();

                return await GetRoleByIdAsync(role.RoleId) ?? throw new InvalidOperationException("Failed to retrieve created role");
            }
            catch (Exception ex)
            {
                throw new Exception("Error creating role", ex);
            }
        }

        public async Task<RoleInfoDto?> UpdateRoleAsync(string roleId, UpdateRoleDto updateRoleDto)
        {
            try
            {
                var role = await _context.Roles.FirstOrDefaultAsync(r => r.RoleId == roleId);
                if (role == null)
                {
                    return null;
                }

                // Update only provided fields
                if (updateRoleDto.RoleName != null)
                    role.RoleName = updateRoleDto.RoleName;

                if (updateRoleDto.IsActive.HasValue)
                    role.IsActive = updateRoleDto.IsActive.Value;

                await _context.SaveChangesAsync();

                // Update permissions if provided
                if (updateRoleDto.PermissionIds != null)
                {
                    await AssignPermissionsToRoleAsync(roleId, updateRoleDto.PermissionIds);
                }

                return await GetRoleByIdAsync(roleId);
            }
            catch (Exception ex)
            {
                throw new Exception("Error updating role", ex);
            }
        }

        public async Task<bool> DeleteRoleAsync(string roleId)
        {
            try
            {
                var role = await _context.Roles.FirstOrDefaultAsync(r => r.RoleId == roleId);
                if (role == null)
                {
                    return false;
                }

                // Soft delete by setting IsActive to false
                role.IsActive = false;
                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                throw new Exception("Error deleting role", ex);
            }
        }

        public async Task<bool> HardDeleteRoleAsync(string roleId)
        {
            try
            {
                var role = await _context.Roles
                    .Include(r => r.Permissions)
                    .FirstOrDefaultAsync(r => r.RoleId == roleId);

                if (role == null)
                {
                    return false;
                }

                // Remove role-permission relationships first
                role.Permissions.Clear();

                // Remove the role
                _context.Roles.Remove(role);
                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                throw new Exception("Error permanently deleting role", ex);
            }
        }

        public async Task<bool> RoleIdExistsAsync(string roleId, string? excludeRoleId = null)
        {
            try
            {
                var query = _context.Roles.Where(r => r.RoleId == roleId);

                if (!string.IsNullOrEmpty(excludeRoleId))
                {
                    query = query.Where(r => r.RoleId != excludeRoleId);
                }

                return await query.AnyAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error checking role ID existence", ex);
            }
        }

        public async Task<bool> AssignPermissionsToRoleAsync(string roleId, List<string> permissionIds)
        {
            try
            {
                var role = await _context.Roles
                    .Include(r => r.Permissions)
                    .FirstOrDefaultAsync(r => r.RoleId == roleId);

                if (role == null)
                {
                    return false;
                }

                // Clear existing permissions
                role.Permissions.Clear();

                // Add new permissions
                if (permissionIds.Any())
                {
                    var permissions = await _context.Permissions
                        .Where(p => permissionIds.Contains(p.PermissionId))
                        .ToListAsync();

                    foreach (var permission in permissions)
                    {
                        role.Permissions.Add(permission);
                    }
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception("Error assigning permissions to role", ex);
            }
        }

        public async Task<(List<RoleWithUserCountDto> Roles, int TotalCount)> GetRolesWithUserCountAsync(int pageNumber = 1, int pageSize = 10, string? searchTerm = null, bool includeInactive = false)
        {
            try
            {
                var query = _context.Roles
                    .Include(r => r.Permissions)
                    .AsQueryable();

                // Filter by active status
                if (!includeInactive)
                {
                    query = query.Where(r => r.IsActive);
                }

                // Apply search filter
                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    query = query.Where(r =>
                        r.RoleId.Contains(searchTerm) ||
                        r.RoleName.Contains(searchTerm));
                }

                var totalCount = await query.CountAsync();

                var roles = await query
                    .OrderBy(r => r.RoleId)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .Select(r => new RoleWithUserCountDto
                    {
                        RoleId = r.RoleId,
                        RoleName = r.RoleName,
                        IsActive = r.IsActive,
                        Permissions = r.Permissions.Select(p => p.PermissionId).ToList(),
                        // Note: This is a simplified user count - you might need to adjust based on your user-role relationship
                        UserCount = _context.RoleUsers.Count(ru => ru.RoleId == r.RoleId)
                    })
                    .ToListAsync();

                return (roles, totalCount);
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving roles with user count", ex);
            }
        }

        public async Task<List<string>> GetRolePermissionsAsync(string roleId)
        {
            try
            {
                var permissions = await _context.Roles
                    .Where(r => r.RoleId == roleId)
                    .SelectMany(r => r.Permissions)
                    .Select(p => p.PermissionId)
                    .ToListAsync();

                return permissions;
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving role permissions", ex);
            }
        }

        public async Task<List<UserInfoDto>> GetUsersInRoleAsync(string roleId)
        {
            try
            {
                var users = await _context.RoleUsers
                    .Where(ru => ru.RoleId == roleId)
                    .Include(ru => ru.User)
                        .ThenInclude(u => u.Group)
                    .Include(ru => ru.User)
                        .ThenInclude(u => u.ManageByNavigation)
                    .Where(ru => ru.User.UserStatus != -2) // Exclude deleted users
                    .Select(ru => new UserInfoDto
                    {
                        UserId = ru.User.UserId,
                        UserName = ru.User.UserName,
                        FullName = ru.User.FullName,
                        PhoneNumber = ru.User.PhoneNumber,
                        Department = ru.User.Department,
                        UserStatus = ru.User.UserStatus,
                        UserStatusString = ru.User.GetUserStatusString(),
                        ManageBy = ru.User.ManageBy,
                        ManagerName = ru.User.ManageByNavigation != null ?
                            ru.User.ManageByNavigation.FullName ?? ru.User.ManageByNavigation.UserName : null,
                        LevelSecurity = ru.User.LevelSecurity,
                        CreateAt = ru.User.CreateAt,
                        UpdateAt = ru.User.UpdateAt,
                        GroupId = ru.User.GroupId,
                        GroupName = ru.User.Group != null ? ru.User.Group.GroupName : null
                    })
                    .OrderBy(u => u.FullName ?? u.UserName)
                    .ToListAsync();

                return users;
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving users in role", ex);
            }
        }

        public async Task<(List<RoleInfoDto> Roles, int TotalCount)> SearchRolesAsync(RoleSearchDto searchDto, int pageNumber = 1, int pageSize = 10)
        {
            try
            {
                var query = _context.Roles
                    .Include(r => r.Permissions)
                    .AsQueryable();

                // Apply search filters
                if (!string.IsNullOrWhiteSpace(searchDto.SearchTerm))
                {
                    query = query.Where(r =>
                        r.RoleId.Contains(searchDto.SearchTerm) ||
                        r.RoleName.Contains(searchDto.SearchTerm));
                }

                if (searchDto.IsActive.HasValue)
                {
                    query = query.Where(r => r.IsActive == searchDto.IsActive.Value);
                }

                // Apply user count filters
                if (searchDto.MinUserCount.HasValue || searchDto.MaxUserCount.HasValue)
                {
                    var roleUserCounts = _context.RoleUsers
                        .GroupBy(ru => ru.RoleId)
                        .Select(g => new { RoleId = g.Key, UserCount = g.Count() })
                        .AsQueryable();

                    if (searchDto.MinUserCount.HasValue)
                    {
                        var validRoleIds = roleUserCounts
                            .Where(ruc => ruc.UserCount >= searchDto.MinUserCount.Value)
                            .Select(ruc => ruc.RoleId);
                        query = query.Where(r => validRoleIds.Contains(r.RoleId));
                    }

                    if (searchDto.MaxUserCount.HasValue)
                    {
                        var validRoleIds = roleUserCounts
                            .Where(ruc => ruc.UserCount <= searchDto.MaxUserCount.Value)
                            .Select(ruc => ruc.RoleId);
                        query = query.Where(r => validRoleIds.Contains(r.RoleId));
                    }
                }

                // Apply permission count filters
                if (searchDto.MinPermissionCount.HasValue)
                {
                    query = query.Where(r => r.Permissions.Count >= searchDto.MinPermissionCount.Value);
                }

                if (searchDto.MaxPermissionCount.HasValue)
                {
                    query = query.Where(r => r.Permissions.Count <= searchDto.MaxPermissionCount.Value);
                }

                // Filter by specific permissions
                if (!string.IsNullOrWhiteSpace(searchDto.HasPermissions))
                {
                    var requiredPermissions = searchDto.HasPermissions.Split(',')
                        .Select(p => p.Trim())
                        .Where(p => !string.IsNullOrEmpty(p))
                        .ToList();

                    if (requiredPermissions.Any())
                    {
                        query = query.Where(r => r.Permissions.Any(p => requiredPermissions.Contains(p.PermissionId)));
                    }
                }

                // Filter roles with no users
                if (searchDto.IsEmpty.HasValue && searchDto.IsEmpty.Value)
                {
                    var rolesWithUsers = _context.RoleUsers.Select(ru => ru.RoleId).Distinct();
                    query = query.Where(r => !rolesWithUsers.Contains(r.RoleId));
                }

                // Apply sorting
                switch (searchDto.SortBy?.ToLower())
                {
                    case "rolename":
                        query = searchDto.SortDirection?.ToLower() == "desc"
                            ? query.OrderByDescending(r => r.RoleName)
                            : query.OrderBy(r => r.RoleName);
                        break;
                    case "isactive":
                        query = searchDto.SortDirection?.ToLower() == "desc"
                            ? query.OrderByDescending(r => r.IsActive)
                            : query.OrderBy(r => r.IsActive);
                        break;
                    case "permissioncount":
                        query = searchDto.SortDirection?.ToLower() == "desc"
                            ? query.OrderByDescending(r => r.Permissions.Count)
                            : query.OrderBy(r => r.Permissions.Count);
                        break;
                    default:
                        query = searchDto.SortDirection?.ToLower() == "desc"
                            ? query.OrderByDescending(r => r.RoleId)
                            : query.OrderBy(r => r.RoleId);
                        break;
                }

                var totalCount = await query.CountAsync();

                var roles = await query
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .Select(r => new RoleInfoDto
                    {
                        RoleId = r.RoleId,
                        RoleName = r.RoleName,
                        IsActive = r.IsActive,
                        Permissions = r.Permissions.Select(p => p.PermissionId).ToList(),
                        UserCount = _context.RoleUsers.Count(ru => ru.RoleId == r.RoleId)
                    })
                    .ToListAsync();

                return (roles, totalCount);
            }
            catch (Exception ex)
            {
                throw new Exception("Error searching roles", ex);
            }
        }
    }
}
