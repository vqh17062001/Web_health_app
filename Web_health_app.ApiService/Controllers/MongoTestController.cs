using Microsoft.AspNetCore.Mvc;
using Web_health_app.ApiService.Entities.NonSQLTable;
using MongoDB.Driver;

namespace Web_health_app.ApiService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MongoTestController : ControllerBase
    {
        private readonly AtlasDbContext _atlasDbContext;

        public MongoTestController(AtlasDbContext atlasDbContext)
        {
            _atlasDbContext = atlasDbContext;
        }

        [HttpGet("test-connection")]
        public async Task<IActionResult> TestConnection()
        {
            try
            {
                // Test getting all audit logs (should be empty initially)
                var auditLogs = await _atlasDbContext.FindAsync(_atlasDbContext.AuditLogs, Builders<AuditLog>.Filter.Empty);

                return Ok(new
                {
                    Message = "Successfully connected to MongoDB Atlas!",
                    AuditLogsCount = auditLogs.Count,
                    Timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    Message = "Failed to connect to MongoDB Atlas",
                    Error = ex.Message,
                    Timestamp = DateTime.UtcNow
                });
            }
        }

        [HttpPost("create-audit-log")]
        public async Task<IActionResult> CreateAuditLog([FromBody] CreateAuditLogRequest request)
        {
            try
            {
                var auditLog = new AuditLog
                {
                    Action = request.Action,
                    Resource = request.Resource,
                    UserId = request.UserId,
                    IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown",
                    Detail = request.Detail ?? new Dictionary<string, object?>()
                };

                var createdAuditLog = await _atlasDbContext.CreateAuditLogAsync(auditLog);

                return Ok(new
                {
                    Message = "Audit log created successfully",
                    AuditLog = createdAuditLog,
                    Timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    Message = "Failed to create audit log",
                    Error = ex.Message,
                    Timestamp = DateTime.UtcNow
                });
            }
        }

        [HttpGet("audit-logs")]
        public async Task<IActionResult> GetAuditLogs([FromQuery] string? userId = null, [FromQuery] string? action = null)
        {
            try
            {
                var filter = _atlasDbContext.GetAuditLogFilter(userId, action);
                var auditLogs = await _atlasDbContext.FindAsync(_atlasDbContext.AuditLogs, filter);

                return Ok(new
                {
                    Message = "Audit logs retrieved successfully",
                    Count = auditLogs.Count,
                    AuditLogs = auditLogs,
                    Timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    Message = "Failed to retrieve audit logs",
                    Error = ex.Message,
                    Timestamp = DateTime.UtcNow
                });
            }
        }

        [HttpPost("create-user")]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
        {
            try
            {
                var user = new Entities.NonSQLTable.User
                {
                    Username = request.Username,
                    FullName = request.FullName,
                    Email = request.Email,
                    Role = request.Role,
                    Department = request.Department,
                    Gender = request.Gender,
                    Dob = request.Dob,
                    Phone = request.Phone,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password) // Bạn cần cài BCrypt.Net package
                };

                var createdUser = await _atlasDbContext.CreateUserAsync(user);

                return Ok(new
                {
                    Message = "User created successfully",
                    User = new
                    {
                        createdUser.Id,
                        createdUser.Username,
                        createdUser.FullName,
                        createdUser.Email,
                        createdUser.Role,
                        createdUser.Department,
                        createdUser.CreatedAt
                    },
                    Timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    Message = "Failed to create user",
                    Error = ex.Message,
                    Timestamp = DateTime.UtcNow
                });
            }
        }
    }

    // Request DTOs
    public class CreateAuditLogRequest
    {
        public string Action { get; set; } = string.Empty;
        public string Resource { get; set; } = string.Empty;
        public string? UserId { get; set; }
        public Dictionary<string, object?>? Detail { get; set; }
    }

    public class CreateUserRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public string Gender { get; set; } = string.Empty;
        public string Dob { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
    }
}
