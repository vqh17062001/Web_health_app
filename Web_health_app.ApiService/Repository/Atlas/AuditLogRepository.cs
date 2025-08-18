using Web_health_app.ApiService.Entities.NonSQLTable;
using Web_health_app.ApiService.Data;
using MongoDB.Driver;
using MongoDB.Bson;

namespace Web_health_app.ApiService.Repository.Atlas
{
    public class AuditLogRepository : IAuditLogRepository
    {
        private readonly AtlasDbContext _context;
        private readonly IMongoCollection<AuditLog> _collection;

        public AuditLogRepository(AtlasDbContext context)
        {
            _context = context;
            _collection = _context.AuditLogs;
        }

        public async Task<AuditLog> CreateAsync(AuditLog auditLog)
        {
            auditLog.EventAt = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
            await _collection.InsertOneAsync(auditLog);
            return auditLog;
        }

        public async Task<List<AuditLog>> GetAllAsync()
        {
            return await _collection.Find(Builders<AuditLog>.Filter.Empty).ToListAsync();
        }

        public async Task<AuditLog?> GetByIdAsync(string id)
        {
            var filter = Builders<AuditLog>.Filter.Eq(x => x.Id, id);
            return await _collection.Find(filter).FirstOrDefaultAsync();
        }

        public async Task<List<AuditLog>> GetByUserIdAsync(string userId)
        {
            var filter = Builders<AuditLog>.Filter.Eq(x => x.UserId, userId);
            return await _collection.Find(filter).ToListAsync();
        }

        public async Task<List<AuditLog>> GetByActionAsync(string action)
        {
            var filter = Builders<AuditLog>.Filter.Eq(x => x.Action, action);
            return await _collection.Find(filter).ToListAsync();
        }

        public async Task<List<AuditLog>> GetByResourceAsync(string resource)
        {
            var filter = Builders<AuditLog>.Filter.Eq(x => x.Resource, resource);
            return await _collection.Find(filter).ToListAsync();
        }

        public async Task<List<AuditLog>> GetByDateRangeAsync(DateTime fromDate, DateTime toDate)
        {
            var builder = Builders<AuditLog>.Filter;
            var filter = builder.And(
                builder.Gte(x => x.EventAt, fromDate.ToString("yyyy-MM-ddTHH:mm:ss.fffZ")),
                builder.Lte(x => x.EventAt, toDate.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"))
            );
            return await _collection.Find(filter).ToListAsync();
        }

        public async Task<List<AuditLog>> GetByFilterAsync(string? userId = null, string? action = null, string? resource = null, DateTime? fromDate = null, DateTime? toDate = null)
        {
            var builder = Builders<AuditLog>.Filter;
            var filters = new List<FilterDefinition<AuditLog>>();

            if (!string.IsNullOrEmpty(userId))
                filters.Add(builder.Eq(x => x.UserId, userId));

            if (!string.IsNullOrEmpty(action))
                filters.Add(builder.Eq(x => x.Action, action));

            if (!string.IsNullOrEmpty(resource))
                filters.Add(builder.Eq(x => x.Resource, resource));

            if (fromDate.HasValue)
                filters.Add(builder.Gte(x => x.EventAt, fromDate.Value.ToString("yyyy-MM-ddTHH:mm:ss.fffZ")));

            if (toDate.HasValue)
                filters.Add(builder.Lte(x => x.EventAt, toDate.Value.ToString("yyyy-MM-ddTHH:mm:ss.fffZ")));

            var filter = filters.Count > 0 ? builder.And(filters) : builder.Empty;
            return await _collection.Find(filter).ToListAsync();
        }

        public async Task<bool> DeleteAsync(string id)
        {
            var filter = Builders<AuditLog>.Filter.Eq(x => x.Id, id);
            var result = await _collection.DeleteOneAsync(filter);
            return result.DeletedCount > 0;
        }

        public async Task<long> CountAsync()
        {
            return await _collection.CountDocumentsAsync(Builders<AuditLog>.Filter.Empty);
        }

        public async Task<List<AuditLog>> GetPaginatedAsync(int page, int pageSize)
        {
            var skip = (page - 1) * pageSize;
            return await _collection.Find(Builders<AuditLog>.Filter.Empty)
                .Sort(Builders<AuditLog>.Sort.Descending(x => x.EventAt))
                .Skip(skip)
                .Limit(pageSize)
                .ToListAsync();
        }
    }
}
