
using MongoDB.Bson;
using MongoDB.Driver;
using System.Data.Entity;
using System.Globalization;
using Web_health_app.ApiService.Entities.NonSQLTable;
using Web_health_app.Models.Models;


namespace Web_health_app.ApiService.Repository.Atlas
{
    public class UserAtlasRepository : IUserAtlasRepository
    {
        private readonly AtlasDbContext _context;
        private readonly IMongoCollection<User> _collection;
        private readonly Web_health_app.ApiService.Entities.HealthDbContext _healthWebDbContext;

        public UserAtlasRepository(AtlasDbContext context, Entities.HealthDbContext healthWebDbContext)
        {
            _context = context;
            _collection = _context.Users;
            _healthWebDbContext = healthWebDbContext;
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

        public async Task<List<StudentInfoDto>> SyncUserToStudent() 
        {

            var allStudents = _healthWebDbContext.Students.Where(x => x.StudentId.Contains("ObjectId_")).AsNoTracking()
               .Select(s => new Web_health_app.ApiService.Entities.Student
                {
                    StudentId = s.StudentId != null ? s.StudentId.Replace("ObjectId_", "") : null,
                    
                })
                .ToList();
            var studentIds = allStudents
                .Select(s => s.StudentId)
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .Distinct()
                .ToList();

            var filter = Builders<User>.Filter.And(
                 Builders<User>.Filter.Nin(u => u.Id, studentIds)
            );

            var userNotInSqlDb = _collection.Find(filter).ToList();


            var newStudents = userNotInSqlDb
               .Select(u => new Web_health_app.ApiService.Entities.Student
               {
                   StudentId = "ObjectId_"+u.Id,          // nếu u.Id là ObjectId thì dùng u.Id.ToString()
                   Name = u.FullName,
                   Email = !string.IsNullOrWhiteSpace(u.Email) ? u.Email : null,
                   Phone = !string.IsNullOrWhiteSpace(u.Phone) ? u.Phone : null,
                   Dob = !string.IsNullOrWhiteSpace(u.Dob) ? DateTime.Parse(u.Dob).ToString("yyyy-MM-dd") : null,
                   Gender = !string.IsNullOrWhiteSpace(u.Gender) ? u.Gender : null,
                   Department = !string.IsNullOrWhiteSpace(u.Department) ? u.Department : null,
                   CreatedAt = u.CreatedAt != null ? DateTime.Parse(u.CreatedAt) : DateTime.UtcNow,
                   UpdateAt = u.UpdatedAt != null ? DateTime.Parse(u.UpdatedAt) : null,
                   ManageBy = !string.IsNullOrWhiteSpace(u.ManagerIds) ? Guid.Parse(u.ManagerIds) : null, 
                   Status = 1,

               })
               .ToList();
            if (newStudents.Count > 0) { 
                _healthWebDbContext.Students.AddRange(newStudents);
                await _healthWebDbContext.SaveChangesAsync();
                return newStudents.Select(u => new StudentInfoDto
                {
                    StudentId = u.StudentId,
                    Name = u.Name,
                    Email =  u.Email ,
                    Phone =u.Phone ,
                    Dob = u.Dob,
                    Gender =  u.Gender ,
                    Department =  u.Department ,

                    CreatedAt = u.CreatedAt ,
                    UpdateAt = u.UpdateAt,
                    ManageBy = u.ManageBy,
                    Status = 1
                }).ToList();

            }

            return new List<StudentInfoDto>();
        }


        public async Task<List<StudentInfoDto>> UpdateSyncUserToStudent()
        {
            const string Prefix = "ObjectId_";

            // 1) Lấy các student hiện có (ID kèm prefix) + UpdateAt để so sánh ObjectId_68a847790230ac5dac5f46b6
            var sqlStudentsLight = _healthWebDbContext.Students.Where(x => x.StudentId.Contains("ObjectId_")).AsNoTracking()
             
                .ToList();

            var studentIdsWithPrefix = sqlStudentsLight
                .Select(s => s.StudentId!)
                .Distinct()
                .ToList();

            // Lấy Id user (bỏ prefix)
            var userIds = studentIdsWithPrefix
                .Select(id => id.Substring(Prefix.Length))
                .Distinct()
                .ToList();

            // 2) Lấy users tương ứng từ Mongo
            var filter = Builders<User>.Filter.In(u => u.Id, userIds);
            var users = await _collection.Find(filter).ToListAsync();

            // Convert users -> dictionary theo StudentId (kèm prefix) cho dễ join
            var userByStudentId = users.ToDictionary(u => Prefix + u.Id, u => u);

            // 3) Load lại Students ở trạng thái tracked để update
            var studentsTracked =  _healthWebDbContext.Students
                .Where(s => studentIdsWithPrefix.Contains(s.StudentId!))
                .ToDictionary(s => s.StudentId!);

            // 4) So sánh UpdatedAt & update khi khác
            var changed = new List<Web_health_app.ApiService.Entities.Student>();
            foreach (var sid in studentIdsWithPrefix)
            {
                if (!userByStudentId.TryGetValue(sid, out var u)) continue;
                if (!studentsTracked.TryGetValue(sid, out var s)) continue;

                DateTime? userUpdatedUtc = TryParseUtc(u.UpdatedAt);      // string? -> DateTime? UTC
                DateTime? studentUpdatedUtc = s.UpdateAt; // DateTime? -> UTC

                // Chỉ update khi user có UpdatedAt và khác với student
                if (NeedsUpdate(studentUpdatedUtc, userUpdatedUtc))
                {
                    s.Name = u.FullName;
                    s.Email = string.IsNullOrWhiteSpace(u.Email) ? null : u.Email;
                    s.Phone = string.IsNullOrWhiteSpace(u.Phone) ? null : u.Phone;
                    s.Dob = string.IsNullOrWhiteSpace(u.Dob) ? null : DateTime.Parse(u.Dob).ToString("yyyy-MM-dd");
                    s.Gender = string.IsNullOrWhiteSpace(u.Gender) ? null : u.Gender;
                    s.Department = string.IsNullOrWhiteSpace(u.Department) ? null : u.Department;
                    s.UpdateAt = userUpdatedUtc ?? DateTime.UtcNow;
                    s.ManageBy = Guid.TryParse(u.ManagerIds, out var g) ? g : null;
                    s.Status = 1;

                    changed.Add(s);
                }
            }

            await _healthWebDbContext.SaveChangesAsync();

            // 5) Trả về danh sách đã cập nhật (map sang DTO của bạn)
            var result = changed.Select(s => new StudentInfoDto
            {
                StudentId = s.StudentId,
                Name = s.Name,
                Email = s.Email,
                Phone = s.Phone,
                UpdateAt = s.UpdateAt
                // ... bổ sung các field khác nếu cần
            }).ToList();

            return result;
        }


        static DateTime? TryParseUtc(string? input)
        {
            if (string.IsNullOrWhiteSpace(input)) return null;

            // Ưu tiên parse kiểu có timezone → UTC
            if (DateTimeOffset.TryParse(input, CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var dto))
                return dto.UtcDateTime;

            if (DateTime.TryParse(input, CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var dt))
                return dt.ToUniversalTime();

            return null;
        }

        static bool NeedsUpdate(DateTime? studentUtc, DateTime? userUtc)
        {
            if (!userUtc.HasValue) return false;       // không có mốc thời gian từ user thì bỏ qua
            if (!studentUtc.HasValue) return true;     // student chưa có thì update
                                                       // Cho phép lệch nhỏ ≤ 1 giây để tránh khác nhau do độ chính xác
            return Math.Abs((studentUtc.Value - userUtc.Value).TotalSeconds) > 1;
        }
    }
}
 