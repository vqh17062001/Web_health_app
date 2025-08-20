using Web_health_app.ApiService.Entities.NonSQLTable;
using Web_health_app.Models.Models.NonSqlDTO;
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

        private SensorReadingInfoDto ConvertToDto(SensorReading sensorReading)
        {
            return new SensorReadingInfoDto
            {
                Id = sensorReading.Id,
                Timestamp = sensorReading.Timestamp,
                Metadata = new MetadataDto
                {
                    UserId = sensorReading.Metadata?.UserId ?? string.Empty,
                    DeviceId = sensorReading.Metadata?.DeviceId ?? string.Empty,
                    SensorType = sensorReading.Metadata?.SensorType
                },
                Readings = sensorReading.Readings?.Select(r => new ReadingDto
                {
                    Key = r.Key,
                    Value = r.Value?.ToString()
                }).ToList() ?? new List<ReadingDto>()
            };
        }

        private List<SensorReadingInfoDto> ConvertToDtoList(List<SensorReading> sensorReadings)
        {
            return sensorReadings.Select(ConvertToDto).ToList();
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

        public async Task<List<SensorReadingInfoDto>> GetAllAsync()
        {
            try
            {
                var sensorReadings = await _collection.Find(Builders<SensorReading>.Filter.Empty).ToListAsync();
                return ConvertToDtoList(sensorReadings);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching all sensor readings: {ex.Message}");
                return new List<SensorReadingInfoDto>();
            }
        }

        public async Task<SensorReadingInfoDto?> GetByIdAsync(string id)
        {
            var filter = Builders<SensorReading>.Filter.Eq(x => x.Id, id);
            var sensorReading = await _collection.Find(filter).FirstOrDefaultAsync();
            return sensorReading != null ? ConvertToDto(sensorReading) : null;
        }

        public async Task<List<SensorReadingInfoDto>> GetByUserIdAsync(string userId)
        {
            var filter = Builders<SensorReading>.Filter
            .Eq(x => x.Metadata.UserId, userId);
            var sensorReadings = await _collection.Find(filter).ToListAsync();
            return ConvertToDtoList(sensorReadings);
        }

        public async Task<List<SensorReadingInfoDto>> GetByDeviceIdAsync(string deviceId, int page, int pageSize)
        {
            var skip = (page - 1) * pageSize;

            var filter = Builders<SensorReading>.Filter.Eq("metadata.deviceId", deviceId);
            var sensorReadings = await _collection.Find(filter)
                .Skip(skip)
                .Limit(pageSize)
                .ToListAsync();
            return ConvertToDtoList(sensorReadings);
        }

        public async Task<List<SensorReadingInfoDto>> GetBySensorTypeAsync(string sensorType, int page, int pageSize)
        {
            var skip = (page - 1) * pageSize;
            var filter = Builders<SensorReading>.Filter.Eq("metadata.sensorType", sensorType);
            var sensorReadings = await _collection.Find(filter)
                .Skip(skip)
                .Limit(pageSize)
                .ToListAsync();
            return ConvertToDtoList(sensorReadings);
        }

        public async Task<List<SensorReadingInfoDto>> GetByDateRangeAsync(DateTime fromDate, DateTime toDate)
        {
            var builder = Builders<SensorReading>.Filter;
            var filter = builder.And(
                builder.Gte(x => x.Timestamp, new DateTimeOffset(fromDate)),
                builder.Lte(x => x.Timestamp, new DateTimeOffset(toDate))
            );
            var sensorReadings = await _collection.Find(filter).ToListAsync();
            return ConvertToDtoList(sensorReadings);
        }

        public async Task<List<SensorReadingInfoDto>> GetByUserAndDateRangeAsync(string userId, DateTime fromDate, DateTime toDate)
        {
            var builder = Builders<SensorReading>.Filter;
            var filter = builder.And(
                builder.Eq("metadata.userId", userId),
                builder.Gte(x => x.Timestamp, new DateTimeOffset(fromDate)),
                builder.Lte(x => x.Timestamp, new DateTimeOffset(toDate))
            );
            var sensorReadings = await _collection.Find(filter).ToListAsync();
            return ConvertToDtoList(sensorReadings);
        }

        public async Task<List<SensorReadingInfoDto>> GetByDeviceAndDateRangeAsync(string deviceId, DateTime fromDate, DateTime toDate)
        {
            var builder = Builders<SensorReading>.Filter;
            var filter = builder.And(
                builder.Eq("metadata.deviceId", deviceId),
                builder.Gte(x => x.Timestamp, new DateTimeOffset(fromDate)),
                builder.Lte(x => x.Timestamp, new DateTimeOffset(toDate))
            );
            var sensorReadings = await _collection.Find(filter).ToListAsync();
            return ConvertToDtoList(sensorReadings);
        }

        public async Task<List<SensorReadingInfoDto>> GetLatestByUserAsync(string userId, int limit = 10, string sensorType = null)
        {
            var filter = Builders<SensorReading>.Filter
            .Eq(x => x.Metadata.UserId, userId);
            if (sensorType != null) {

                filter = Builders<SensorReading>.Filter.Eq("metadata.sensorType", sensorType);
            }
            var sensorReadings = await _collection.Find(filter)
                .Sort(Builders<SensorReading>.Sort.Descending(x => x.Timestamp))
                .Limit(limit)
                .ToListAsync();
            return ConvertToDtoList(sensorReadings);
        }

        public async Task<List<SensorReadingInfoDto>> GetLatestByDeviceAsync(string deviceId, int limit = 10)
        {
            var filter = Builders<SensorReading>.Filter.Eq("metadata.deviceId", deviceId);
            var sensorReadings = await _collection.Find(filter)
                .Sort(Builders<SensorReading>.Sort.Descending(x => x.Timestamp))
                .Limit(limit)
                .ToListAsync();
            return ConvertToDtoList(sensorReadings);
        }

        public async Task<SensorReadingInfoDto?> GetLatestByUserAndSensorTypeAsync(string userId, string sensorType)
        {
            var builder = Builders<SensorReading>.Filter;
            var filter = builder.And(
                builder.Eq("metadata.userId", userId),
                builder.Eq("metadata.sensorType", sensorType)
            );

            var sensorReading = await _collection.Find(filter)
                .Sort(Builders<SensorReading>.Sort.Descending(x => x.Timestamp))
                .FirstOrDefaultAsync();
            return sensorReading != null ? ConvertToDto(sensorReading) : null;
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

        public async Task<SensorReadingListDto> GetPaginatedAsync(int page, int pageSize)
        {
            var skip = (page - 1) * pageSize;
            var totalCount = await _collection.CountDocumentsAsync(Builders<SensorReading>.Filter.Empty);
            var sensorReadings = await _collection.Find(Builders<SensorReading>.Filter.Empty)
                .Sort(Builders<SensorReading>.Sort.Descending(x => x.Timestamp))
                .Skip(skip)
                .Limit(pageSize)
                .ToListAsync();

            return new SensorReadingListDto
            {
                SensorReadings = ConvertToDtoList(sensorReadings),
                TotalCount = (int)totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<SensorStatisticsDto> GetSensorTypeStatisticsAsync()
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
                var count = doc["count"].AsInt32;
                statistics[sensorType] = count;
            });

            var totalCount = await _collection.CountDocumentsAsync(Builders<SensorReading>.Filter.Empty);

            return new SensorStatisticsDto
            {
                SensorTypeCounts = statistics,
                TotalReadings = totalCount,
                LastUpdated = DateTime.UtcNow
            };
        }

        public async Task<List<SensorReadingInfoDto>> GetRecentReadingsAsync(int hours = 24)
        {
            var cutoff = DateTime.UtcNow.AddHours(-hours);
            var filter = Builders<SensorReading>.Filter.Gte("timestamp", cutoff);

            var sensorReadings = await _collection.Find(filter)
                .Sort(Builders<SensorReading>.Sort.Descending(x => x.Timestamp))
                .ToListAsync();
            return ConvertToDtoList(sensorReadings);
        }
    }
}
