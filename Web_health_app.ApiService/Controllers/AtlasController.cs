using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using Web_health_app.ApiService.Entities.NonSQLTable;
using Web_health_app.ApiService.Repository.Atlas;

namespace Web_health_app.ApiService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AtlasController : ControllerBase
    {
        private readonly IAuditLogRepository _auditLogRepository;
        private readonly IDeviceRepository _deviceRepository;
        private readonly IUserAtlasRepository _userRepository;
        private readonly ISensorReadingRepository _sensorReadingRepository;

        public AtlasController(
            IAuditLogRepository auditLogRepository,
            IDeviceRepository deviceRepository,
            IUserAtlasRepository userRepository,
            ISensorReadingRepository sensorReadingRepository)
        {
            _auditLogRepository = auditLogRepository;
            _deviceRepository = deviceRepository;
            _userRepository = userRepository;
            _sensorReadingRepository = sensorReadingRepository;
        }

        // ========== User Management ==========
        [HttpPost("users")]
        public async Task<ActionResult<User>> CreateUser([FromBody] User user)
        {
            var result = await _userRepository.CreateAsync(user);
            return Ok(result);
        }

        [HttpGet("users")]
        public async Task<ActionResult<List<User>>> GetAllUsers()
        {
            var users = await _userRepository.GetAllAsync();
            return Ok(users);
        }

        [HttpGet("users/{id}")]
        public async Task<ActionResult<User>> GetUser(string id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
                return NotFound();
            return Ok(user);
        }

        [HttpGet("users/username/{username}")]
        public async Task<ActionResult<User>> GetUserByUsername(string username)
        {
            var user = await _userRepository.GetByUsernameAsync(username);
            if (user == null)
                return NotFound();
            return Ok(user);
        }

        //[HttpGet("users/validate/username/{username}")]
        //public async Task<ActionResult<bool>> ValidateUsername(string username)
        //{
        //    var isValid = await _userRepository.IsUsernameAvailableAsync(username);
        //    return Ok(isValid);
        //}

        //[HttpGet("users/validate/email/{email}")]
        //public async Task<ActionResult<bool>> ValidateEmail(string email)
        //{
        //    var isValid = await _userRepository.IsEmailAvailableAsync(email);
        //    return Ok(isValid);
        //}

        // ========== Device Management ==========
        [HttpPost("devices")]
        public async Task<ActionResult<Device>> CreateDevice([FromBody] Device device)
        {
            var result = await _deviceRepository.CreateAsync(device);
            return Ok(result);
        }

        [HttpGet("devices")]
        public async Task<ActionResult<List<Device>>> GetAllDevices()
        {
            var devices = await _deviceRepository.GetAllAsync();
            return Ok(devices);
        }

        [HttpGet("devices/{id}")]
        public async Task<ActionResult<Device>> GetDevice(string id)
        {
            var device = await _deviceRepository.GetByIdAsync(id);
            if (device == null)
                return NotFound();
            return Ok(device);
        }

        [HttpGet("devices/user/{userId}")]
        public async Task<ActionResult<List<Device>>> GetDevicesByUser(string userId)
        {
            var devices = await _deviceRepository.GetByOwnerIdAsync(userId);
            return Ok(devices);
        }

        [HttpPut("devices/{id}/status")]
        public async Task<ActionResult<bool>> UpdateDeviceStatus(string id, [FromBody] string status)
        {
            var result = await _deviceRepository.UpdateStatusAsync(id, status);
            if (!result)
                return NotFound();
            return Ok(result);
        }

        //[HttpGet("devices/online")]
        //public async Task<ActionResult<List<Device>>> GetOnlineDevices()
        //{
        //    var devices = await _deviceRepository.GetOnlineDevicesAsync();
        //    return Ok(devices);
        //}

        // ========== Sensor Reading Management ==========
        [HttpPost("sensor-readings")]
        public async Task<ActionResult<SensorReading>> CreateSensorReading([FromBody] SensorReading reading)
        {
            var result = await _sensorReadingRepository.CreateAsync(reading);
            return Ok(result);
        }

        [HttpPost("sensor-readings/bulk")]
        public async Task<ActionResult<List<SensorReading>>> CreateSensorReadings([FromBody] List<SensorReading> readings)
        {
            var result = await _sensorReadingRepository.CreateManyAsync(readings);
            return Ok(result);
        }

        [HttpGet("sensor-readings")]
        public async Task<ActionResult<List<SensorReading>>> GetAllSensorReadings()
        {
            var readings = await _sensorReadingRepository.GetAllAsync();

            // Dùng serializer của Mongo (Extended JSON)
            var json = readings.ToJson(new JsonWriterSettings
            {
                OutputMode = JsonOutputMode.RelaxedExtendedJson
            });

            return Content(json, "application/json"); // không dùng Ok()
        }

        [HttpGet("sensor-readings/{id}")]
        public async Task<ActionResult<SensorReading>> GetSensorReading(string id)
        {
            var reading = await _sensorReadingRepository.GetByIdAsync(id);
            if (reading == null)
                return NotFound();
            return Ok(reading);
        }

        [HttpGet("sensor-readings/user/{userId}")]
        public async Task<ActionResult<List<SensorReading>>> GetSensorReadingsByUser(string userId)
        {
            var readings = await _sensorReadingRepository.GetByUserIdAsync(userId);
            return Ok(readings);
        }

        [HttpGet("sensor-readings/device/{deviceId}")]
        public async Task<ActionResult<List<SensorReading>>> GetSensorReadingsByDevice(string deviceId)
        {
            var readings = await _sensorReadingRepository.GetByDeviceIdAsync(deviceId);
            return Ok(readings);
        }

        [HttpGet("sensor-readings/type/{sensorType}")]
        public async Task<ActionResult<List<SensorReading>>> GetSensorReadingsByType(string sensorType)
        {
            var readings = await _sensorReadingRepository.GetBySensorTypeAsync(sensorType);
            return Ok(readings);
        }

        [HttpGet("sensor-readings/date-range")]
        public async Task<ActionResult<List<SensorReading>>> GetSensorReadingsByDateRange(
            [FromQuery] DateTime fromDate, 
            [FromQuery] DateTime toDate)
        {
            var readings = await _sensorReadingRepository.GetByDateRangeAsync(fromDate, toDate);
            return Ok(readings);
        }

        [HttpGet("sensor-readings/user/{userId}/latest")]
        public async Task<ActionResult<List<SensorReading>>> GetLatestSensorReadingsByUser(
            string userId, 
            [FromQuery] int limit = 10)
        {
            var readings = await _sensorReadingRepository.GetLatestByUserAsync(userId, limit);
            return Ok(readings);
        }

        [HttpGet("sensor-readings/statistics/sensor-types")]
        public async Task<ActionResult<Dictionary<string, long>>> GetSensorTypeStatistics()
        {
            var statistics = await _sensorReadingRepository.GetSensorTypeStatisticsAsync();
            return Ok(statistics);
        }

        [HttpGet("sensor-readings/recent")]
        public async Task<ActionResult<List<SensorReading>>> GetRecentSensorReadings([FromQuery] int hours = 24)
        {
            var readings = await _sensorReadingRepository.GetRecentReadingsAsync(hours);
            return Ok(readings);
        }

        // ========== Audit Log Management ==========
        [HttpPost("audit-logs")]
        public async Task<ActionResult<AuditLog>> CreateAuditLog([FromBody] AuditLog auditLog)
        {
            var result = await _auditLogRepository.CreateAsync(auditLog);
            return Ok(result);
        }

        [HttpGet("audit-logs")]
        public async Task<ActionResult<List<AuditLog>>> GetAllAuditLogs()
        {
            var logs = await _auditLogRepository.GetAllAsync();
            return Ok(logs);
        }

        [HttpGet("audit-logs/{id}")]
        public async Task<ActionResult<AuditLog>> GetAuditLog(string id)
        {
            var log = await _auditLogRepository.GetByIdAsync(id);
            if (log == null)
                return NotFound();
            return Ok(log);
        }

        [HttpGet("audit-logs/user/{userId}")]
        public async Task<ActionResult<List<AuditLog>>> GetAuditLogsByUser(string userId)
        {
            var logs = await _auditLogRepository.GetByUserIdAsync(userId);
            return Ok(logs);
        }

        [HttpGet("audit-logs/action/{action}")]
        public async Task<ActionResult<List<AuditLog>>> GetAuditLogsByAction(string action)
        {
            var logs = await _auditLogRepository.GetByActionAsync(action);
            return Ok(logs);
        }

        [HttpGet("audit-logs/date-range")]
        public async Task<ActionResult<List<AuditLog>>> GetAuditLogsByDateRange(
            [FromQuery] DateTime fromDate, 
            [FromQuery] DateTime toDate)
        {
            var logs = await _auditLogRepository.GetByDateRangeAsync(fromDate, toDate);
            return Ok(logs);
        }

        [HttpGet("audit-logs/paginated")]
        public async Task<ActionResult<List<AuditLog>>> GetPaginatedAuditLogs(
            [FromQuery] int page = 1, 
            [FromQuery] int pageSize = 20)
        {
            var logs = await _auditLogRepository.GetPaginatedAsync(page, pageSize);
            return Ok(logs);
        }

        // ========== General Statistics ==========
        [HttpGet("statistics/counts")]
        public async Task<ActionResult<object>> GetCounts()
        {
            var userCount = await _userRepository.CountAsync();
            var deviceCount = await _deviceRepository.CountAsync();
            var sensorReadingCount = await _sensorReadingRepository.CountAsync();
            var auditLogCount = await _auditLogRepository.CountAsync();

            return Ok(new
            {
                Users = userCount,
                Devices = deviceCount,
                SensorReadings = sensorReadingCount,
                AuditLogs = auditLogCount
            });
        }
    }
}
