using Web_health_app.Models.Models;

namespace Web_health_app.ApiService.Repository
{
    public interface IEntityRepository
    {
        Task<(List<EntityInfoDto> Entities, int TotalCount)> GetAllEntitiesAsync(int pageNumber = 1, int pageSize = 10, string? searchTerm = null);
        Task<EntityInfoDto?> GetEntityByIdAsync(string entityId);
        Task<EntityInfoDto?> UpdateEntityAsync(string entityId, UpdateEntityDto updateEntityDto);
        Task<List<EntityInfoDto>> GetEntitiesBySecurityLevelAsync(byte minSecurityLevel);
    }
}
