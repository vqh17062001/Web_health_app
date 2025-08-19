using Microsoft.AspNetCore.Mvc;
using Web_health_app.ApiService.Entities.NonSQLTable;
using Web_health_app.ApiService.Repository.Atlas;
using Web_health_app.Models.Models.NonSqlDTO;

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

        #region User Management - Read Only APIs

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

        #endregion

        #region Device Management - Read Only APIs

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

        #endregion

        #region Sensor Reading Management - Read Only APIs

        [HttpGet("sensor-readings")]
        public async Task<ActionResult<List<SensorReadingInfoDto>>> GetAllSensorReadings()
        {
            var readings = await _sensorReadingRepository.GetAllAsync();
            return Ok(readings);
        }

        [HttpGet("sensor-readings/{id}")]
        public async Task<ActionResult<SensorReadingInfoDto>> GetSensorReading(string id)
        {
            var reading = await _sensorReadingRepository.GetByIdAsync(id);
            if (reading == null)
                return NotFound();
            return Ok(reading);
        }

        [HttpGet("sensor-readings/user/{userId}")]
        public async Task<ActionResult<List<SensorReadingInfoDto>>> GetSensorReadingsByUser(string userId)
        {
            var readings = await _sensorReadingRepository.GetByUserIdAsync(userId);
            return Ok(readings);
        }

        [HttpGet("sensor-readings/device/{deviceId}")]
        public async Task<ActionResult<List<SensorReadingInfoDto>>> GetSensorReadingsByDevice(string deviceId)
        {
            var readings = await _sensorReadingRepository.GetByDeviceIdAsync(deviceId);
            return Ok(readings);
        }

        [HttpGet("sensor-readings/type/{sensorType}")]
        public async Task<ActionResult<List<SensorReadingInfoDto>>> GetSensorReadingsByType(string sensorType)
        {
            var readings = await _sensorReadingRepository.GetBySensorTypeAsync(sensorType);
            return Ok(readings);
        }

        [HttpGet("sensor-readings/date-range")]
        public async Task<ActionResult<List<SensorReadingInfoDto>>> GetSensorReadingsByDateRange(
            [FromQuery] DateTime fromDate,
            [FromQuery] DateTime toDate)
        {
            var readings = await _sensorReadingRepository.GetByDateRangeAsync(fromDate, toDate);
            return Ok(readings);
        }

        [HttpGet("sensor-readings/user/{userId}/latest")]
        public async Task<ActionResult<List<SensorReadingInfoDto>>> GetLatestSensorReadingsByUser(
            string userId,
            [FromQuery] int limit = 10,
            [FromQuery] string sensorType = null)
        {
            var readings = await _sensorReadingRepository.GetLatestByUserAsync(userId, limit, sensorType);
            return Ok(readings);
        }

        [HttpGet("sensor-readings/statistics/sensor-types")]
        public async Task<ActionResult<SensorStatisticsDto>> GetSensorTypeStatistics()
        {
            var statistics = await _sensorReadingRepository.GetSensorTypeStatisticsAsync();
            return Ok(statistics);
        }

        [HttpGet("sensor-readings/recent")]
        public async Task<ActionResult<List<SensorReadingInfoDto>>> GetRecentSensorReadings([FromQuery] int hours = 24)
        {
            var readings = await _sensorReadingRepository.GetRecentReadingsAsync(hours);
            return Ok(readings);
        }

        [HttpGet("sensor-readings/paginated")]
        public async Task<ActionResult<SensorReadingListDto>> GetPaginatedSensorReadings(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            var result = await _sensorReadingRepository.GetPaginatedAsync(page, pageSize);
            return Ok(result);
        }

        #endregion

        #region Audit Log Management - Read Only APIs

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

        #endregion

        #region General Statistics - Read Only APIs

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

        #endregion
    }
}
