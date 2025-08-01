using Microsoft.EntityFrameworkCore;
using Web_health_app.ApiService.Entities;
using Web_health_app.Models.Models;

namespace Web_health_app.ApiService.Repository
{
    public class GroupRepository : IGroupRepository
    {
        private readonly HealthDbContext _context;

        public GroupRepository(HealthDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<(List<GroupInfoDto> Groups, int TotalCount)> GetAllGroupsAsync(int pageNumber = 1, int pageSize = 10, string? searchTerm = null, bool includeInactive = false)
        {
            try
            {
                var query = _context.Groups
                    .Include(g => g.Users)
                    .Include(g => g.TimeActive)
                    .AsQueryable();

                // Filter by active status
                if (!includeInactive)
                {
                    query = query.Where(g => g.IsActive);
                }

                // Apply search filter
                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    query = query.Where(g =>
                        g.GroupId.Contains(searchTerm) ||
                        g.GroupName.Contains(searchTerm) ||
                        (g.TimeActiveId != null && g.TimeActiveId.Contains(searchTerm)));
                }

                var totalCount = await query.CountAsync();

                var groups = await query
                    .OrderBy(g => g.GroupId)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .Select(g => new GroupInfoDto
                    {
                        GroupId = g.GroupId,
                        GroupName = g.GroupName,
                        TimeActiveId = g.TimeActiveId,
                        IsActive = g.IsActive,
                        UserCount = g.Users.Count,
                        RoleCount = _context.GroupRoles.Count(gr => gr.GroupId == g.GroupId)
                    })
                    .ToListAsync();

                return (groups, totalCount);
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving groups", ex);
            }
        }

        public async Task<GroupInfoDto?> GetGroupByIdAsync(string groupId)
        {
            try
            {
                var group = await _context.Groups
                    .Include(g => g.Users)
                    .Include(g => g.TimeActive)
                    .FirstOrDefaultAsync(g => g.GroupId == groupId);

                if (group == null)
                    return null;

                return new GroupInfoDto
                {
                    GroupId = group.GroupId,
                    GroupName = group.GroupName,
                    TimeActiveId = group.TimeActiveId,
                    IsActive = group.IsActive,
                    UserCount = group.Users.Count,
                    RoleCount = await _context.GroupRoles.CountAsync(gr => gr.GroupId == group.GroupId)
                };
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving group by ID: {groupId}", ex);
            }
        }

        public async Task<GroupDetailDto?> GetGroupDetailAsync(string groupId)
        {
            try
            {
                var group = await _context.Groups
                    .Include(g => g.Users)
                    .Include(g => g.TimeActive)
                    .FirstOrDefaultAsync(g => g.GroupId == groupId);

                if (group == null)
                    return null;

                // Get users in group
                var users = group.Users.Select(u => new UserInfoDto
                {
                    UserId = u.UserId,
                    UserName = u.UserName,
                    FullName = u.FullName,
                    PhoneNumber = u.PhoneNumber,
                    Department = u.Department,
                    UserStatus = u.UserStatus,
                    UserStatusString = u.GetUserStatusString(),
                    ManageBy = u.ManageBy,
                    LevelSecurity = u.LevelSecurity,
                    CreateAt = u.CreateAt,
                    UpdateAt = u.UpdateAt,
                    GroupId = u.GroupId,
                    GroupName = group.GroupName
                }).ToList();

                // Get roles assigned to group
                var groupRoles = await _context.GroupRoles
                    .Include(gr => gr.Role)
                    .Where(gr => gr.GroupId == groupId)
                    .ToListAsync();

                var roles = groupRoles.Select(gr => new RoleInfoDto
                {
                    RoleId = gr.Role.RoleId,
                    RoleName = gr.Role.RoleName,
                    IsActive = gr.Role.IsActive,
                    UserCount = _context.RoleUsers.Count(ru => ru.RoleId == gr.Role.RoleId),
                    Permissions = _context.Permissions
                                .Where(p => p.RoleId == gr.Role.RoleId)
                                .Select(p => p.PermissionId)
                                .ToList()
                }).ToList();

                return new GroupDetailDto
                {
                    GroupId = group.GroupId,
                    GroupName = group.GroupName,
                    TimeActiveId = group.TimeActiveId,
                    IsActive = group.IsActive,
                    UserCount = users.Count,
                    RoleCount = roles.Count,
                    Users = users,
                    Roles = roles,
                    TimeActiveName = group.TimeActive?.TimeActiveId
                };
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving group detail: {groupId}", ex);
            }
        }

        public async Task<List<GroupInfoDto>> GetActiveGroupsAsync()
        {
            try
            {
                return await _context.Groups
                    .Where(g => g.IsActive)
                    .Include(g => g.Users)
                    .OrderBy(g => g.GroupName)
                    .Select(g => new GroupInfoDto
                    {
                        GroupId = g.GroupId,
                        GroupName = g.GroupName,
                        TimeActiveId = g.TimeActiveId,
                        IsActive = g.IsActive,
                        UserCount = g.Users.Count,
                        RoleCount = _context.GroupRoles.Count(gr => gr.GroupId == g.GroupId)
                    })
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving active groups", ex);
            }
        }

        public async Task<GroupInfoDto> CreateGroupAsync(CreateGroupDto createGroupDto)
        {
            try
            {
                var generatedGroupId = await _context.Database
                  .SqlQueryRaw<string>("SELECT dbo.fn_RemoveVietnameseDiacritics(REPLACE({0}, ' ', '')) AS Value", createGroupDto.GroupName)
                  .FirstOrDefaultAsync();
                if (string.IsNullOrEmpty(generatedGroupId))
                {
                    throw new InvalidOperationException("Failed to generate Role ID from Role Name");
                }

                // Check if group ID already exists
                var existingGroup = await _context.Groups.AnyAsync(g => g.GroupId == generatedGroupId);
                if (existingGroup)
                {
                    throw new InvalidOperationException($"Group with ID '{generatedGroupId}' already exists");
                }

                // Validate TimeActiveId if provided
                if (!string.IsNullOrEmpty(createGroupDto.TimeActiveId))
                {
                    var timeActiveExists = await _context.TimeActives.AnyAsync(ta => ta.TimeActiveId == createGroupDto.TimeActiveId);
                    if (!timeActiveExists)
                    {
                        throw new InvalidOperationException($"TimeActive with ID '{createGroupDto.TimeActiveId}' does not exist");
                    }
                }

                var group = new Group
                {
                    GroupId = generatedGroupId,
                    GroupName = createGroupDto.GroupName,
                    TimeActiveId = createGroupDto.TimeActiveId,
                    IsActive = createGroupDto.IsActive
                };

                _context.Groups.Add(group);
                await _context.SaveChangesAsync();

                return new GroupInfoDto
                {
                    GroupId = group.GroupId,
                    GroupName = group.GroupName,
                    TimeActiveId = group.TimeActiveId,
                    IsActive = group.IsActive,
                    UserCount = 0,
                    RoleCount = 0,
                    CreatedAt = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                throw new Exception("Error creating group", ex);
            }
        }

        public async Task<GroupInfoDto?> UpdateGroupAsync(string groupId, UpdateGroupDto updateGroupDto)
        {
            try
            {
                var group = await _context.Groups
                    .Include(g => g.Users)
                    .FirstOrDefaultAsync(g => g.GroupId == groupId);

                if (group == null)
                    return null;

                // Validate TimeActiveId if provided
                if (!string.IsNullOrEmpty(updateGroupDto.TimeActiveId))
                {
                    var timeActiveExists = await _context.TimeActives.AnyAsync(ta => ta.TimeActiveId == updateGroupDto.TimeActiveId);
                    if (!timeActiveExists)
                    {
                        throw new InvalidOperationException($"TimeActive with ID '{updateGroupDto.TimeActiveId}' does not exist");
                    }
                }

                group.GroupName = updateGroupDto.GroupName;
                group.TimeActiveId = updateGroupDto.TimeActiveId;
                group.IsActive = updateGroupDto.IsActive;

                await _context.SaveChangesAsync();

                return new GroupInfoDto
                {
                    GroupId = group.GroupId,
                    GroupName = group.GroupName,
                    TimeActiveId = group.TimeActiveId,
                    IsActive = group.IsActive,
                    UserCount = group.Users.Count,
                    RoleCount = await _context.GroupRoles.CountAsync(gr => gr.GroupId == group.GroupId),
                    UpdatedAt = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                throw new Exception($"Error updating group: {groupId}", ex);
            }
        }

        public async Task<bool> DeleteGroupAsync(string groupId)
        {
            try
            {
                var group = await _context.Groups.FirstOrDefaultAsync(g => g.GroupId == groupId);
                if (group == null)
                    return false;

                group.IsActive = false;
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error deleting group: {groupId}", ex);
            }
        }

        public async Task<bool> HardDeleteGroupAsync(string groupId)
        {
            try
            {
                var group = await _context.Groups
                    .Include(g => g.Users)
                    .FirstOrDefaultAsync(g => g.GroupId == groupId);

                if (group == null)
                    return false;

                // Remove users from group first
                foreach (var user in group.Users)
                {
                    user.GroupId = null;
                }

                // Remove group roles
                var groupRoles = await _context.GroupRoles.Where(gr => gr.GroupId == groupId).ToListAsync();
                _context.GroupRoles.RemoveRange(groupRoles);

                // Remove the group
                _context.Groups.Remove(group);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error hard deleting group: {groupId}", ex);
            }
        }

        public async Task<bool> GroupIdExistsAsync(string groupName, string? excludeGroupId = null)
        {
            try
            {
                var generatedGroupId = await _context.Database
                  .SqlQueryRaw<string>("SELECT dbo.fn_RemoveVietnameseDiacritics(REPLACE({0}, ' ', '')) AS Value", groupName)
                  .FirstOrDefaultAsync();


                var query = _context.Groups.AsQueryable();

                if (!string.IsNullOrEmpty(excludeGroupId))
                {
                    query = query.Where(g => g.GroupId != excludeGroupId);
                }

                return await query.AnyAsync(g => g.GroupId == generatedGroupId);
            }
            catch (Exception ex)
            {
                throw new Exception("Error checking group ID existence", ex);
            }
        }

        public async Task<(List<GroupWithUserCountDto> Groups, int TotalCount)> GetGroupsWithUserCountAsync(int pageNumber = 1, int pageSize = 10, string? searchTerm = null, bool includeInactive = false)
        {
            try
            {
                var query = _context.Groups
                    .Include(g => g.Users)
                    .Include(g => g.TimeActive)
                    .AsQueryable();

                // Filter by active status
                if (!includeInactive)
                {
                    query = query.Where(g => g.IsActive);
                }

                // Apply search filter
                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    query = query.Where(g =>
                        g.GroupId.Contains(searchTerm) ||
                        g.GroupName.Contains(searchTerm));
                }

                var totalCount = await query.CountAsync();

                var groups = await query
                    .OrderBy(g => g.GroupId)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .Select(g => new GroupWithUserCountDto
                    {
                        GroupId = g.GroupId,
                        GroupName = g.GroupName,
                        TimeActiveId = g.TimeActiveId,
                        IsActive = g.IsActive,
                        UserCount = g.Users.Count,
                        RoleCount = _context.GroupRoles.Count(gr => gr.GroupId == g.GroupId),
                        UserNames = g.Users.Take(5).Select(u => u.UserName).ToList(),
                        LastActivity = g.Users.Max(u => (DateTime?)u.UpdateAt)
                    })
                    .ToListAsync();

                return (groups, totalCount);
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving groups with user count", ex);
            }
        }

        public async Task<bool> AddUsersToGroupAsync(string groupId, List<Guid> userIds)
        {
            try
            {
                // Check if group exists and is active
                var groupExists = await _context.Groups.AnyAsync(g => g.GroupId == groupId && g.IsActive);
                if (!groupExists)
                    return false;

                // Check if all users exist
                var existingUsers = await _context.Users
                    .Where(u => userIds.Contains(u.UserId))
                    .ToListAsync();

                if (existingUsers.Count != userIds.Count)
                    return false;

                // Update users' group assignment
                foreach (var user in existingUsers)
                {
                    user.GroupId = groupId;
                    user.UpdateAt = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error adding users to group: {groupId}", ex);
            }
        }

        public async Task<bool> RemoveUsersFromGroupAsync(List<Guid> userIds)
        {
            try
            {
                var users = await _context.Users
                    .Where(u => userIds.Contains(u.UserId))
                    .ToListAsync();

                foreach (var user in users)
                {
                    user.GroupId = null;
                    user.UpdateAt = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception("Error removing users from group", ex);
            }
        }

        public async Task<bool> RemoveUsersFromSpecificGroupAsync(string groupId, List<Guid> userIds)
        {
            try
            {
                var users = await _context.Users
                    .Where(u => userIds.Contains(u.UserId) && u.GroupId == groupId)
                    .ToListAsync();

                foreach (var user in users)
                {
                    user.GroupId = null;
                    user.UpdateAt = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error removing users from specific group: {groupId}", ex);
            }
        }

        public async Task<List<UserInfoDto>> GetGroupUsersAsync(string groupId)
        {
            try
            {
                var users = await _context.Users
                    .Include(u => u.Group)
                    .Where(u => u.GroupId == groupId)
                    .OrderBy(u => u.UserName)
                    .Select(u => new UserInfoDto
                    {
                        UserId = u.UserId,
                        UserName = u.UserName,
                        FullName = u.FullName,
                        PhoneNumber = u.PhoneNumber,
                        Department = u.Department,
                        UserStatus = u.UserStatus,
                        UserStatusString = u.GetUserStatusString(),
                        ManageBy = u.ManageBy,
                        LevelSecurity = u.LevelSecurity,
                        CreateAt = u.CreateAt,
                        UpdateAt = u.UpdateAt,
                        GroupId = u.GroupId,
                        GroupName = u.Group != null ? u.Group.GroupName : null
                    })
                    .ToListAsync();

                return users;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving users for group: {groupId}", ex);
            }
        }

        public async Task<List<GroupInfoDto>> GetUserGroupsAsync(Guid userId)
        {
            try
            {
                var user = await _context.Users
                    .Include(u => u.Group)
                    .FirstOrDefaultAsync(u => u.UserId == userId);

                if (user?.Group == null)
                    return new List<GroupInfoDto>();

                var group = new GroupInfoDto
                {
                    GroupId = user.Group.GroupId,
                    GroupName = user.Group.GroupName,
                    TimeActiveId = user.Group.TimeActiveId,
                    IsActive = user.Group.IsActive,
                    UserCount = await _context.Users.CountAsync(u => u.GroupId == user.Group.GroupId),
                    RoleCount = await _context.GroupRoles.CountAsync(gr => gr.GroupId == user.Group.GroupId)
                };

                return new List<GroupInfoDto> { group };
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving groups for user: {userId}", ex);
            }
        }

        public async Task<bool> MoveUserToGroupAsync(Guid userId, string? newGroupId)
        {
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId);
                if (user == null)
                    return false;

                // Validate new group if provided
                if (!string.IsNullOrEmpty(newGroupId))
                {
                    var groupExists = await _context.Groups.AnyAsync(g => g.GroupId == newGroupId && g.IsActive);
                    if (!groupExists)
                        return false;
                }

                user.GroupId = newGroupId;
                user.UpdateAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error moving user to group: {userId}", ex);
            }
        }

        public async Task<bool> AddRolesToGroupAsync(string groupId, List<string> roleIds, string? note = null)
        {
            try
            {
                // Check if group exists
                var groupExists = await _context.Groups.AnyAsync(g => g.GroupId == groupId);
                if (!groupExists)
                    return false;

                // Check if all roles exist
                var existingRoles = await _context.Roles
                    .Where(r => roleIds.Contains(r.RoleId) && r.IsActive)
                    .Select(r => r.RoleId)
                    .ToListAsync();

                if (existingRoles.Count != roleIds.Count)
                    return false;

                // Get existing role assignments to avoid duplicates
                var existingAssignments = await _context.GroupRoles
                    .Where(gr => gr.GroupId == groupId && roleIds.Contains(gr.RoleId))
                    .Select(gr => gr.RoleId)
                    .ToListAsync();

                // Add only new assignments
                var newRoleIds = roleIds.Except(existingAssignments).ToList();

                foreach (var roleId in newRoleIds)
                {
                    _context.GroupRoles.Add(new GroupRole
                    {
                        GroupId = groupId,
                        RoleId = roleId,
                        Note = note
                    });
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error adding roles to group: {groupId}", ex);
            }
        }

        public async Task<bool> RemoveRolesFromGroupAsync(string groupId, List<string> roleIds)
        {
            try
            {
                var groupRoles = await _context.GroupRoles
                    .Where(gr => gr.GroupId == groupId && roleIds.Contains(gr.RoleId))
                    .ToListAsync();

                if (groupRoles.Any())
                {
                    _context.GroupRoles.RemoveRange(groupRoles);
                    await _context.SaveChangesAsync();
                }

                return true;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error removing roles from group: {groupId}", ex);
            }
        }

        public async Task<List<GroupRoleDto>> GetGroupRolesAsync(string groupId)
        {
            try
            {
                var groupRoles = await _context.GroupRoles
                    .Include(gr => gr.Group)
                    .Include(gr => gr.Role)
                    .Where(gr => gr.GroupId == groupId)
                    .Select(gr => new GroupRoleDto
                    {
                        GroupId = gr.GroupId,
                        RoleId = gr.RoleId,
                        Note = gr.Note,
                        Group = new GroupInfoDto
                        {
                            GroupId = gr.Group.GroupId,
                            GroupName = gr.Group.GroupName,
                            IsActive = gr.Group.IsActive
                        },
                        Role = new RoleInfoDto
                        {
                            RoleId = gr.Role.RoleId,
                            RoleName = gr.Role.RoleName,
                            IsActive = gr.Role.IsActive
                        }
                    })
                    .ToListAsync();

                return groupRoles;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving roles for group: {groupId}", ex);
            }
        }

        public async Task<List<GroupRoleDto>> GetRoleGroupsAsync(string roleId)
        {
            try
            {
                var groupRoles = await _context.GroupRoles
                    .Include(gr => gr.Group)
                    .Include(gr => gr.Role)
                    .Where(gr => gr.RoleId == roleId)
                    .Select(gr => new GroupRoleDto
                    {
                        GroupId = gr.GroupId,
                        RoleId = gr.RoleId,
                        Note = gr.Note,
                        Group = new GroupInfoDto
                        {
                            GroupId = gr.Group.GroupId,
                            GroupName = gr.Group.GroupName,
                            IsActive = gr.Group.IsActive
                        },
                        Role = new RoleInfoDto
                        {
                            RoleId = gr.Role.RoleId,
                            RoleName = gr.Role.RoleName,
                            IsActive = gr.Role.IsActive
                        }
                    })
                    .ToListAsync();

                return groupRoles;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving groups for role: {roleId}", ex);
            }
        }

        public async Task<bool> ReplaceGroupRolesAsync(string groupId, List<string> roleIds, string? note = null)
        {
            try
            {
                // Remove existing roles
                var existingRoles = await _context.GroupRoles
                    .Where(gr => gr.GroupId == groupId)
                    .ToListAsync();

                _context.GroupRoles.RemoveRange(existingRoles);

                // Add new roles
                if (roleIds.Any())
                {
                    // Check if all roles exist
                    var existingRoleIds = await _context.Roles
                        .Where(r => roleIds.Contains(r.RoleId) && r.IsActive)
                        .Select(r => r.RoleId)
                        .ToListAsync();

                    if (existingRoleIds.Count != roleIds.Count)
                        return false;

                    foreach (var roleId in roleIds)
                    {
                        _context.GroupRoles.Add(new GroupRole
                        {
                            GroupId = groupId,
                            RoleId = roleId,
                            Note = note
                        });
                    }
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error replacing roles for group: {groupId}", ex);
            }
        }

        public async Task<bool> RemoveAllGroupRolesAsync(string groupId)
        {
            try
            {
                var groupRoles = await _context.GroupRoles
                    .Where(gr => gr.GroupId == groupId)
                    .ToListAsync();

                if (groupRoles.Any())
                {
                    _context.GroupRoles.RemoveRange(groupRoles);
                    await _context.SaveChangesAsync();
                }

                return true;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error removing all roles from group: {groupId}", ex);
            }
        }

        public async Task<List<GroupInfoDto>> GetGroupsByTimeActiveAsync(string timeActiveId)
        {
            try
            {
                return await _context.Groups
                    .Include(g => g.Users)
                    .Where(g => g.TimeActiveId == timeActiveId)
                    .OrderBy(g => g.GroupName)
                    .Select(g => new GroupInfoDto
                    {
                        GroupId = g.GroupId,
                        GroupName = g.GroupName,
                        TimeActiveId = g.TimeActiveId,
                        IsActive = g.IsActive,
                        UserCount = g.Users.Count,
                        RoleCount = _context.GroupRoles.Count(gr => gr.GroupId == g.GroupId)
                    })
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving groups by time active: {timeActiveId}", ex);
            }
        }

        public async Task<int> BulkGroupOperationAsync(BulkGroupOperationDto bulkOperationDto)
        {
            try
            {
                var groups = await _context.Groups
                    .Where(g => bulkOperationDto.GroupIds.Contains(g.GroupId))
                    .ToListAsync();

                int affectedCount = 0;

                foreach (var group in groups)
                {
                    switch (bulkOperationDto.Operation?.ToLower())
                    {
                        case "activate":
                            group.IsActive = true;
                            affectedCount++;
                            break;
                        case "deactivate":
                            group.IsActive = false;
                            affectedCount++;
                            break;
                        case "delete":
                            group.IsActive = false;
                            affectedCount++;
                            break;
                    }
                }

                if (affectedCount > 0)
                {
                    await _context.SaveChangesAsync();
                }

                return affectedCount;
            }
            catch (Exception ex)
            {
                throw new Exception("Error performing bulk operation on groups", ex);
            }
        }

        public async Task<(List<GroupInfoDto> Groups, int TotalCount)> SearchGroupsAsync(GroupSearchDto searchDto, int pageNumber = 1, int pageSize = 10)
        {
            try
            {
                var query = _context.Groups
                    .Include(g => g.Users)
                    .Include(g => g.TimeActive)
                    .AsQueryable();

                // Apply filters
                if (!string.IsNullOrWhiteSpace(searchDto.SearchTerm))
                {
                    query = query.Where(g =>
                        g.GroupId.Contains(searchDto.SearchTerm) ||
                        g.GroupName.Contains(searchDto.SearchTerm));
                }

                if (searchDto.IsActive.HasValue)
                {
                    query = query.Where(g => g.IsActive == searchDto.IsActive.Value);
                }

                if (!string.IsNullOrWhiteSpace(searchDto.TimeActiveId))
                {
                    query = query.Where(g => g.TimeActiveId == searchDto.TimeActiveId);
                }

                if (searchDto.MinUserCount.HasValue)
                {
                    query = query.Where(g => g.Users.Count >= searchDto.MinUserCount.Value);
                }

                if (searchDto.MaxUserCount.HasValue)
                {
                    query = query.Where(g => g.Users.Count <= searchDto.MaxUserCount.Value);
                }

                // Apply sorting
                switch (searchDto.SortBy?.ToLower())
                {
                    case "groupname":
                        query = searchDto.SortDirection?.ToLower() == "desc"
                            ? query.OrderByDescending(g => g.GroupName)
                            : query.OrderBy(g => g.GroupName);
                        break;
                    case "usercount":
                        query = searchDto.SortDirection?.ToLower() == "desc"
                            ? query.OrderByDescending(g => g.Users.Count)
                            : query.OrderBy(g => g.Users.Count);
                        break;
                    default:
                        query = searchDto.SortDirection?.ToLower() == "desc"
                            ? query.OrderByDescending(g => g.GroupId)
                            : query.OrderBy(g => g.GroupId);
                        break;
                }

                var totalCount = await query.CountAsync();

                var groups = await query
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .Select(g => new GroupInfoDto
                    {
                        GroupId = g.GroupId,
                        GroupName = g.GroupName,
                        TimeActiveId = g.TimeActiveId,
                        IsActive = g.IsActive,
                        UserCount = g.Users.Count,
                        RoleCount = _context.GroupRoles.Count(gr => gr.GroupId == g.GroupId)
                    })
                    .ToListAsync();

                return (groups, totalCount);
            }
            catch (Exception ex)
            {
                throw new Exception("Error searching groups", ex);
            }
        }
    }
}
