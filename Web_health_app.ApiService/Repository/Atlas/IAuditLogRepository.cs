using Web_health_app.ApiService.Entities.NonSQLTable;
using MongoDB.Driver;

namespace Web_health_app.ApiService.Repository.Atlas
{
    public interface IAuditLogRepository
    {
        Task<AuditLog> CreateAsync(AuditLog auditLog);
        Task<List<AuditLog>> GetAllAsync();
        Task<AuditLog?> GetByIdAsync(string id);
        Task<List<AuditLog>> GetByUserIdAsync(string userId);
        Task<List<AuditLog>> GetByActionAsync(string action);
        Task<List<AuditLog>> GetByResourceAsync(string resource);
        Task<List<AuditLog>> GetByDateRangeAsync(DateTime fromDate, DateTime toDate);
        Task<List<AuditLog>> GetByFilterAsync(string? userId = null, string? action = null, string? resource = null, DateTime? fromDate = null, DateTime? toDate = null);
        Task<bool> DeleteAsync(string id);
        Task<long> CountAsync();
        Task<List<AuditLog>> GetPaginatedAsync(int page, int pageSize);
    }
}
