using Web_health_app.ApiService.Entities.NonSQLTable;
using MongoDB.Driver;

namespace Web_health_app.ApiService.Repository.Atlas
{
    public interface IDeviceRepository
    {
        Task<Device> CreateAsync(Device device);
        Task<List<Device>> GetAllAsync();
        Task<Device?> GetByIdAsync(string id);
        Task<Device?> GetByDeviceIdAsync(string deviceId);
        Task<List<Device>> GetByOwnerIdAsync(string ownerId);
        Task<List<Device>> GetByStatusAsync(string status);
        Task<List<Device>> GetByModelAsync(string model);
        Task<Device> UpdateAsync(Device device);
        Task<bool> UpdateStatusAsync(string id, string status);
        Task<bool> UpdateLastSyncAsync(string id);
        Task<bool> DeleteAsync(string id);
        Task<long> CountAsync();
        Task<long> CountByStatusAsync(string status);
        Task<List<Device>> GetPaginatedAsync(int page, int pageSize);
        Task<List<Device>> SearchAsync(string searchTerm);
    }
}
