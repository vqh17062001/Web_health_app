using Microsoft.EntityFrameworkCore;
using Web_health_app.ApiService.Entities;
using Web_health_app.Models.Models;

namespace Web_health_app.ApiService.Repository
{
    public class EntityRepository : IEntityRepository
    {
        private readonly HealthDbContext _context;

        public EntityRepository(HealthDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<(List<Models.Models.EntityInfoDto> Entities, int TotalCount)> GetAllEntitiesAsync(int pageNumber = 1, int pageSize = 10, string? searchTerm = null)
        {
            try
            {
                var query = _context.Entities.AsQueryable();

                // Apply search filter
                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    query = query.Where(e =>
                        e.EntityId.Contains(searchTerm) ||
                        e.NameEntity.Contains(searchTerm) ||
                        (e.Type != null && e.Type.Contains(searchTerm)));
                }

                var totalCount = await query.CountAsync();

                var entities = await query
                    .OrderBy(e => e.EntityId)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .Select(e => new Models.Models.EntityInfoDto
                    {
                        EntityId = e.EntityId,
                        NameEntity = e.NameEntity,
                        LevelSecurity = e.LevelSecurity,
                        Type = e.Type
                    })
                    .ToListAsync();

                return (entities, totalCount);
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving entities", ex);
            }
        }

        public async Task<Models.Models.EntityInfoDto?> GetEntityByIdAsync(string entityId)
        {
            try
            {
                var entity = await _context.Entities
                    .Where(e => e.EntityId == entityId)
                    .Select(e => new Models.Models.EntityInfoDto
                    {
                        EntityId = e.EntityId,
                        NameEntity = e.NameEntity,
                        LevelSecurity = e.LevelSecurity,
                        Type = e.Type
                    })
                    .FirstOrDefaultAsync();

                return entity;
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving entity by ID", ex);
            }
        }

        public async Task<Models.Models.EntityInfoDto?> UpdateEntityAsync(string entityId, UpdateEntityDto updateEntityDto)
        {
            try
            {
                var entity = await _context.Entities.FirstOrDefaultAsync(e => e.EntityId == entityId);

                if (entity == null)
                {
                    return null;
                }



                if (updateEntityDto.LevelSecurity.HasValue)
                {
                    entity.LevelSecurity = updateEntityDto.LevelSecurity.Value;
                }



                await _context.SaveChangesAsync();

                return new Models.Models.EntityInfoDto
                {
                    EntityId = entity.EntityId,
                    NameEntity = entity.NameEntity,
                    LevelSecurity = entity.LevelSecurity,
                    Type = entity.Type
                };
            }
            catch (Exception ex)
            {
                throw new Exception("Error updating entity", ex);
            }
        }

        public async Task<List<Models.Models.EntityInfoDto>> GetEntitiesBySecurityLevelAsync(byte minSecurityLevel)
        {
            try
            {
                var entities = await _context.Entities
                    .Where(e => e.LevelSecurity <= minSecurityLevel)
                    .Select(e => new Models.Models.EntityInfoDto
                    {
                        EntityId = e.EntityId,
                        NameEntity = e.NameEntity,
                        LevelSecurity = e.LevelSecurity,
                        Type = e.Type
                    })
                    .OrderBy(e => e.LevelSecurity)
                    .ThenBy(e => e.NameEntity)
                    .ToListAsync();

                return entities;
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving entities by security level", ex);
            }
        }
    }
}
