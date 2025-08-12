using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Web_health_app.Models.Models;

namespace Web_health_app.Web.ApiClients
{
    public class AssessmentBatchApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly AuthenticationStateProvider _authStateProvider;
        private readonly ProtectedLocalStorage _localStorage;
        private readonly JsonSerializerOptions _jsonOptions;

        public AssessmentBatchApiClient(HttpClient httpClient,
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

        /// <summary>
        /// Get all assessment batches with pagination and search
        /// </summary>
        public async Task<AssessmentBatchesApiResponse?> GetAllAssessmentBatchesAsync(int pageNumber = 1, int pageSize = 10, string? searchTerm = null, bool includeInactive = false)
        {
            try
            {
                await SetAuthorizeHeader();

                var queryParams = new List<string>
                {
                    $"pageNumber={pageNumber}",
                    $"pageSize={pageSize}",
                    $"includeInactive={includeInactive.ToString().ToLower()}"
                };

                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    queryParams.Add($"searchTerm={Uri.EscapeDataString(searchTerm)}");
                }

                var queryString = string.Join("&", queryParams);
                var response = await _httpClient.GetAsync($"api/AssessmentBatch?{queryString}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var apiResponse = JsonSerializer.Deserialize<ApiResponse<AssessmentBatchesApiResponse>>(content, _jsonOptions);
                    return apiResponse?.Data;
                }

                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting assessment batches: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Search assessment batches with advanced filtering
        /// </summary>
        public async Task<AssessmentBatchesApiResponse?> SearchAssessmentBatchesAsync(AssessmentBatchSearchDto searchDto)
        {
            try
            {
                await SetAuthorizeHeader();

                var json = JsonSerializer.Serialize(searchDto, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("api/AssessmentBatch/search", content);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var apiResponse = JsonSerializer.Deserialize<ApiResponse<AssessmentBatchesApiResponse>>(responseContent, _jsonOptions);
                    return apiResponse?.Data;
                }

                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error searching assessment batches: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Get assessment batch by ID
        /// </summary>
        public async Task<AssessmentBatchInfoDto?> GetAssessmentBatchByIdAsync(string id)
        {
            try
            {
                await SetAuthorizeHeader();
                var response = await _httpClient.GetAsync($"api/AssessmentBatch/{id}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var apiResponse = JsonSerializer.Deserialize<ApiResponse<AssessmentBatchInfoDto>>(content, _jsonOptions);
                    return apiResponse?.Data;
                }

                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting assessment batch by ID: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Get assessment batch detail with students and tests
        /// </summary>
        public async Task<AssessmentBatchDetailDto?> GetAssessmentBatchDetailAsync(string id)
        {
            try
            {
                await SetAuthorizeHeader();
                var response = await _httpClient.GetAsync($"api/AssessmentBatch/{id}/detail");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var apiResponse = JsonSerializer.Deserialize<ApiResponse<AssessmentBatchDetailDto>>(content, _jsonOptions);
                    return apiResponse?.Data;
                }

                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting assessment batch detail: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Create a new assessment batch
        /// </summary>
        public async Task<AssessmentBatchInfoDto?> CreateAssessmentBatchAsync(CreateAssessmentBatchDto createDto)
        {
            try
            {
                await SetAuthorizeHeader();

                var json = JsonSerializer.Serialize(createDto, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("api/AssessmentBatch", content);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var apiResponse = JsonSerializer.Deserialize<ApiResponse<AssessmentBatchInfoDto>>(responseContent, _jsonOptions);
                    return apiResponse?.Data;
                }

                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating assessment batch: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Update an existing assessment batch
        /// </summary>
        public async Task<AssessmentBatchInfoDto?> UpdateAssessmentBatchAsync(string id, UpdateAssessmentBatchDto updateDto)
        {
            try
            {
                await SetAuthorizeHeader();

                var json = JsonSerializer.Serialize(updateDto, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PutAsync($"api/AssessmentBatch/{id}", content);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var apiResponse = JsonSerializer.Deserialize<ApiResponse<AssessmentBatchInfoDto>>(responseContent, _jsonOptions);
                    return apiResponse?.Data;
                }

                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating assessment batch: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Delete an assessment batch
        /// </summary>
        public async Task<bool> DeleteAssessmentBatchAsync(string id)
        {
            try
            {
                await SetAuthorizeHeader();
                var response = await _httpClient.DeleteAsync($"api/AssessmentBatch/{id}");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting assessment batch: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Soft delete an assessment batch
        /// </summary>
        public async Task<bool> SoftDeleteAssessmentBatchAsync(string id)
        {
            try
            {
                await SetAuthorizeHeader();
                var response = await _httpClient.PatchAsync($"api/AssessmentBatch/{id}/soft-delete", null);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error soft deleting assessment batch: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Get assessment batch statistics
        /// </summary>
        public async Task<AssessmentBatchStatisticsDto?> GetAssessmentBatchStatisticsAsync()
        {
            try
            {
                await SetAuthorizeHeader();
                var response = await _httpClient.GetAsync("api/AssessmentBatch/statistics");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var apiResponse = JsonSerializer.Deserialize<ApiResponse<AssessmentBatchStatisticsDto>>(content, _jsonOptions);
                    return apiResponse?.Data;
                }

                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting assessment batch statistics: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Get my assessment batches (current user)
        /// </summary>
        public async Task<AssessmentBatchesApiResponse?> GetMyAssessmentBatchesAsync(int pageNumber = 1, int pageSize = 20, string? searchTerm = null, bool includeInactive = false)
        {
            try
            {
                await SetAuthorizeHeader();

                var queryParams = new List<string>
                {
                    $"pageNumber={pageNumber}",
                    $"pageSize={pageSize}",
                    $"includeInactive={includeInactive.ToString().ToLower()}"
                };

                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    queryParams.Add($"searchTerm={Uri.EscapeDataString(searchTerm)}");
                }

                var queryString = string.Join("&", queryParams);
                var response = await _httpClient.GetAsync($"api/AssessmentBatch/My?{queryString}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var apiResponse = JsonSerializer.Deserialize<ApiResponse<AssessmentBatchesApiResponse>>(content, _jsonOptions);
                    return apiResponse?.Data;
                }

                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting my assessment batches: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Get recent assessment batches
        /// </summary>
        public async Task<AssessmentBatchesApiResponse?> GetRecentAssessmentBatchesAsync(int days = 30, int pageNumber = 1, int pageSize = 20)
        {
            try
            {
                await SetAuthorizeHeader();

                var queryParams = new List<string>
                {
                    $"days={days}",
                    $"pageNumber={pageNumber}",
                    $"pageSize={pageSize}"
                };

                var queryString = string.Join("&", queryParams);
                var response = await _httpClient.GetAsync($"api/AssessmentBatch/Recent?{queryString}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var apiResponse = JsonSerializer.Deserialize<ApiResponse<AssessmentBatchesApiResponse>>(content, _jsonOptions);
                    return apiResponse?.Data;
                }

                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting recent assessment batches: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Get active assessment batches
        /// </summary>
        public async Task<AssessmentBatchesApiResponse?> GetActiveAssessmentBatchesAsync(int pageNumber = 1, int pageSize = 20)
        {
            try
            {
                await SetAuthorizeHeader();

                var queryParams = new List<string>
                {
                    $"pageNumber={pageNumber}",
                    $"pageSize={pageSize}"
                };

                var queryString = string.Join("&", queryParams);
                var response = await _httpClient.GetAsync($"api/AssessmentBatch/active?{queryString}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var apiResponse = JsonSerializer.Deserialize<ApiResponse<AssessmentBatchesApiResponse>>(content, _jsonOptions);
                    return apiResponse?.Data;
                }

                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting active assessment batches: {ex.Message}");
                return null;
            }
        }
    }

    /// <summary>
    /// Generic API response wrapper
    /// </summary>
   
}
