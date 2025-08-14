using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Web_health_app.Models.Models;

namespace Web_health_app.Web.ApiClients
{
    public class AssessmentTestApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonOptions;
        private readonly ProtectedLocalStorage _localStorage;
        private readonly AuthenticationStateProvider _authStateProvider;



        public AssessmentTestApiClient(HttpClient httpClient,
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
        /// Get all assessment tests with pagination and search
        /// </summary>
        public async Task<AssessmentTestsApiResponse?> GetAllAssessmentTestsAsync(int pageNumber = 1, int pageSize = 10, string? searchTerm = null)
        {
            try
            {
                await SetAuthorizeHeader();
                var queryParams = new List<string>
                {
                    $"pageNumber={pageNumber}",
                    $"pageSize={pageSize}"
                };

                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    queryParams.Add($"searchTerm={Uri.EscapeDataString(searchTerm)}");
                }

                var query = string.Join("&", queryParams);
                var response = await _httpClient.GetAsync($"api/AssessmentTest?{query}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<AssessmentTestsApiResponse>(content, _jsonOptions);
                }

                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetAllAssessmentTestsAsync: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Search assessment tests with filters
        /// </summary>
        public async Task<AssessmentTestsApiResponse?> SearchAssessmentTestsAsync(AssessmentTestSearchDto searchDto)
        {
            try
            {
                await SetAuthorizeHeader();
                var json = JsonSerializer.Serialize(searchDto, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("api/AssessmentTest/search", content);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<AssessmentTestsApiResponse>(responseContent, _jsonOptions);
                }

                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in SearchAssessmentTestsAsync: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Get assessment test by composite key (TestTypeId and AbsId)
        /// </summary>
        public async Task<AssessmentTestInfoDto?> GetAssessmentTestByIdAsync(string testTypeId, string absId)
        {
            try
            {
                await SetAuthorizeHeader();
                var response = await _httpClient.GetAsync($"api/AssessmentTest/{Uri.EscapeDataString(testTypeId)}/{Uri.EscapeDataString(absId)}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<AssessmentTestInfoDto>(content, _jsonOptions);
                }

                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetAssessmentTestByIdAsync: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Get all assessment tests for a specific student (by AbsId)
        /// </summary>
        public async Task<List<AssessmentTestInfoDto>?> GetAssessmentTestsByAbsIdAsync(string absId)
        {
            try
            {
                await SetAuthorizeHeader();
                var response = await _httpClient.GetAsync($"api/AssessmentTest/abs/{Uri.EscapeDataString(absId)}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<List<AssessmentTestInfoDto>>(content, _jsonOptions);
                }

                return new List<AssessmentTestInfoDto>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetAssessmentTestsByAbsIdAsync: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Get assessment tests by assessment batch ID with pagination
        /// </summary>
        public async Task<AssessmentTestsApiResponse?> GetAssessmentTestsByBatchIdAsync(string batchId, int pageNumber = 1, int pageSize = 10)
        {
            try
            {
                await SetAuthorizeHeader();
                var query = $"pageNumber={pageNumber}&pageSize={pageSize}";
                var response = await _httpClient.GetAsync($"api/AssessmentTest/batch/{Uri.EscapeDataString(batchId)}?{query}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<AssessmentTestsApiResponse>(content, _jsonOptions);
                }

                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetAssessmentTestsByBatchIdAsync: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Get assessment tests by recorder with pagination
        /// </summary>
        public async Task<AssessmentTestsApiResponse?> GetAssessmentTestsByRecorderAsync(Guid recordedBy, int pageNumber = 1, int pageSize = 10)
        {
            try
            {
                await SetAuthorizeHeader();
                var query = $"pageNumber={pageNumber}&pageSize={pageSize}";
                var response = await _httpClient.GetAsync($"api/AssessmentTest/recorder/{recordedBy}?{query}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<AssessmentTestsApiResponse>(content, _jsonOptions);
                }

                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetAssessmentTestsByRecorderAsync: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Create new assessment test
        /// </summary>
        public async Task<AssessmentTestInfoDto?> CreateAssessmentTestAsync(CreateAssessmentTestDto createDto)
        {
            try
            {
                await SetAuthorizeHeader();
                var json = JsonSerializer.Serialize(createDto, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("api/AssessmentTest", content);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<AssessmentTestInfoDto>(responseContent, _jsonOptions);
                }

                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in CreateAssessmentTestAsync: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Update assessment test
        /// </summary>
        public async Task<AssessmentTestInfoDto?> UpdateAssessmentTestAsync(string testTypeId, string absId, UpdateAssessmentTestDto updateDto)
        {
            try
            {
                await SetAuthorizeHeader();
                var json = JsonSerializer.Serialize(updateDto, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PutAsync($"api/AssessmentTest/{Uri.EscapeDataString(testTypeId)}/{Uri.EscapeDataString(absId)}", content);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<AssessmentTestInfoDto>(responseContent, _jsonOptions);
                }

                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in UpdateAssessmentTestAsync: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Delete assessment test
        /// </summary>
        public async Task<bool> DeleteAssessmentTestAsync(string testTypeId, string absId)
        {
            try
            {
                await SetAuthorizeHeader();
                var response = await _httpClient.DeleteAsync($"api/AssessmentTest/{Uri.EscapeDataString(testTypeId)}/{Uri.EscapeDataString(absId)}");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in DeleteAssessmentTestAsync: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Check if assessment test exists
        /// </summary>
        public async Task<bool> AssessmentTestExistsAsync(string testTypeId, string absId)
        {
            try
            {
                await SetAuthorizeHeader();
                var response = await _httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Head, $"api/AssessmentTest/{Uri.EscapeDataString(testTypeId)}/{Uri.EscapeDataString(absId)}"));
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in AssessmentTestExistsAsync: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Bulk operations on assessment tests
        /// </summary>
        public async Task<bool> BulkOperationAsync(BulkAssessmentTestOperationDto operationDto)
        {
            try
            {
                await SetAuthorizeHeader();
                var json = JsonSerializer.Serialize(operationDto, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("api/AssessmentTest/bulk", content);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in BulkOperationAsync: {ex.Message}");
                throw;
            }
        }
    }
}
