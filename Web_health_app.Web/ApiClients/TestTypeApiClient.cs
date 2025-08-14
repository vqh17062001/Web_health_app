using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Web_health_app.Models.Models;

namespace Web_health_app.Web.ApiClients
{
    public class TestTypeApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly AuthenticationStateProvider _authStateProvider;
        private readonly ProtectedLocalStorage _localStorage;
        private readonly JsonSerializerOptions _jsonOptions;

        public TestTypeApiClient(HttpClient httpClient,
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
        /// Get all test types with pagination and search
        /// </summary>
        public async Task<TestTypesApiResponse?> GetAllTestTypesAsync(int pageNumber = 1, int pageSize = 10, string? searchTerm = null)
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
                var response = await _httpClient.GetAsync($"api/TestType?{query}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<TestTypesApiResponse>(content, _jsonOptions);
                }

                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetAllTestTypesAsync: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Search test types with advanced filters
        /// </summary>
        public async Task<TestTypesApiResponse?> SearchTestTypesAsync(TestTypeSearchDto searchDto)
        {
            try
            {
                await SetAuthorizeHeader();
                var json = JsonSerializer.Serialize(searchDto, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("api/TestType/search", content);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<TestTypesApiResponse>(responseContent, _jsonOptions);
                }

                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in SearchTestTypesAsync: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Get test type by ID
        /// </summary>
        public async Task<TestTypeInfoDto?> GetTestTypeByIdAsync(string testTypeId)
        {
            try
            {
                await SetAuthorizeHeader();
                var response = await _httpClient.GetAsync($"api/TestType/{Uri.EscapeDataString(testTypeId)}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<TestTypeInfoDto>(content, _jsonOptions);
                }

                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetTestTypeByIdAsync: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Get all test types for dropdown/select options
        /// </summary>
        public async Task<List<TestTypeSelectDto>?> GetTestTypeSelectOptionsAsync()
        {
            try
            {
                await SetAuthorizeHeader();
                var response = await _httpClient.GetAsync("api/TestType/select-options");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<List<TestTypeSelectDto>>(content, _jsonOptions);
                }

                return new List<TestTypeSelectDto>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetTestTypeSelectOptionsAsync: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Get test types by unit
        /// </summary>
        public async Task<List<TestTypeInfoDto>?> GetTestTypesByUnitAsync(string unit)
        {
            try
            {
                await SetAuthorizeHeader();
                var response = await _httpClient.GetAsync($"api/TestType/unit/{Uri.EscapeDataString(unit)}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<dynamic>(content, _jsonOptions);

                    // The API returns an object with testTypes property
                    if (result != null)
                    {
                        var testTypesJson = JsonSerializer.Serialize(((JsonElement)result).GetProperty("testTypes"));
                        return JsonSerializer.Deserialize<List<TestTypeInfoDto>>(testTypesJson, _jsonOptions);
                    }
                }

                return new List<TestTypeInfoDto>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetTestTypesByUnitAsync: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Get test types by code pattern
        /// </summary>
        public async Task<List<TestTypeInfoDto>?> GetTestTypesByCodePatternAsync(string codePattern)
        {
            try
            {
                await SetAuthorizeHeader();
                var response = await _httpClient.GetAsync($"api/TestType/code-pattern/{Uri.EscapeDataString(codePattern)}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<dynamic>(content, _jsonOptions);

                    // The API returns an object with testTypes property
                    if (result != null)
                    {
                        var testTypesJson = JsonSerializer.Serialize(((JsonElement)result).GetProperty("testTypes"));
                        return JsonSerializer.Deserialize<List<TestTypeInfoDto>>(testTypesJson, _jsonOptions);
                    }
                }

                return new List<TestTypeInfoDto>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetTestTypesByCodePatternAsync: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Check if test type exists
        /// </summary>
        public async Task<bool> TestTypeExistsAsync(string testTypeId)
        {
            try
            {
                await SetAuthorizeHeader();
                var response = await _httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Head, $"api/TestType/{Uri.EscapeDataString(testTypeId)}"));
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in TestTypeExistsAsync: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Check if test type ID exists (returns JSON response)
        /// </summary>
        public async Task<bool> CheckTestTypeIdAsync(string testTypeId)
        {
            try
            {
                await SetAuthorizeHeader();
                var response = await _httpClient.GetAsync($"api/TestType/check-id/{Uri.EscapeDataString(testTypeId)}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<dynamic>(content, _jsonOptions);

                    if (result != null)
                    {
                        return ((JsonElement)result).GetProperty("exists").GetBoolean();
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in CheckTestTypeIdAsync: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Get total count of test types
        /// </summary>
        public async Task<int> GetTotalTestTypesCountAsync()
        {
            try
            {
                await SetAuthorizeHeader();
                var response = await _httpClient.GetAsync("api/TestType/count");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<dynamic>(content, _jsonOptions);

                    if (result != null)
                    {
                        return ((JsonElement)result).GetProperty("totalCount").GetInt32();
                    }
                }

                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetTotalTestTypesCountAsync: {ex.Message}");
                return 0;
            }
        }

        /// <summary>
        /// Get unique units from all test types
        /// </summary>
        public async Task<List<string>?> GetUniqueUnitsAsync()
        {
            try
            {
                await SetAuthorizeHeader();
                var response = await _httpClient.GetAsync("api/TestType/units");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<dynamic>(content, _jsonOptions);

                    if (result != null)
                    {
                        var unitsJson = JsonSerializer.Serialize(((JsonElement)result).GetProperty("units"));
                        return JsonSerializer.Deserialize<List<string>>(unitsJson, _jsonOptions);
                    }
                }

                return new List<string>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetUniqueUnitsAsync: {ex.Message}");
                throw;
            }
        }
    }
}
