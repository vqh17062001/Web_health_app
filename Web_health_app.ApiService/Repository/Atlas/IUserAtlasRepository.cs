using Web_health_app.ApiService.Entities.NonSQLTable;
using MongoDB.Driver;

namespace Web_health_app.ApiService.Repository.Atlas
{
    public interface IUserAtlasRepository
    {
        Task<User> CreateAsync(User user);
        Task<List<User>> GetAllAsync();
        Task<User?> GetByIdAsync(string id);
        Task<User?> GetByUsernameAsync(string username);
        Task<User?> GetByEmailAsync(string email);
        Task<List<User>> GetByRoleAsync(string role);
        Task<List<User>> GetByDepartmentAsync(string department);
        Task<List<User>> GetByManagerIdAsync(string managerId);
        Task<User> UpdateAsync(User user);
        Task<bool> UpdatePasswordAsync(string id, string passwordHash);
        Task<bool> UpdateProfileAsync(string id, string fullName, string email, string phone);
        Task<bool> DeleteAsync(string id);
        Task<long> CountAsync();
        Task<long> CountByRoleAsync(string role);
        Task<long> CountByDepartmentAsync(string department);
        Task<List<User>> GetPaginatedAsync(int page, int pageSize);
        Task<List<User>> SearchAsync(string searchTerm);
        Task<bool> UsernameExistsAsync(string username);
        Task<bool> EmailExistsAsync(string email);
    }
}
