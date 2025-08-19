using Web_health_app.ApiService.Entities.NonSQLTable;
using Web_health_app.Models.Models.NonSqlDTO;
using MongoDB.Driver;

namespace Web_health_app.ApiService.Repository.Atlas
{
    public interface ISensorReadingRepository
    {
        Task<SensorReading> CreateAsync(SensorReading sensorReading);
        Task<List<SensorReading>> CreateManyAsync(List<SensorReading> sensorReadings);
        Task<List<SensorReadingInfoDto>> GetAllAsync();
        Task<SensorReadingInfoDto?> GetByIdAsync(string id);
        Task<List<SensorReadingInfoDto>> GetByUserIdAsync(string userId);
        Task<List<SensorReadingInfoDto>> GetByDeviceIdAsync(string deviceId);
        Task<List<SensorReadingInfoDto>> GetBySensorTypeAsync(string sensorType);
        Task<List<SensorReadingInfoDto>> GetByDateRangeAsync(DateTime fromDate, DateTime toDate);
        Task<List<SensorReadingInfoDto>> GetByUserAndDateRangeAsync(string userId, DateTime fromDate, DateTime toDate);
        Task<List<SensorReadingInfoDto>> GetByDeviceAndDateRangeAsync(string deviceId, DateTime fromDate, DateTime toDate);
        Task<List<SensorReadingInfoDto>> GetLatestByUserAsync(string userId, int limit = 10, string sensorType = null);
        Task<List<SensorReadingInfoDto>> GetLatestByDeviceAsync(string deviceId, int limit = 10);
        Task<SensorReadingInfoDto?> GetLatestByUserAndSensorTypeAsync(string userId, string sensorType);
        Task<bool> DeleteAsync(string id);
        Task<bool> DeleteByUserIdAsync(string userId);
        Task<bool> DeleteByDeviceIdAsync(string deviceId);
        Task<long> CountAsync();
        Task<long> CountByUserIdAsync(string userId);
        Task<long> CountByDeviceIdAsync(string deviceId);
        Task<SensorReadingListDto> GetPaginatedAsync(int page, int pageSize);
        Task<SensorStatisticsDto> GetSensorTypeStatisticsAsync();
        Task<List<SensorReadingInfoDto>> GetRecentReadingsAsync(int hours = 24);
    }
}
