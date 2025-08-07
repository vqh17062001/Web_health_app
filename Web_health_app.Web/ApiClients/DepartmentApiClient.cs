using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Web_health_app.Models.Models;

namespace Web_health_app.Web.ApiClients
{
    public class DepartmentApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly AuthenticationStateProvider _authStateProvider;
        private readonly ProtectedLocalStorage _localStorage;
        private readonly JsonSerializerOptions _jsonOptions;

        public DepartmentApiClient(HttpClient httpClient,
                                  AuthenticationStateProvider authenticationStateProvider,
                                  ProtectedLocalStorage localStorage)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _authStateProvider = authenticationStateProvider ?? throw new ArgumentNullException(nameof(authenticationStateProvider));
            _localStorage = localStorage ?? throw new ArgumentNullException(nameof(localStorage));

            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };
        }

        /// <summary>
        /// Set authorization header with token from local storage
        /// </summary>
        private async Task SetAuthorizeHeader()
        {
            try
            {
                var token = (await _localStorage.GetAsync<string>("authToken")).Value;
                if (!string.IsNullOrEmpty(token))
                {
                    _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error setting authorization header: {ex.Message}");
            }
        }

        /// <summary>
        /// Get all departments with pagination and search
        /// </summary>
        public async Task<ApiResponse<DepartmentsApiResponse>> GetAllDepartmentsAsync(int pageNumber = 1, int pageSize = 20, string? searchTerm = null)
        {
            try
            {
                await SetAuthorizeHeader();

                var queryParams = new List<string>();
                queryParams.Add($"pageNumber={pageNumber}");
                queryParams.Add($"pageSize={pageSize}");

                if (!string.IsNullOrEmpty(searchTerm))
                {
                    queryParams.Add($"searchTerm={Uri.EscapeDataString(searchTerm)}");
                }

                var queryString = string.Join("&", queryParams);
                var response = await _httpClient.GetAsync($"api/Department?{queryString}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var data = JsonSerializer.Deserialize<DepartmentsApiResponse>(content, _jsonOptions);
                    return new ApiResponse<DepartmentsApiResponse> { IsSuccess = true, Data = data };
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                return new ApiResponse<DepartmentsApiResponse>
                {
                    IsSuccess = false,
                    Message = $"Error: {response.StatusCode} - {errorContent}"
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<DepartmentsApiResponse>
                {
                    IsSuccess = false,
                    Message = $"Exception: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Search departments with advanced filters
        /// </summary>
        public async Task<ApiResponse<DepartmentsApiResponse>> SearchDepartmentsAsync(DepartmentSearchDto searchDto)
        {
            try
            {
                await SetAuthorizeHeader();

                var response = await _httpClient.PostAsJsonAsync("api/Department/search", searchDto, _jsonOptions);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var data = JsonSerializer.Deserialize<DepartmentsApiResponse>(content, _jsonOptions);
                    return new ApiResponse<DepartmentsApiResponse> { IsSuccess = true, Data = data };
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                return new ApiResponse<DepartmentsApiResponse>
                {
                    IsSuccess = false,
                    Message = $"Error: {response.StatusCode} - {errorContent}"
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<DepartmentsApiResponse>
                {
                    IsSuccess = false,
                    Message = $"Exception: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Get department by code
        /// </summary>
        public async Task<ApiResponse<DepartmentInfoDto>> GetDepartmentByCodeAsync(string departmentCode)
        {
            try
            {
                await SetAuthorizeHeader();

                var response = await _httpClient.GetAsync($"api/Department/{Uri.EscapeDataString(departmentCode)}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var data = JsonSerializer.Deserialize<DepartmentInfoDto>(content, _jsonOptions);
                    return new ApiResponse<DepartmentInfoDto> { IsSuccess = true, Data = data };
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                return new ApiResponse<DepartmentInfoDto>
                {
                    IsSuccess = false,
                    Message = $"Error: {response.StatusCode} - {errorContent}"
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<DepartmentInfoDto>
                {
                    IsSuccess = false,
                    Message = $"Exception: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Get all departments (simple list without pagination)
        /// </summary>
        public async Task<ApiResponse<List<DepartmentInfoDto>>> GetAllDepartmentsSimpleAsync()
        {
            try
            {
                await SetAuthorizeHeader();

                var response = await _httpClient.GetAsync("api/Department/simple");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var data = JsonSerializer.Deserialize<List<DepartmentInfoDto>>(content, _jsonOptions);
                    return new ApiResponse<List<DepartmentInfoDto>> { IsSuccess = true, Data = data };
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                return new ApiResponse<List<DepartmentInfoDto>>
                {
                    IsSuccess = false,
                    Message = $"Error: {response.StatusCode} - {errorContent}"
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<List<DepartmentInfoDto>>
                {
                    IsSuccess = false,
                    Message = $"Exception: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Get departments with student count
        /// </summary>
        public async Task<ApiResponse<object>> GetDepartmentsWithStudentCountAsync(int pageNumber = 1, int pageSize = 20, string? searchTerm = null)
        {
            try
            {
                await SetAuthorizeHeader();

                var queryParams = new List<string>();
                queryParams.Add($"pageNumber={pageNumber}");
                queryParams.Add($"pageSize={pageSize}");

                if (!string.IsNullOrEmpty(searchTerm))
                {
                    queryParams.Add($"searchTerm={Uri.EscapeDataString(searchTerm)}");
                }

                var queryString = string.Join("&", queryParams);
                var response = await _httpClient.GetAsync($"api/Department/with-student-count?{queryString}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var data = JsonSerializer.Deserialize<object>(content, _jsonOptions);
                    return new ApiResponse<object> { IsSuccess = true, Data = data };
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                return new ApiResponse<object>
                {
                    IsSuccess = false,
                    Message = $"Error: {response.StatusCode} - {errorContent}"
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<object>
                {
                    IsSuccess = false,
                    Message = $"Exception: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Get department statistics
        /// </summary>
        public async Task<ApiResponse<DepartmentStatisticsDto>> GetDepartmentStatisticsAsync()
        {
            try
            {
                await SetAuthorizeHeader();

                var response = await _httpClient.GetAsync("api/Department/statistics");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var data = JsonSerializer.Deserialize<DepartmentStatisticsDto>(content, _jsonOptions);
                    return new ApiResponse<DepartmentStatisticsDto> { IsSuccess = true, Data = data };
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                return new ApiResponse<DepartmentStatisticsDto>
                {
                    IsSuccess = false,
                    Message = $"Error: {response.StatusCode} - {errorContent}"
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<DepartmentStatisticsDto>
                {
                    IsSuccess = false,
                    Message = $"Exception: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Get distinct battalions
        /// </summary>
        public async Task<ApiResponse<List<string>>> GetBattalionsAsync()
        {
            try
            {
                await SetAuthorizeHeader();

                var response = await _httpClient.GetAsync("api/Department/battalions");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var data = JsonSerializer.Deserialize<List<string>>(content, _jsonOptions);
                    return new ApiResponse<List<string>> { IsSuccess = true, Data = data };
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                return new ApiResponse<List<string>>
                {
                    IsSuccess = false,
                    Message = $"Error: {response.StatusCode} - {errorContent}"
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<List<string>>
                {
                    IsSuccess = false,
                    Message = $"Exception: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Get distinct courses
        /// </summary>
        public async Task<ApiResponse<List<string>>> GetCoursesAsync()
        {
            try
            {
                await SetAuthorizeHeader();

                var response = await _httpClient.GetAsync("api/Department/courses");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var data = JsonSerializer.Deserialize<List<string>>(content, _jsonOptions);
                    return new ApiResponse<List<string>> { IsSuccess = true, Data = data };
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                return new ApiResponse<List<string>>
                {
                    IsSuccess = false,
                    Message = $"Error: {response.StatusCode} - {errorContent}"
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<List<string>>
                {
                    IsSuccess = false,
                    Message = $"Exception: {ex.Message}"
                };
            }
        }
    }
}
