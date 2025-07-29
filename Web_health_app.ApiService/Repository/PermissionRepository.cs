using Microsoft.EntityFrameworkCore;
using System;
using Web_health_app.ApiService.Entities;
using Web_health_app.Models.Models;

namespace Web_health_app.ApiService.Repository
{
    public class PermissionRepository : IPermissionRepository
    {
        private readonly HealthDbContext _context;

        public PermissionRepository(HealthDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<(List<PermissionInfoDto> Permissions, int TotalCount)> GetAllPermissionsAsync(int pageNumber = 1, int pageSize = 10, string? searchTerm = null, bool includeInactive = false)
        {
            try
            {
                var query = _context.Permissions
                    .Include(p => p.Action)
                    .Include(p => p.Entity)
                    .Include(p => p.Role)
                    .Include(p => p.TimeActive)
                    .AsQueryable();

                // Filter by active status
                if (!includeInactive)
                {
                    query = query.Where(p => p.IsActive);
                }

                // Apply search filter
                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    query = query.Where(p =>
                        p.PermissionId.Contains(searchTerm) ||
                        p.PermissionName.Contains(searchTerm) ||
                        p.Action.ActionName.Contains(searchTerm) ||
                        p.Entity.NameEntity.Contains(searchTerm) ||
                        (p.Role != null && p.Role.RoleName.Contains(searchTerm)));
                }

                var totalCount = await query.CountAsync();

                var permissions = await query
                    .OrderBy(p => p.PermissionId)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .Select(p => new PermissionInfoDto
                    {
                        PermissionId = p.PermissionId,
                        PermissionName = p.PermissionName,
                        ActionId = p.ActionId,
                        ActionName = p.Action.ActionName,
                        EntityId = p.EntityId,
                        EntityName = p.Entity.NameEntity,
                        TimeActiveId = p.TimeActiveId,
                        RoleId = p.RoleId,
                        RoleName = p.Role != null ? p.Role.RoleName : null,
                        IsActive = p.IsActive
                    })
                    .ToListAsync();

                return (permissions, totalCount);
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving permissions", ex);
            }
        }

        public async Task<PermissionInfoDto?> GetPermissionByIdAsync(string permissionId)
        {
            try
            {
                var permission = await _context.Permissions
                    .Include(p => p.Action)
                    .Include(p => p.Entity)
                    .Include(p => p.Role)
                    .Include(p => p.TimeActive)
                    .Where(p => p.PermissionId == permissionId)
                    .Select(p => new PermissionInfoDto
                    {
                        PermissionId = p.PermissionId,
                        PermissionName = p.PermissionName,
                        ActionId = p.ActionId,
                        ActionName = p.Action.ActionName,
                        EntityId = p.EntityId,
                        EntityName = p.Entity.NameEntity,
                        TimeActiveId = p.TimeActiveId,
                        RoleId = p.RoleId,
                        RoleName = p.Role != null ? p.Role.RoleName : null,
                        IsActive = p.IsActive
                    })
                    .FirstOrDefaultAsync();

                return permission;
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving permission by ID", ex);
            }
        }

        public async Task<PermissionInfoDto> CreatePermissionAsync(CreatePermissionDto createPermissionDto)
        {
            try
            {

                createPermissionDto.PermissionId = (createPermissionDto.ActionId ?? string.Empty) + "_" + (createPermissionDto.EntityId ?? string.Empty) + "_" + (createPermissionDto.RoleId ?? string.Empty);

                // Check if permission ID already exists
                if (await PermissionIdExistsAsync(createPermissionDto.PermissionId))
                {
                    throw new InvalidOperationException("Permission ID already exists");
                }

                // Validate Action exists
                var actionExists = _context.Actions.Where(a => a.ActionId == createPermissionDto.ActionId && a.IsActive).FirstOrDefault();
                var entityExists = _context.Entities.Where(e => e.EntityId == createPermissionDto.EntityId).FirstOrDefault();
                var roleExists = _context.Roles.Where(r => r.RoleId == createPermissionDto.RoleId && r.IsActive).FirstOrDefault();


                if (actionExists != null && entityExists != null&& roleExists != null)
                {
                    createPermissionDto.PermissionName = actionExists.ActionName + " " + entityExists.NameEntity;
                }

              

                
             

                // Validate TimeActive exists if provided
                if (!string.IsNullOrEmpty(createPermissionDto.TimeActiveId))
                {
                    var timeActiveExists = await _context.TimeActives.AnyAsync(t => t.TimeActiveId == createPermissionDto.TimeActiveId);
                    if (!timeActiveExists)
                    {
                        throw new InvalidOperationException("Time Active does not exist");
                    }
                }

                var permission = new Permission
                {
                    PermissionId = createPermissionDto.PermissionId,
                    PermissionName = createPermissionDto.PermissionName,
                    ActionId = createPermissionDto.ActionId,
                    EntityId = createPermissionDto.EntityId,
                    TimeActiveId = createPermissionDto.TimeActiveId,
                    RoleId = createPermissionDto.RoleId,
                    IsActive = createPermissionDto.IsActive
                };

                _context.Permissions.Add(permission);
                await _context.SaveChangesAsync();

                return await GetPermissionByIdAsync(permission.PermissionId) ??
                       throw new InvalidOperationException("Failed to retrieve created permission");
            }
            catch (Exception ex)
            {
                throw new Exception("Error creating permission", ex);
            }
        }

        public async Task<PermissionInfoDto?> UpdatePermissionAsync(string permissionId, UpdatePermissionDto updatePermissionDto)
        {
            try
            {
                var permission = await _context.Permissions.FirstOrDefaultAsync(p => p.PermissionId == permissionId);
                if (permission == null)
                {
                    return null;
                }

                // Update only provided fields
                if (!string.IsNullOrEmpty(updatePermissionDto.PermissionName))
                    permission.PermissionName = updatePermissionDto.PermissionName;

                if (!string.IsNullOrEmpty(updatePermissionDto.ActionId))
                {
                    // Validate Action exists
                    var actionExists = await _context.Actions.AnyAsync(a => a.ActionId == updatePermissionDto.ActionId && a.IsActive);
                    if (!actionExists)
                    {
                        throw new InvalidOperationException("Action does not exist or is inactive");
                    }
                    permission.ActionId = updatePermissionDto.ActionId;
                }

                if (!string.IsNullOrEmpty(updatePermissionDto.EntityId))
                {
                    // Validate Entity exists
                    var entityExists = await _context.Entities.AnyAsync(e => e.EntityId == updatePermissionDto.EntityId);
                    if (!entityExists)
                    {
                        throw new InvalidOperationException("Entity does not exist");
                    }
                    permission.EntityId = updatePermissionDto.EntityId;
                }

                if (updatePermissionDto.TimeActiveId != null)
                {
                    if (!string.IsNullOrEmpty(updatePermissionDto.TimeActiveId))
                    {
                        var timeActiveExists = await _context.TimeActives.AnyAsync(t => t.TimeActiveId == updatePermissionDto.TimeActiveId);
                        if (!timeActiveExists)
                        {
                            throw new InvalidOperationException("Time Active does not exist");
                        }
                    }
                    permission.TimeActiveId = updatePermissionDto.TimeActiveId;
                }

                if (updatePermissionDto.RoleId != null)
                {
                    if (!string.IsNullOrEmpty(updatePermissionDto.RoleId))
                    {
                        var roleExists = await _context.Roles.AnyAsync(r => r.RoleId == updatePermissionDto.RoleId && r.IsActive);
                        if (!roleExists)
                        {
                            throw new InvalidOperationException("Role does not exist or is inactive");
                        }
                    }
                    permission.RoleId = updatePermissionDto.RoleId;
                }

                if (updatePermissionDto.IsActive.HasValue)
                    permission.IsActive = updatePermissionDto.IsActive.Value;

                await _context.SaveChangesAsync();

                return await GetPermissionByIdAsync(permissionId);
            }
            catch (Exception ex)
            {
                throw new Exception("Error updating permission", ex);
            }
        }

        public async Task<bool> DeletePermissionAsync(string permissionId)
        {
            try
            {
                var permission = await _context.Permissions.FirstOrDefaultAsync(p => p.PermissionId == permissionId);
                if (permission == null)
                {
                    return false;
                }

                // Soft delete by setting IsActive to false
                permission.IsActive = false;
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception("Error deleting permission", ex);
            }
        }

        public async Task<bool> HardDeletePermissionAsync(string permissionId)
        {
            try
            {
                var permission = await _context.Permissions.FirstOrDefaultAsync(p => p.PermissionId == permissionId);
                if (permission == null)
                {
                    return false;
                }

                _context.Permissions.Remove(permission);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception("Error permanently deleting permission", ex);
            }
        }

        public async Task<bool> PermissionIdExistsAsync(string permissionId, string? excludePermissionId = null)
        {
            try
            {
                var query = _context.Permissions.Where(p => p.PermissionId == permissionId);

                if (!string.IsNullOrEmpty(excludePermissionId))
                {
                    query = query.Where(p => p.PermissionId != excludePermissionId);
                }

                return await query.AnyAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error checking permission ID existence", ex);
            }
        }

        public async Task<List<PermissionInfoDto>> GetPermissionsByActionAsync(string actionId)
        {
            try
            {
                var permissions = await _context.Permissions
                    .Include(p => p.Action)
                    .Include(p => p.Entity)
                    .Include(p => p.Role)
                    .Where(p => p.ActionId == actionId && p.IsActive)
                    .Select(p => new PermissionInfoDto
                    {
                        PermissionId = p.PermissionId,
                        PermissionName = p.PermissionName,
                        ActionId = p.ActionId,
                        ActionName = p.Action.ActionName,
                        EntityId = p.EntityId,
                        EntityName = p.Entity.NameEntity,
                        TimeActiveId = p.TimeActiveId,
                        RoleId = p.RoleId,
                        RoleName = p.Role != null ? p.Role.RoleName : null,
                        IsActive = p.IsActive
                    })
                    .OrderBy(p => p.PermissionName)
                    .ToListAsync();

                return permissions;
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving permissions by action", ex);
            }
        }

        public async Task<List<PermissionInfoDto>> GetPermissionsByEntityAsync(string entityId)
        {
            try
            {
                var permissions = await _context.Permissions
                    .Include(p => p.Action)
                    .Include(p => p.Entity)
                    .Include(p => p.Role)
                    .Where(p => p.EntityId == entityId && p.IsActive)
                    .Select(p => new PermissionInfoDto
                    {
                        PermissionId = p.PermissionId,
                        PermissionName = p.PermissionName,
                        ActionId = p.ActionId,
                        ActionName = p.Action.ActionName,
                        EntityId = p.EntityId,
                        EntityName = p.Entity.NameEntity,
                        TimeActiveId = p.TimeActiveId,
                        RoleId = p.RoleId,
                        RoleName = p.Role != null ? p.Role.RoleName : null,
                        IsActive = p.IsActive
                    })
                    .OrderBy(p => p.PermissionName)
                    .ToListAsync();

                return permissions;
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving permissions by entity", ex);
            }
        }

        public async Task<List<PermissionInfoDto>> GetPermissionsByRoleAsync(string roleId)
        {
            try
            {
                var permissions = await _context.Permissions
                    .Include(p => p.Action)
                    .Include(p => p.Entity)
                    .Include(p => p.Role)
                    .Where(p => p.RoleId == roleId && p.IsActive)
                    .Select(p => new PermissionInfoDto
                    {
                        PermissionId = p.PermissionId,
                        PermissionName = p.PermissionName,
                        ActionId = p.ActionId,
                        ActionName = p.Action.ActionName,
                        EntityId = p.EntityId,
                        EntityName = p.Entity.NameEntity,
                        TimeActiveId = p.TimeActiveId,
                        RoleId = p.RoleId,
                        RoleName = p.Role != null ? p.Role.RoleName : null,
                        IsActive = p.IsActive
                    })
                    .OrderBy(p => p.PermissionName)
                    .ToListAsync();

                return permissions;
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving permissions by role", ex);
            }
        }

        public async Task<List<ActionInfoDto>> GetAvailableActionsAsync()
        {
            try
            {
                var actions = await _context.Actions
                    .Where(a => a.IsActive)
                    .Select(a => new ActionInfoDto
                    {
                        ActionId = a.ActionId,
                        ActionName = a.ActionName,
                        Code = a.Code,
                        IsActive = a.IsActive
                    })
                    .OrderBy(a => a.ActionName)
                    .ToListAsync();

                return actions;
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving available actions", ex);
            }
        }

        public async Task<List<EntityInfoDto>> GetAvailableEntitiesAsync()
        {
            try
            {
                var entities = await _context.Entities
                    .Select(e => new EntityInfoDto
                    {
                        EntityId = e.EntityId,
                        NameEntity = e.NameEntity,
                        LevelSecurity = e.LevelSecurity,
                        Type = e.Type,
                        // Since Entity model doesn't have IsActive property
                    })
                    .OrderBy(e => e.NameEntity)
                    .ToListAsync();

                return entities;
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving available entities", ex);
            }
        }
    }
}
