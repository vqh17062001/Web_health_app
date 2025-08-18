using Web_health_app.ApiService.Entities.NonSQLTable;
using Web_health_app.ApiService.Data;
using MongoDB.Driver;
using MongoDB.Bson;

namespace Web_health_app.ApiService.Repository.Atlas
{
    public class UserAtlasRepository : IUserAtlasRepository
    {
        private readonly AtlasDbContext _context;
        private readonly IMongoCollection<User> _collection;

        public UserAtlasRepository(AtlasDbContext context)
        {
            _context = context;
            _collection = _context.Users;
        }

        public async Task<User> CreateAsync(User user)
        {
            user.CreatedAt = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
            user.UpdatedAt = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
            await _collection.InsertOneAsync(user);
            return user;
        }

        public async Task<List<User>> GetAllAsync()
        {
            return await _collection.Find(Builders<User>.Filter.Empty).ToListAsync();
        }

        public async Task<User?> GetByIdAsync(string id)
        {
            var filter = Builders<User>.Filter.Eq(x => x.Id, id);
            return await _collection.Find(filter).FirstOrDefaultAsync();
        }

        public async Task<User?> GetByUsernameAsync(string username)
        {
            var filter = Builders<User>.Filter.Eq(x => x.Username, username);
            return await _collection.Find(filter).FirstOrDefaultAsync();
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            var filter = Builders<User>.Filter.Eq(x => x.Email, email);
            return await _collection.Find(filter).FirstOrDefaultAsync();
        }

        public async Task<List<User>> GetByRoleAsync(string role)
        {
            var filter = Builders<User>.Filter.Eq(x => x.Role, role);
            return await _collection.Find(filter).ToListAsync();
        }

        public async Task<List<User>> GetByDepartmentAsync(string department)
        {
            var filter = Builders<User>.Filter.Eq(x => x.Department, department);
            return await _collection.Find(filter).ToListAsync();
        }

        public async Task<List<User>> GetByManagerIdAsync(string managerId)
        {
            var filter = Builders<User>.Filter.Eq(x => x.ManagerIds, managerId);
            return await _collection.Find(filter).ToListAsync();
        }

        public async Task<User> UpdateAsync(User user)
        {
            user.UpdatedAt = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
            var filter = Builders<User>.Filter.Eq(x => x.Id, user.Id);
            await _collection.ReplaceOneAsync(filter, user);
            return user;
        }

        public async Task<bool> UpdatePasswordAsync(string id, string passwordHash)
        {
            var filter = Builders<User>.Filter.Eq(x => x.Id, id);
            var update = Builders<User>.Update
                .Set(x => x.PasswordHash, passwordHash)
                .Set(x => x.UpdatedAt, DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"));
            
            var result = await _collection.UpdateOneAsync(filter, update);
            return result.ModifiedCount > 0;
        }

        public async Task<bool> UpdateProfileAsync(string id, string fullName, string email, string phone)
        {
            var filter = Builders<User>.Filter.Eq(x => x.Id, id);
            var update = Builders<User>.Update
                .Set(x => x.FullName, fullName)
                .Set(x => x.Email, email)
                .Set(x => x.Phone, phone)
                .Set(x => x.UpdatedAt, DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"));
            
            var result = await _collection.UpdateOneAsync(filter, update);
            return result.ModifiedCount > 0;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            var filter = Builders<User>.Filter.Eq(x => x.Id, id);
            var result = await _collection.DeleteOneAsync(filter);
            return result.DeletedCount > 0;
        }

        public async Task<long> CountAsync()
        {
            return await _collection.CountDocumentsAsync(Builders<User>.Filter.Empty);
        }

        public async Task<long> CountByRoleAsync(string role)
        {
            var filter = Builders<User>.Filter.Eq(x => x.Role, role);
            return await _collection.CountDocumentsAsync(filter);
        }

        public async Task<long> CountByDepartmentAsync(string department)
        {
            var filter = Builders<User>.Filter.Eq(x => x.Department, department);
            return await _collection.CountDocumentsAsync(filter);
        }

        public async Task<List<User>> GetPaginatedAsync(int page, int pageSize)
        {
            var skip = (page - 1) * pageSize;
            return await _collection.Find(Builders<User>.Filter.Empty)
                .Sort(Builders<User>.Sort.Descending(x => x.CreatedAt))
                .Skip(skip)
                .Limit(pageSize)
                .ToListAsync();
        }

        public async Task<List<User>> SearchAsync(string searchTerm)
        {
            var builder = Builders<User>.Filter;
            var filter = builder.Or(
                builder.Regex(x => x.Username, new BsonRegularExpression(searchTerm, "i")),
                builder.Regex(x => x.FullName, new BsonRegularExpression(searchTerm, "i")),
                builder.Regex(x => x.Email, new BsonRegularExpression(searchTerm, "i")),
                builder.Regex(x => x.Department, new BsonRegularExpression(searchTerm, "i"))
            );
            
            return await _collection.Find(filter).ToListAsync();
        }

        public async Task<bool> UsernameExistsAsync(string username)
        {
            var filter = Builders<User>.Filter.Eq(x => x.Username, username);
            var count = await _collection.CountDocumentsAsync(filter);
            return count > 0;
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            var filter = Builders<User>.Filter.Eq(x => x.Email, email);
            var count = await _collection.CountDocumentsAsync(filter);
            return count > 0;
        }
    }
}
