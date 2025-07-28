using Microsoft.EntityFrameworkCore;
using Web_health_app.ApiService.Entities;
using Web_health_app.Models.Models;

namespace Web_health_app.ApiService.Repository
{
    public class RoleUserRepository : IRoleUserRepository
    {
        private readonly HealthDbContext _context;

        public RoleUserRepository(HealthDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<bool> AssignRolesToUserAsync(Guid userId, List<string> roleIds)
        {
            try
            {
                // Check if user exists
                var userExists = await _context.Users.AnyAsync(u => u.UserId == userId);
                if (!userExists)
                {
                    return false;
                }

                // Check if all roles exist
                var existingRoles = await _context.Roles
                    .Where(r => roleIds.Contains(r.RoleId) && r.IsActive)
                    .Select(r => r.RoleId)
                    .ToListAsync();

                if (existingRoles.Count != roleIds.Count)
                {
                    return false;
                }

                // Get existing role assignments to avoid duplicates
                var existingAssignments = await _context.RoleUsers
                    .Where(ru => ru.UserId == userId && roleIds.Contains(ru.RoleId))
                    .Select(ru => ru.RoleId)
                    .ToListAsync();

                // Add only new assignments
                var newRoleIds = roleIds.Except(existingAssignments).ToList();

                foreach (var roleId in newRoleIds)
                {
                    _context.Database.ExecuteSqlRaw("INSERT INTO ROLE_USERS (user_ID, role_ID) VALUES ({0}, {1})", userId, roleId);

                }


                return true;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error assigning roles to user: {ex.Message}", ex);
            }
        }

        public async Task<bool> RemoveRolesFromUserAsync(Guid userId, List<string> roleIds)
        {
            try
            {
                var roleUsers = await _context.RoleUsers
                    .Where(ru => ru.UserId == userId && roleIds.Contains(ru.RoleId))
                    .ToListAsync();

                if (roleUsers.Any())
                {
                    _context.RoleUsers.RemoveRange(roleUsers);
                    await _context.SaveChangesAsync();
                }

                return true;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error removing roles from user: {ex.Message}", ex);
            }
        }

        public async Task<List<RoleInfoDto>> GetUserRolesAsync(Guid userId)
        {
            try
            {
                var roles = await _context.RoleUsers
                    .Where(ru => ru.UserId == userId)
                    .Include(ru => ru.Role)
                        .ThenInclude(r => r.Permissions)
                    .Where(ru => ru.Role.IsActive)
                    .Select(ru => new RoleInfoDto
                    {
                        RoleId = ru.Role.RoleId,
                        RoleName = ru.Role.RoleName,
                        IsActive = ru.Role.IsActive,
                        Permissions = ru.Role.Permissions.Select(p => p.PermissionId).ToList(),
                        UserCount = _context.RoleUsers.Count(r => r.RoleId == ru.Role.RoleId)
                    })
                    .OrderBy(r => r.RoleName)
                    .ToListAsync();

                return roles;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving user roles: {ex.Message}", ex);
            }
        }

        public async Task<List<UserInfoDto>> GetRoleUsersAsync(string roleId)
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
                throw new Exception($"Error retrieving role users: {ex.Message}", ex);
            }
        }

        public async Task<bool> UserHasRoleAsync(Guid userId, string roleId)
        {
            try
            {
                return await _context.RoleUsers
                    .AnyAsync(ru => ru.UserId == userId && ru.RoleId == roleId);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error checking user role: {ex.Message}", ex);
            }
        }

        public async Task<bool> ReplaceUserRolesAsync(Guid userId, List<string> roleIds)
        {
            try
            {
                // Check if user exists
                var userExists = await _context.Users.AnyAsync(u => u.UserId == userId);
                if (!userExists)
                {
                    return false;
                }

                // Check if all roles exist and are active
                var existingRoles = await _context.Roles
                    .Where(r => roleIds.Contains(r.RoleId) && r.IsActive)
                    .Select(r => r.RoleId)
                    .ToListAsync();

                if (existingRoles.Count != roleIds.Count)
                {
                    return false;
                }

                // Remove all existing role assignments
                var existingAssignments = await _context.RoleUsers
                    .Where(ru => ru.UserId == userId)
                    .ToListAsync();

                _context.RoleUsers.RemoveRange(existingAssignments);

                // Add new role assignments
                foreach (var roleId in roleIds)
                {
                    _context.RoleUsers.Add(new RoleUser
                    {
                        UserId = userId,
                        RoleId = roleId
                    });
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error replacing user roles: {ex.Message}", ex);
            }
        }

        public async Task<bool> RemoveAllUserRolesAsync(Guid userId)
        {
            try
            {
                var roleUsers = await _context.RoleUsers
                    .Where(ru => ru.UserId == userId)
                    .ToListAsync();

                if (roleUsers.Any())
                {
                    _context.RoleUsers.RemoveRange(roleUsers);
                    await _context.SaveChangesAsync();
                }

                return true;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error removing all user roles: {ex.Message}", ex);
            }
        }
    }
}
