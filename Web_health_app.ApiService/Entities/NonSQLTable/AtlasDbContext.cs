using MongoDB.Driver;
using MongoDB.Bson;

namespace Web_health_app.ApiService.Entities.NonSQLTable
{
    public class AtlasDbContext : IDisposable
    {
        private readonly IMongoDatabase _database;
        private readonly MongoClient _client;

        public AtlasDbContext(string connectionString, string databaseName = "health_monitor")
        {
            // Set up MongoDB client settings based on your sample code
            var settings = MongoClientSettings.FromConnectionString(connectionString);

            // Set the ServerApi field to set the version of the Stable API on the client
            settings.ServerApi = new ServerApi(ServerApiVersion.V1);

            // Create a new client and connect to the server
            _client = new MongoClient(settings);
            _database = _client.GetDatabase(databaseName);

            // Test connection
            TestConnection();
        }

        private void TestConnection()
        {
            try
            {
                // Send a ping to confirm a successful connection
                var result = _database.RunCommand<BsonDocument>(new BsonDocument("ping", 1));
                Console.WriteLine("Pinged your deployment. You successfully connected to MongoDB!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to connect to MongoDB: {ex.Message}");
                throw;
            }
        }

        // Collections access
        public IMongoCollection<AuditLog> AuditLogs => _database.GetCollection<AuditLog>("audit_logs");
        public IMongoCollection<Device> Devices => _database.GetCollection<Device>("devices");
        public IMongoCollection<SensorReading> SensorReadings => _database.GetCollection<SensorReading>("sensor_readings");
        public IMongoCollection<User> Users => _database.GetCollection<User>("users");

        // Generic CRUD operations
        public async Task<T> InsertOneAsync<T>(IMongoCollection<T> collection, T document)
        {
            await collection.InsertOneAsync(document);
            return document;
        }

        public async Task<List<T>> InsertManyAsync<T>(IMongoCollection<T> collection, IEnumerable<T> documents)
        {
            var documentList = documents.ToList();
            await collection.InsertManyAsync(documentList);
            return documentList;
        }

        public async Task<List<T>> FindAsync<T>(IMongoCollection<T> collection, FilterDefinition<T> filter)
        {
            return await collection.Find(filter).ToListAsync();
        }

        public async Task<T> FindOneAsync<T>(IMongoCollection<T> collection, FilterDefinition<T> filter)
        {
            return await collection.Find(filter).FirstOrDefaultAsync();
        }

        public async Task<T> FindByIdAsync<T>(IMongoCollection<T> collection, string id)
        {
            var filter = Builders<T>.Filter.Eq("_id", ObjectId.Parse(id));
            return await collection.Find(filter).FirstOrDefaultAsync();
        }

        public async Task<ReplaceOneResult> ReplaceOneAsync<T>(IMongoCollection<T> collection, FilterDefinition<T> filter, T document)
        {
            return await collection.ReplaceOneAsync(filter, document);
        }

        public async Task<UpdateResult> UpdateOneAsync<T>(IMongoCollection<T> collection, FilterDefinition<T> filter, UpdateDefinition<T> update)
        {
            return await collection.UpdateOneAsync(filter, update);
        }

        public async Task<DeleteResult> DeleteOneAsync<T>(IMongoCollection<T> collection, FilterDefinition<T> filter)
        {
            return await collection.DeleteOneAsync(filter);
        }

        public async Task<DeleteResult> DeleteManyAsync<T>(IMongoCollection<T> collection, FilterDefinition<T> filter)
        {
            return await collection.DeleteManyAsync(filter);
        }

        public async Task<long> CountDocumentsAsync<T>(IMongoCollection<T> collection, FilterDefinition<T> filter)
        {
            return await collection.CountDocumentsAsync(filter);
        }

        // Specific entity operations
        public async Task<AuditLog> CreateAuditLogAsync(AuditLog auditLog)
        {
            auditLog.EventAt = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
            return await InsertOneAsync(AuditLogs, auditLog);
        }

        public async Task<Device> CreateDeviceAsync(Device device)
        {
            device.RegisteredAt = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
            device.LastSyncAt = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
            return await InsertOneAsync(Devices, device);
        }

        public async Task<SensorReading> CreateSensorReadingAsync(SensorReading sensorReading)
        {
            sensorReading.Timestamp = DateTimeOffset.UtcNow;
            return await InsertOneAsync(SensorReadings, sensorReading);
        }

        public async Task<User> CreateUserAsync(User user)
        {
            user.CreatedAt = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
            user.UpdatedAt = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
            return await InsertOneAsync(Users, user);
        }

        // Filter builders for common queries
        public FilterDefinition<AuditLog> GetAuditLogFilter(string? userId = null, string? action = null, DateTime? fromDate = null, DateTime? toDate = null)
        {
            var builder = Builders<AuditLog>.Filter;
            var filters = new List<FilterDefinition<AuditLog>>();

            if (!string.IsNullOrEmpty(userId))
                filters.Add(builder.Eq(x => x.UserId, userId));

            if (!string.IsNullOrEmpty(action))
                filters.Add(builder.Eq(x => x.Action, action));

            if (fromDate.HasValue)
                filters.Add(builder.Gte(x => x.EventAt, fromDate.Value.ToString("yyyy-MM-ddTHH:mm:ss.fffZ")));

            if (toDate.HasValue)
                filters.Add(builder.Lte(x => x.EventAt, toDate.Value.ToString("yyyy-MM-ddTHH:mm:ss.fffZ")));

            return filters.Count > 0 ? builder.And(filters) : builder.Empty;
        }

        public FilterDefinition<Device> GetDeviceFilter(string? ownerId = null, string? status = null)
        {
            var builder = Builders<Device>.Filter;
            var filters = new List<FilterDefinition<Device>>();

            if (!string.IsNullOrEmpty(ownerId))
                filters.Add(builder.Eq(x => x.OwnerId, ownerId));

            if (!string.IsNullOrEmpty(status))
                filters.Add(builder.Eq(x => x.Status, status));

            return filters.Count > 0 ? builder.And(filters) : builder.Empty;
        }

        // Dispose
        public void Dispose()
        {
            // MongoDB client handles connection pooling, usually no need to explicitly dispose
        }
    }
}
