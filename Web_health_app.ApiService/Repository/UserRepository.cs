using Microsoft.EntityFrameworkCore;
using Web_health_app.ApiService.Entities;
using Web_health_app.Models.Models;
using System.Text.RegularExpressions;
namespace Web_health_app.ApiService.Repository
{
    public class UserRepository : IUserRepository
    {
        private readonly HealthDbContext _context;

        public UserRepository(HealthDbContext context)
        {
            _context = context;
        }

        public async Task<(List<UserInfoDto> Users, int TotalCount)> GetAllUsersAsync(int pageNumber = 1, int pageSize = 10, string? searchTerm = null)
        {
            try
            {
                var query = _context.Users
                    .Include(u => u.Group)
                    .Include(u => u.ManageByNavigation)
                    .Where(u => u.UserStatus != -2) // Exclude deleted users
                    .AsQueryable();

                // Apply search filter
                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    if (searchTerm.Contains("Status") || searchTerm.Contains("status"))
                    {
                        var matches = Regex.Matches(searchTerm, @"-?\d+");
                        query = query.Where(u => u.UserStatus == int.Parse(matches[0].Value)

                            );

                    }
                    else
                    {
                        query = query.Where(u =>
                            u.UserName.Contains(searchTerm) ||
                            (u.FullName != null && u.FullName.Contains(searchTerm)) ||
                            (u.PhoneNumber != null && u.PhoneNumber.Contains(searchTerm)) ||
                            (u.Department != null && u.Department.Contains(searchTerm)));
                    }
                }

                var totalCount = await query.CountAsync();

                var users = await query
                    .OrderBy(u => u.CreateAt)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .Select(u => new UserInfoDto
                    {
                        UserId = u.UserId,
                        UserName = u.UserName,
                        FullName = u.FullName,
                        PhoneNumber = u.PhoneNumber,
                        Department = u.Department,
                        UserStatus = u.UserStatus,
                        UserStatusString = u.GetUserStatusString(),
                        ManageBy = u.ManageBy,
                        ManagerName = u.ManageByNavigation != null ? u.ManageByNavigation.FullName ?? u.ManageByNavigation.UserName : null,
                        LevelSecurity = u.LevelSecurity,
                        CreateAt = u.CreateAt,
                        UpdateAt = u.UpdateAt,
                        GroupId = u.GroupId,
                        GroupName = u.Group != null ? u.Group.GroupName : null
                    })
                    .ToListAsync();

                return (users, totalCount);
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving users", ex);
            }
        }

        public async Task<UserInfoDto?> GetUserByIdAsync(Guid userId)
        {
            try
            {
                var user = await _context.Users
                    .Include(u => u.Group)
                    .Include(u => u.ManageByNavigation)
                    .Where(u => u.UserId == userId && u.UserStatus != -2)
                    .Select(u => new UserInfoDto
                    {
                        UserId = u.UserId,
                        UserName = u.UserName,
                        FullName = u.FullName,
                        PhoneNumber = u.PhoneNumber,
                        Department = u.Department,
                        UserStatus = u.UserStatus,
                        UserStatusString = u.GetUserStatusString(),
                        ManageBy = u.ManageBy,
                        ManagerName = u.ManageByNavigation != null ? u.ManageByNavigation.FullName ?? u.ManageByNavigation.UserName : null,
                        LevelSecurity = u.LevelSecurity,
                        CreateAt = u.CreateAt,
                        UpdateAt = u.UpdateAt,
                        GroupId = u.GroupId,
                        GroupName = u.Group != null ? u.Group.GroupName : null
                    })
                    .FirstOrDefaultAsync();

                return user;
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving user by ID", ex);
            }
        }

        public async Task<UserInfoDto?> GetUserByUsernameAsync(string username)
        {
            try
            {
                var user = await _context.Users
                    .Include(u => u.Group)
                    .Include(u => u.ManageByNavigation)
                    .Where(u => u.UserName == username && u.UserStatus != -2)
                    .Select(u => new UserInfoDto
                    {
                        UserId = u.UserId,
                        UserName = u.UserName,
                        FullName = u.FullName,
                        PhoneNumber = u.PhoneNumber,
                        Department = u.Department,
                        UserStatus = u.UserStatus,
                        UserStatusString = u.GetUserStatusString(),
                        ManageBy = u.ManageBy,
                        ManagerName = u.ManageByNavigation != null ? u.ManageByNavigation.FullName ?? u.ManageByNavigation.UserName : null,
                        LevelSecurity = u.LevelSecurity,
                        CreateAt = u.CreateAt,
                        UpdateAt = u.UpdateAt,
                        GroupId = u.GroupId,
                        GroupName = u.Group != null ? u.Group.GroupName : null
                    })
                    .FirstOrDefaultAsync();

                return user;
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving user by username", ex);
            }
        }

        public async Task<UserInfoDto> CreateUserAsync(CreateUserDto createUserDto)
        {
            try
            {
                // Check if username already exists
                if (await UsernameExistsAsync(createUserDto.UserName))
                {
                    throw new InvalidOperationException("Username already exists");
                }

                var user = new User
                {
                    UserId = Guid.NewGuid(),
                    UserName = createUserDto.UserName,
                    PasswordHash = createUserDto.Password, // Note: Should hash password in production
                    FullName = createUserDto.FullName,
                    PhoneNumber = createUserDto.PhoneNumber,
                    Department = createUserDto.Department,
                    UserStatus = createUserDto.UserStatus,
                    ManageBy = createUserDto.ManageBy,
                    LevelSecurity = createUserDto.LevelSecurity,
                    CreateAt = DateTime.Now,
                    GroupId = createUserDto.GroupId
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                return await GetUserByIdAsync(user.UserId) ?? throw new InvalidOperationException("Failed to retrieve created user");
            }
            catch (Exception ex)
            {
                throw new Exception("Error creating user", ex);
            }
        }

        public async Task<UserInfoDto?> UpdateUserAsync(Guid userId, UpdateUserDto updateUserDto)
        {
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId && u.UserStatus != -2);
                if (user == null)
                {
                    return null;
                }

                // Update only provided fields
                if (updateUserDto.FullName != null)
                    user.FullName = updateUserDto.FullName;

                if (updateUserDto.PhoneNumber != null)
                    user.PhoneNumber = updateUserDto.PhoneNumber;

                if (updateUserDto.Department != null)
                    user.Department = updateUserDto.Department;

                if (updateUserDto.UserStatus.HasValue)
                    user.UserStatus = updateUserDto.UserStatus.Value;

                if (updateUserDto.ManageBy.HasValue)
                    user.ManageBy = updateUserDto.ManageBy;

                if (updateUserDto.LevelSecurity.HasValue)
                    user.LevelSecurity = updateUserDto.LevelSecurity.Value;

                if (updateUserDto.GroupId != null)
                    user.GroupId = updateUserDto.GroupId;

                user.UpdateAt = DateTime.Now;

                await _context.SaveChangesAsync();

                return await GetUserByIdAsync(userId);
            }
            catch (Exception ex)
            {
                throw new Exception("Error updating user", ex);
            }
        }

        public async Task<bool> DeleteUserAsync(Guid userId)
        {
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId && u.UserStatus != -2);
                if (user == null)
                {
                    return false;
                }

                // Soft delete by changing status
                user.UserStatus = -2; // Deleted status
                user.UpdateAt = DateTime.Now;

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception("Error deleting user", ex);
            }
        }

        public async Task<bool> HardDeleteUserAsync(Guid userId)
        {
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId);
                if (user == null)
                {
                    return false;
                }

                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception("Error permanently deleting user", ex);
            }
        }

        public async Task<bool> UsernameExistsAsync(string username, Guid? excludeUserId = null)
        {
            try
            {
                var query = _context.Users.Where(u => u.UserName == username && u.UserStatus != -2);

                if (excludeUserId.HasValue)
                {
                    query = query.Where(u => u.UserId != excludeUserId.Value);
                }

                return await query.AnyAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error checking username existence", ex);
            }
        }

        public async Task<List<UserInfoDto>> GetUsersByManagerAsync(Guid managerId)
        {
            try
            {
                var users = await _context.Users
                    .Include(u => u.Group)
                    .Include(u => u.ManageByNavigation)
                    .Where(u => u.ManageBy == managerId && u.UserStatus != -2)
                    .Select(u => new UserInfoDto
                    {
                        UserId = u.UserId,
                        UserName = u.UserName,
                        FullName = u.FullName,
                        PhoneNumber = u.PhoneNumber,
                        Department = u.Department,
                        UserStatus = u.UserStatus,
                        UserStatusString = u.GetUserStatusString(),
                        ManageBy = u.ManageBy,
                        ManagerName = u.ManageByNavigation != null ? u.ManageByNavigation.FullName ?? u.ManageByNavigation.UserName : null,
                        LevelSecurity = u.LevelSecurity,
                        CreateAt = u.CreateAt,
                        UpdateAt = u.UpdateAt,
                        GroupId = u.GroupId,
                        GroupName = u.Group != null ? u.Group.GroupName : null
                    })
                    .OrderBy(u => u.CreateAt)
                    .ToListAsync();

                return users;
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving users by manager", ex);
            }
        }


        public async Task<List<UserInfoDto>> GetUserWithCompareSecurityLevel(int level, bool lessThen = true) {
            try
            {

             
                if (lessThen)
                {

                    var listUserResult = await _context.Users
                    .Where(u => u.LevelSecurity <= level)
                    .Select(u => new UserInfoDto
                    {
                        UserId = u.UserId,
                        UserName = u.UserName,
                        FullName = u.FullName,
                        PhoneNumber = u.PhoneNumber,
                        Department = u.Department,
                        UserStatus = u.UserStatus,
                        UserStatusString = u.GetUserStatusString(),
                        ManageBy = u.ManageBy,
                        ManagerName = u.ManageByNavigation != null ? u.ManageByNavigation.FullName ?? u.ManageByNavigation.UserName : null,
                        LevelSecurity = u.LevelSecurity,
                        CreateAt = u.CreateAt,
                        UpdateAt = u.UpdateAt,
                        GroupId = u.GroupId,
                        GroupName = u.Group != null ? u.Group.GroupName : null
                    })
                    .OrderBy(u => u.CreateAt)
                    .ToListAsync();
                    return listUserResult;

                }
                else
                {
                    var listUserResult = await _context.Users
                    .Where(u => u.LevelSecurity >= level)
                    .Select(u => new UserInfoDto
                    {
                        UserId = u.UserId,
                        UserName = u.UserName,
                        FullName = u.FullName,
                        PhoneNumber = u.PhoneNumber,
                        Department = u.Department,
                        UserStatus = u.UserStatus,
                        UserStatusString = u.GetUserStatusString(),
                        ManageBy = u.ManageBy,
                        ManagerName = u.ManageByNavigation != null ? u.ManageByNavigation.FullName ?? u.ManageByNavigation.UserName : null,
                        LevelSecurity = u.LevelSecurity,
                        CreateAt = u.CreateAt,
                        UpdateAt = u.UpdateAt,
                        GroupId = u.GroupId,
                        GroupName = u.Group != null ? u.Group.GroupName : null
                    })
                    .OrderBy(u => u.CreateAt)
                    .ToListAsync();
                    return listUserResult;

                }




            }
            catch (Exception ex)
            {
                throw new Exception("Error get user with compare security", ex);
            }

        }

        public async Task<bool> ChangePasswordAsync(Guid userId, string newPassword)
        {
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId && u.UserStatus != -2);
                if (user == null)
                {
                    return false;
                }

                user.PasswordHash = newPassword; // Note: Should hash password in production
                user.UpdateAt = DateTime.Now;

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception("Error changing user password", ex);
            }
        }
    }
}
