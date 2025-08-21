using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using System.Net.Http.Headers;
using System.Text.Json;
using Web_health_app.ApiService.Entities.NonSQLTable;
using Web_health_app.Models.Models.NonSqlDTO;

namespace Web_health_app.Web.ApiClients.Atlas
{
    public class AtlasApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly AuthenticationStateProvider _authStateProvider;
        private readonly ProtectedLocalStorage _localStorage;
        private readonly JsonSerializerOptions _jsonOptions;

        public AtlasApiClient(HttpClient httpClient,
                               AuthenticationStateProvider authenticationStateProvider,
                               ProtectedLocalStorage localStorage)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _authStateProvider = authenticationStateProvider ?? throw new ArgumentNullException(nameof(authenticationStateProvider));
            _localStorage = localStorage ?? throw new ArgumentNullException(nameof(localStorage));
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            };
        }

        /// <summary>
        /// Set authorization header with token from local storage
        /// </summary>
        private async Task SetAuthorizeHeader()
        {
            try
            {
                var tokenResult = await _localStorage.GetAsync<string>("authToken");
                if (tokenResult.Success && !string.IsNullOrEmpty(tokenResult.Value))
                {
                    _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenResult.Value);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error setting authorization header: {ex.Message}");
            }
        }

        #region User Management APIs

        public async Task<List<User>?> GetAllUsersAsync()
        {
            await SetAuthorizeHeader();
            var response = await _httpClient.GetAsync("api/atlas/users");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<List<User>>(json, _jsonOptions);
            }
            return null;
        }

        public async Task<User?> GetUserAsync(string id)
        {
            await SetAuthorizeHeader();
            var response = await _httpClient.GetAsync($"api/atlas/users/{id}");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<User>(json, _jsonOptions);
            }
            return null;
        }

        public async Task<User?> GetUserByUsernameAsync(string username)
        {
            await SetAuthorizeHeader();
            var response = await _httpClient.GetAsync($"api/atlas/users/username/{username}");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<User>(json, _jsonOptions);
            }
            return null;
        }

        #endregion

        #region Device Management APIs

        public async Task<List<Device>?> GetAllDevicesAsync()
        {
            await SetAuthorizeHeader();
            var response = await _httpClient.GetAsync("api/atlas/devices");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<List<Device>>(json, _jsonOptions);
            }
            return null;
        }

        public async Task<Device?> GetDeviceAsync(string id)
        {
            await SetAuthorizeHeader();
            var response = await _httpClient.GetAsync($"api/atlas/devices/{id}");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<Device>(json, _jsonOptions);
            }
            return null;
        }

        public async Task<List<Device>?> GetDevicesByUserAsync(string userId)
        {
            await SetAuthorizeHeader();
            var response = await _httpClient.GetAsync($"api/atlas/devices/user/{userId}");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<List<Device>>(json, _jsonOptions);
            }
            return null;
        }

        public async Task<object?> GetDevicesStatusByUserAsync(string userId)
        {
            await SetAuthorizeHeader();
            var response = await _httpClient.GetAsync($"api/atlas/devices/user/{userId}/statistics");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<object>(json, _jsonOptions);
            }
            return null;
        }

        #endregion

        #region Sensor Reading Management APIs

        public async Task<List<SensorReadingInfoDto>?> GetAllSensorReadingsAsync()
        {
            await SetAuthorizeHeader();
            var response = await _httpClient.GetAsync("api/atlas/sensor-readings");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<List<SensorReadingInfoDto>>(json, _jsonOptions);
            }
            return null;
        }

        public async Task<SensorReadingInfoDto?> GetSensorReadingAsync(string id)
        {
            await SetAuthorizeHeader();
            var response = await _httpClient.GetAsync($"api/atlas/sensor-readings/{id}");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<SensorReadingInfoDto>(json, _jsonOptions);
            }
            return null;
        }

        public async Task<List<SensorReadingInfoDto>?> GetSensorReadingsByUserAsync(string userId)
        {
            await SetAuthorizeHeader();
            var response = await _httpClient.GetAsync($"api/atlas/sensor-readings/user/{userId}");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<List<SensorReadingInfoDto>>(json, _jsonOptions);
            }
            return null;
        }

        public async Task<List<SensorReadingInfoDto>?> GetSensorReadingsByDeviceAsync(string deviceId, int page = 1, int pageSize = 10)
        {
            await SetAuthorizeHeader();
            var response = await _httpClient.GetAsync($"api/atlas/sensor-readings/device/{deviceId}?page={page}&pageSize={pageSize}");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<List<SensorReadingInfoDto>>(json, _jsonOptions);
            }
            return null;
        }

        public async Task<List<SensorReadingInfoDto>?> GetSensorReadingsByTypeAsync(string sensorType, int page = 1, int pageSize = 10)
        {
            await SetAuthorizeHeader();
            var response = await _httpClient.GetAsync($"api/atlas/sensor-readings/type/{sensorType}?page={page}&pageSize={pageSize}");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<List<SensorReadingInfoDto>>(json, _jsonOptions);
            }
            return null;
        }

        public async Task<List<SensorReadingInfoDto>?> GetSensorReadingsByDateRangeAsync(DateTime fromDate, DateTime toDate)
        {
            await SetAuthorizeHeader();
            var response = await _httpClient.GetAsync($"api/atlas/sensor-readings/date-range?fromDate={fromDate:yyyy-MM-dd}&toDate={toDate:yyyy-MM-dd}");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<List<SensorReadingInfoDto>>(json, _jsonOptions);
            }
            return null;
        }

        public async Task<List<SensorReadingInfoDto>?> GetLatestSensorReadingsByUserAsync(string userId, int limit = 70, string? sensorType = null)
        {
            await SetAuthorizeHeader();
            var url = $"api/atlas/sensor-readings/user/{userId}/latest?limit={limit}";
            if (!string.IsNullOrEmpty(sensorType))
            {
                url += $"&sensorType={sensorType}";
            }

            var response = await _httpClient.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<List<SensorReadingInfoDto>>(json, _jsonOptions);
            }
            return null;
        }

        public async Task<SensorStatisticsDto?> GetSensorTypeStatisticsAsync()
        {
            await SetAuthorizeHeader();
            var response = await _httpClient.GetAsync("api/atlas/sensor-readings/statistics/sensor-types");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<SensorStatisticsDto>(json, _jsonOptions);
            }
            return null;
        }

        public async Task<List<SensorReadingInfoDto>?> GetRecentSensorReadingsAsync(int hours = 24)
        {
            await SetAuthorizeHeader();
            var response = await _httpClient.GetAsync($"api/atlas/sensor-readings/recent?hours={hours}");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<List<SensorReadingInfoDto>>(json, _jsonOptions);
            }
            return null;
        }

        public async Task<SensorReadingListDto?> GetPaginatedSensorReadingsAsync(int page = 1, int pageSize = 10)
        {
            await SetAuthorizeHeader();
            var response = await _httpClient.GetAsync($"api/atlas/sensor-readings/paginated?page={page}&pageSize={pageSize}");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<SensorReadingListDto>(json, _jsonOptions);
            }
            return null;
        }

        #endregion

        #region Audit Log Management APIs

        public async Task<List<AuditLog>?> GetAllAuditLogsAsync()
        {
            await SetAuthorizeHeader();
            var response = await _httpClient.GetAsync("api/atlas/audit-logs");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<List<AuditLog>>(json, _jsonOptions);
            }
            return null;
        }

        public async Task<AuditLog?> GetAuditLogAsync(string id)
        {
            await SetAuthorizeHeader();
            var response = await _httpClient.GetAsync($"api/atlas/audit-logs/{id}");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<AuditLog>(json, _jsonOptions);
            }
            return null;
        }

        public async Task<List<AuditLog>?> GetAuditLogsByUserAsync(string userId, int page = 1, int pageSize = 20)
        {
            await SetAuthorizeHeader();
            var response = await _httpClient.GetAsync($"api/atlas/audit-logs/user/?userId={userId}&page={page}&pageSize={pageSize}");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<List<AuditLog>>(json, _jsonOptions);
            }
            return null;
        }

        public async Task<List<AuditLog>?> GetAuditLogsByActionAsync(string action, int page = 1, int pageSize = 20)
        {
            await SetAuthorizeHeader();
            var response = await _httpClient.GetAsync($"api/atlas/audit-logs/action/?action={action}&page={page}&pageSize={pageSize}");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<List<AuditLog>>(json, _jsonOptions);
            }
            return null;
        }

        public async Task<List<AuditLog>?> GetAuditLogsByDateRangeAsync(DateTime fromDate, DateTime toDate)
        {
            await SetAuthorizeHeader();
            var response = await _httpClient.GetAsync($"api/atlas/audit-logs/date-range?fromDate={fromDate:yyyy-MM-dd}&toDate={toDate:yyyy-MM-dd}");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<List<AuditLog>>(json, _jsonOptions);
            }
            return null;
        }

        public async Task<List<AuditLog>?> GetPaginatedAuditLogsAsync(int page = 1, int pageSize = 20)
        {
            await SetAuthorizeHeader();
            var response = await _httpClient.GetAsync($"api/atlas/audit-logs/paginated?page={page}&pageSize={pageSize}");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<List<AuditLog>>(json, _jsonOptions);
            }
            return null;
        }

        #endregion

        #region General Statistics APIs

        public async Task<object?> GetCountsAsync()
        {
            await SetAuthorizeHeader();
            var response = await _httpClient.GetAsync("api/atlas/statistics/counts");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<object>(json, _jsonOptions);
            }
            return null;
        }

        #endregion
    }
}
