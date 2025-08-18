using Web_health_app.ApiService.Entities.NonSQLTable;
using MongoDB.Driver;

namespace Web_health_app.ApiService.Repository.Atlas
{
    public interface ISensorReadingRepository
    {
        Task<SensorReading> CreateAsync(SensorReading sensorReading);
        Task<List<SensorReading>> CreateManyAsync(List<SensorReading> sensorReadings);
        Task<List<SensorReading>> GetAllAsync();
        Task<SensorReading?> GetByIdAsync(string id);
        Task<List<SensorReading>> GetByUserIdAsync(string userId);
        Task<List<SensorReading>> GetByDeviceIdAsync(string deviceId);
        Task<List<SensorReading>> GetBySensorTypeAsync(string sensorType);
        Task<List<SensorReading>> GetByDateRangeAsync(DateTime fromDate, DateTime toDate);
        Task<List<SensorReading>> GetByUserAndDateRangeAsync(string userId, DateTime fromDate, DateTime toDate);
        Task<List<SensorReading>> GetByDeviceAndDateRangeAsync(string deviceId, DateTime fromDate, DateTime toDate);
        Task<List<SensorReading>> GetLatestByUserAsync(string userId, int limit = 10);
        Task<List<SensorReading>> GetLatestByDeviceAsync(string deviceId, int limit = 10);
        Task<SensorReading?> GetLatestByUserAndSensorTypeAsync(string userId, string sensorType);
        Task<bool> DeleteAsync(string id);
        Task<bool> DeleteByUserIdAsync(string userId);
        Task<bool> DeleteByDeviceIdAsync(string deviceId);
        Task<long> CountAsync();
        Task<long> CountByUserIdAsync(string userId);
        Task<long> CountByDeviceIdAsync(string deviceId);
        Task<List<SensorReading>> GetPaginatedAsync(int page, int pageSize);
        Task<Dictionary<string, long>> GetSensorTypeStatisticsAsync();
        Task<List<SensorReading>> GetRecentReadingsAsync(int hours = 24);
    }
}
