using Web_health_app.ApiService.Entities.NonSQLTable;
using MongoDB.Driver;
using MongoDB.Bson;

namespace Web_health_app.ApiService.Repository.Atlas
{
    public class DeviceRepository : IDeviceRepository
    {
        private readonly AtlasDbContext _context;
        private readonly IMongoCollection<Device> _collection;

        public DeviceRepository(AtlasDbContext context)
        {
            _context = context;
            _collection = _context.Devices;
        }

        public async Task<Device> CreateAsync(Device device)
        {
            device.RegisteredAt = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
            device.LastSyncAt = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
            device.Status = "online";
            await _collection.InsertOneAsync(device);
            return device;
        }

        public async Task<List<Device>> GetAllAsync()
        {
            return await _collection.Find(Builders<Device>.Filter.Empty).ToListAsync();
        }

        public async Task<Device?> GetByIdAsync(string id)
        {
            var filter = Builders<Device>.Filter.Eq(x => x.Id, id);
            return await _collection.Find(filter).FirstOrDefaultAsync();
        }

        public async Task<Device?> GetByDeviceIdAsync(string deviceId)
        {
            var filter = Builders<Device>.Filter.Eq(x => x.DeviceId, deviceId);
            return await _collection.Find(filter).FirstOrDefaultAsync();
        }

        public async Task<List<Device>> GetByOwnerIdAsync(string ownerId)
        {
            var filter = Builders<Device>.Filter.Eq(x => x.OwnerId, ownerId);
            return await _collection.Find(filter).ToListAsync();
        }

        public async Task<List<Device>> GetByStatusAsync(string status)
        {
            var filter = Builders<Device>.Filter.Eq(x => x.Status, status);
            return await _collection.Find(filter).ToListAsync();
        }

        public async Task<List<Device>> GetByModelAsync(string model)
        {
            var filter = Builders<Device>.Filter.Eq(x => x.Model, model);
            return await _collection.Find(filter).ToListAsync();
        }

        public async Task<Device> UpdateAsync(Device device)
        {
            device.LastSyncAt = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
            var filter = Builders<Device>.Filter.Eq(x => x.Id, device.Id);
            await _collection.ReplaceOneAsync(filter, device);
            return device;
        }

        public async Task<bool> UpdateStatusAsync(string id, string status)
        {
            var filter = Builders<Device>.Filter.Eq(x => x.Id, id);
            var update = Builders<Device>.Update
                .Set(x => x.Status, status)
                .Set(x => x.LastSyncAt, DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"));
            
            var result = await _collection.UpdateOneAsync(filter, update);
            return result.ModifiedCount > 0;
        }

        public async Task<bool> UpdateLastSyncAsync(string id)
        {
            var filter = Builders<Device>.Filter.Eq(x => x.Id, id);
            var update = Builders<Device>.Update
                .Set(x => x.LastSyncAt, DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"));
            
            var result = await _collection.UpdateOneAsync(filter, update);
            return result.ModifiedCount > 0;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            var filter = Builders<Device>.Filter.Eq(x => x.Id, id);
            var result = await _collection.DeleteOneAsync(filter);
            return result.DeletedCount > 0;
        }

        public async Task<long> CountAsync()
        {
            return await _collection.CountDocumentsAsync(Builders<Device>.Filter.Empty);
        }

        public async Task<long> CountByStatusAsync(string status)
        {
            var filter = Builders<Device>.Filter.Eq(x => x.Status, status);
            return await _collection.CountDocumentsAsync(filter);
        }

        public async Task<List<Device>> GetPaginatedAsync(int page, int pageSize)
        {
            var skip = (page - 1) * pageSize;
            return await _collection.Find(Builders<Device>.Filter.Empty)
                .Sort(Builders<Device>.Sort.Descending(x => x.RegisteredAt))
                .Skip(skip)
                .Limit(pageSize)
                .ToListAsync();
        }

        public async Task<List<Device>> SearchAsync(string searchTerm)
        {
            var builder = Builders<Device>.Filter;
            var filter = builder.Or(
                builder.Regex(x => x.DeviceId, new BsonRegularExpression(searchTerm, "i")),
                builder.Regex(x => x.Model, new BsonRegularExpression(searchTerm, "i")),
                builder.Regex(x => x.OsVersion, new BsonRegularExpression(searchTerm, "i"))
            );
            
            return await _collection.Find(filter).ToListAsync();
        }
    }
}
