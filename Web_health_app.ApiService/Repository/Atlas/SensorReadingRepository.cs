using Web_health_app.ApiService.Entities.NonSQLTable;
using Web_health_app.ApiService.Data;
using MongoDB.Driver;
using MongoDB.Bson;

namespace Web_health_app.ApiService.Repository.Atlas
{
    public class SensorReadingRepository : ISensorReadingRepository
    {
        private readonly AtlasDbContext _context;
        private readonly IMongoCollection<SensorReading> _collection;

        public SensorReadingRepository(AtlasDbContext context)
        {
            _context = context;
            _collection = _context.SensorReadings;
        }

        public async Task<SensorReading> CreateAsync(SensorReading sensorReading)
        {
            sensorReading.Timestamp = DateTimeOffset.UtcNow;
            await _collection.InsertOneAsync(sensorReading);
            return sensorReading;
        }

        public async Task<List<SensorReading>> CreateManyAsync(List<SensorReading> sensorReadings)
        {
            foreach (var reading in sensorReadings)
            {
                if (reading.Timestamp == default)
                    reading.Timestamp = DateTimeOffset.UtcNow;
            }
            await _collection.InsertManyAsync(sensorReadings);
            return sensorReadings;
        }

        public async Task<List<SensorReading>> GetAllAsync()
        {
            return await _collection.Find(Builders<SensorReading>.Filter.Empty).ToListAsync();
        }

        public async Task<SensorReading?> GetByIdAsync(string id)
        {
            var filter = Builders<SensorReading>.Filter.Eq(x => x.Id, id);
            return await _collection.Find(filter).FirstOrDefaultAsync();
        }

        public async Task<List<SensorReading>> GetByUserIdAsync(string userId)
        {
            var filter = Builders<SensorReading>.Filter.Eq("metadata.userId", userId);
            return await _collection.Find(filter).ToListAsync();
        }

        public async Task<List<SensorReading>> GetByDeviceIdAsync(string deviceId)
        {
            var filter = Builders<SensorReading>.Filter.Eq("metadata.deviceId", deviceId);
            return await _collection.Find(filter).ToListAsync();
        }

        public async Task<List<SensorReading>> GetBySensorTypeAsync(string sensorType)
        {
            var filter = Builders<SensorReading>.Filter.Eq("metadata.sensorType", sensorType);
            return await _collection.Find(filter).ToListAsync();
        }

        public async Task<List<SensorReading>> GetByDateRangeAsync(DateTime fromDate, DateTime toDate)
        {
            var builder = Builders<SensorReading>.Filter;
            var filter = builder.And(
                builder.Gte(x => x.Timestamp, new DateTimeOffset(fromDate)),
                builder.Lte(x => x.Timestamp, new DateTimeOffset(toDate))
            );
            return await _collection.Find(filter).ToListAsync();
        }

        public async Task<List<SensorReading>> GetByUserAndDateRangeAsync(string userId, DateTime fromDate, DateTime toDate)
        {
            var builder = Builders<SensorReading>.Filter;
            var filter = builder.And(
                builder.Eq("metadata.userId", userId),
                builder.Gte(x => x.Timestamp, new DateTimeOffset(fromDate)),
                builder.Lte(x => x.Timestamp, new DateTimeOffset(toDate))
            );
            return await _collection.Find(filter).ToListAsync();
        }

        public async Task<List<SensorReading>> GetByDeviceAndDateRangeAsync(string deviceId, DateTime fromDate, DateTime toDate)
        {
            var builder = Builders<SensorReading>.Filter;
            var filter = builder.And(
                builder.Eq("metadata.deviceId", deviceId),
                builder.Gte(x => x.Timestamp, new DateTimeOffset(fromDate)),
                builder.Lte(x => x.Timestamp, new DateTimeOffset(toDate))
            );
            return await _collection.Find(filter).ToListAsync();
        }

        public async Task<List<SensorReading>> GetLatestByUserAsync(string userId, int limit = 10)
        {
            var filter = Builders<SensorReading>.Filter.Eq("metadata.userId", userId);
            return await _collection.Find(filter)
                .Sort(Builders<SensorReading>.Sort.Descending(x => x.Timestamp))
                .Limit(limit)
                .ToListAsync();
        }

        public async Task<List<SensorReading>> GetLatestByDeviceAsync(string deviceId, int limit = 10)
        {
            var filter = Builders<SensorReading>.Filter.Eq("metadata.deviceId", deviceId);
            return await _collection.Find(filter)
                .Sort(Builders<SensorReading>.Sort.Descending(x => x.Timestamp))
                .Limit(limit)
                .ToListAsync();
        }

        public async Task<SensorReading?> GetLatestByUserAndSensorTypeAsync(string userId, string sensorType)
        {
            var builder = Builders<SensorReading>.Filter;
            var filter = builder.And(
                builder.Eq("metadata.userId", userId),
                builder.Eq("metadata.sensorType", sensorType)
            );
            
            return await _collection.Find(filter)
                .Sort(Builders<SensorReading>.Sort.Descending(x => x.Timestamp))
                .FirstOrDefaultAsync();
        }

        public async Task<bool> DeleteAsync(string id)
        {
            var filter = Builders<SensorReading>.Filter.Eq(x => x.Id, id);
            var result = await _collection.DeleteOneAsync(filter);
            return result.DeletedCount > 0;
        }

        public async Task<bool> DeleteByUserIdAsync(string userId)
        {
            var filter = Builders<SensorReading>.Filter.Eq("metadata.userId", userId);
            var result = await _collection.DeleteManyAsync(filter);
            return result.DeletedCount > 0;
        }

        public async Task<bool> DeleteByDeviceIdAsync(string deviceId)
        {
            var filter = Builders<SensorReading>.Filter.Eq("metadata.deviceId", deviceId);
            var result = await _collection.DeleteManyAsync(filter);
            return result.DeletedCount > 0;
        }

        public async Task<long> CountAsync()
        {
            return await _collection.CountDocumentsAsync(Builders<SensorReading>.Filter.Empty);
        }

        public async Task<long> CountByUserIdAsync(string userId)
        {
            var filter = Builders<SensorReading>.Filter.Eq("metadata.userId", userId);
            return await _collection.CountDocumentsAsync(filter);
        }

        public async Task<long> CountByDeviceIdAsync(string deviceId)
        {
            var filter = Builders<SensorReading>.Filter.Eq("metadata.deviceId", deviceId);
            return await _collection.CountDocumentsAsync(filter);
        }

        public async Task<List<SensorReading>> GetPaginatedAsync(int page, int pageSize)
        {
            var skip = (page - 1) * pageSize;
            return await _collection.Find(Builders<SensorReading>.Filter.Empty)
                .Sort(Builders<SensorReading>.Sort.Descending(x => x.Timestamp))
                .Skip(skip)
                .Limit(pageSize)
                .ToListAsync();
        }

        public async Task<Dictionary<string, long>> GetSensorTypeStatisticsAsync()
        {
            var pipeline = new[]
            {
                new BsonDocument("$group", new BsonDocument
                {
                    {"_id", "$metadata.sensorType"},
                    {"count", new BsonDocument("$sum", 1)}
                })
            };

            var result = await _collection.AggregateAsync<BsonDocument>(pipeline);
            var statistics = new Dictionary<string, long>();

            await result.ForEachAsync(doc =>
            {
                var sensorType = doc["_id"].AsString;
                var count = doc["count"].AsInt64;
                statistics[sensorType] = count;
            });

            return statistics;
        }

        public async Task<List<SensorReading>> GetRecentReadingsAsync(int hours = 24)
        {
            var cutoffTime = DateTimeOffset.UtcNow.AddHours(-hours);
            var filter = Builders<SensorReading>.Filter.Gte(x => x.Timestamp, cutoffTime);
            
            return await _collection.Find(filter)
                .Sort(Builders<SensorReading>.Sort.Descending(x => x.Timestamp))
                .ToListAsync();
        }
    }
}
