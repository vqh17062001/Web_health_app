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
    public class StudentApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly AuthenticationStateProvider _authStateProvider;
        private readonly ProtectedLocalStorage _localStorage;
        private readonly JsonSerializerOptions _jsonOptions;

        public StudentApiClient(HttpClient httpClient,
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
        /// Get all students with pagination and search
        /// </summary>
        public async Task<ApiResponse<StudentsApiResponse>> GetAllStudentsAsync(int pageNumber = 1, int pageSize = 20, string? searchTerm = null, bool includeInactive = false)
        {
            try
            {
                await SetAuthorizeHeader();

                var queryParams = new List<string>
                {
                    $"pageNumber={pageNumber}",
                    $"pageSize={pageSize}",
                    $"includeInactive={includeInactive}"
                };

                if (!string.IsNullOrEmpty(searchTerm))
                {
                    queryParams.Add($"searchTerm={Uri.EscapeDataString(searchTerm)}");
                }

                var queryString = string.Join("&", queryParams);
                var response = await _httpClient.GetAsync($"api/student?{queryString}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<StudentsApiResponse>(content, _jsonOptions);
                    return new ApiResponse<StudentsApiResponse> { IsSuccess = true, Data = result };
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return new ApiResponse<StudentsApiResponse>
                    {
                        IsSuccess = false,
                        Message = $"API Error: {response.StatusCode} - {errorContent}"
                    };
                }
            }
            catch (Exception ex)
            {
                return new ApiResponse<StudentsApiResponse>
                {
                    IsSuccess = false,
                    Message = $"Exception: {ex.Message}"
                };
            }
        }


        public async Task<ApiResponse<StudentsApiResponse>> GetStudentWithManageIDsAsync(int pageNumber = 1, int pageSize = 20, string? searchTerm = null, bool includeInactive = false, string manageId =null)
        {
            try
            {
                await SetAuthorizeHeader();

                var queryParams = new List<string>
                {
                    $"pageNumber={pageNumber}",
                    $"pageSize={pageSize}",
                    $"includeInactive={includeInactive}",
                    $"managerID={manageId}"

                };

                if (!string.IsNullOrEmpty(searchTerm))
                {
                    queryParams.Add($"searchTerm={Uri.EscapeDataString(searchTerm)}");
                }

                var queryString = string.Join("&", queryParams);
                var response = await _httpClient.GetAsync($"api/student/StudentWithManager?{queryString}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<StudentsApiResponse>(content, _jsonOptions);
                    return new ApiResponse<StudentsApiResponse> { IsSuccess = true, Data = result };
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return new ApiResponse<StudentsApiResponse>
                    {
                        IsSuccess = false,
                        Message = $"API Error: {response.StatusCode} - {errorContent}"
                    };
                }
            }
            catch (Exception ex)
            {
                return new ApiResponse<StudentsApiResponse>
                {
                    IsSuccess = false,
                    Message = $"Exception: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Get student by ID
        /// </summary>
        public async Task<ApiResponse<StudentInfoDto?>> GetStudentByIdAsync(string studentId)
        {
            try
            {
                await SetAuthorizeHeader();
                var response = await _httpClient.GetAsync($"api/student/{studentId}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<StudentInfoDto>(content, _jsonOptions);
                    return new ApiResponse<StudentInfoDto?> { IsSuccess = true, Data = result };
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return new ApiResponse<StudentInfoDto?> { IsSuccess = false, Message = "Student not found", Data = null };
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return new ApiResponse<StudentInfoDto?>
                    {
                        IsSuccess = false,
                        Message = $"API Error: {response.StatusCode} - {errorContent}"
                    };
                }
            }
            catch (Exception ex)
            {
                return new ApiResponse<StudentInfoDto?>
                {
                    IsSuccess = false,
                    Message = $"Exception: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Search students with advanced filters
        /// </summary>
        public async Task<ApiResponse<StudentsApiResponse>> SearchStudentsAsync(StudentSearchDto searchDto)
        {
            try
            {
                await SetAuthorizeHeader();
                var json = JsonSerializer.Serialize(searchDto, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync("api/student/search", content);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<StudentsApiResponse>(responseContent, _jsonOptions);
                    return new ApiResponse<StudentsApiResponse> { IsSuccess = true, Data = result };
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return new ApiResponse<StudentsApiResponse>
                    {
                        IsSuccess = false,
                        Message = $"API Error: {response.StatusCode} - {errorContent}"
                    };
                }
            }
            catch (Exception ex)
            {
                return new ApiResponse<StudentsApiResponse>
                {
                    IsSuccess = false,
                    Message = $"Exception: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Get student statistics
        /// </summary>
        public async Task<ApiResponse<StudentStatisticsDto>> GetStudentStatisticsAsync(string manageId = null)
        {
            try
            {
                await SetAuthorizeHeader();

                HttpResponseMessage response;
                if (manageId != null)
                {

                    response = await _httpClient.GetAsync($"api/student/statistics?manageId={manageId}");

                }
                else { 
                    response = await _httpClient.GetAsync($"api/student/statistics");

                }




                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<StudentStatisticsDto>(content, _jsonOptions);
                    return new ApiResponse<StudentStatisticsDto> { IsSuccess = true, Data = result };
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return new ApiResponse<StudentStatisticsDto>
                    {
                        IsSuccess = false,
                        Message = $"API Error: {response.StatusCode} - {errorContent}"
                    };
                }
            }
            catch (Exception ex)
            {
                return new ApiResponse<StudentStatisticsDto>
                {
                    IsSuccess = false,
                    Message = $"Exception: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Create new student
        /// </summary>
        public async Task<ApiResponse<StudentInfoDto?>> CreateStudentAsync(CreateStudentDto createStudentDto)
        {
            try
            {
                await SetAuthorizeHeader();
                var json = JsonSerializer.Serialize(createStudentDto, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync("api/student", content);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<StudentInfoDto>(responseContent, _jsonOptions);
                    return new ApiResponse<StudentInfoDto?> { IsSuccess = true, Data = result };
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return new ApiResponse<StudentInfoDto?>
                    {
                        IsSuccess = false,
                        Message = $"API Error: {response.StatusCode} - {errorContent}"
                    };
                }
            }
            catch (Exception ex)
            {
                return new ApiResponse<StudentInfoDto?>
                {
                    IsSuccess = false,
                    Message = $"Exception: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Update student
        /// </summary>
        public async Task<ApiResponse<StudentInfoDto?>> UpdateStudentAsync(string studentId, UpdateStudentDto updateStudentDto)
        {
            try
            {
                await SetAuthorizeHeader();
                var json = JsonSerializer.Serialize(updateStudentDto, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PutAsync($"api/student/{studentId}", content);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<StudentInfoDto>(responseContent, _jsonOptions);
                    return new ApiResponse<StudentInfoDto?> { IsSuccess = true, Data = result };
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return new ApiResponse<StudentInfoDto?> { IsSuccess = false, Message = "Student not found", Data = null };
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return new ApiResponse<StudentInfoDto?>
                    {
                        IsSuccess = false,
                        Message = $"API Error: {response.StatusCode} - {errorContent}"
                    };
                }
            }
            catch (Exception ex)
            {
                return new ApiResponse<StudentInfoDto?>
                {
                    IsSuccess = false,
                    Message = $"Exception: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Delete student (soft delete)
        /// </summary>
        public async Task<ApiResponse<bool>> DeleteStudentAsync(string studentId)
        {
            try
            {
                await SetAuthorizeHeader();
                var response = await _httpClient.DeleteAsync($"api/student/{studentId}");

                if (response.IsSuccessStatusCode)
                {
                    return new ApiResponse<bool> { IsSuccess = true, Data = true };
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return new ApiResponse<bool> { IsSuccess = false, Message = "Student not found", Data = false };
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return new ApiResponse<bool>
                    {
                        IsSuccess = false,
                        Message = $"API Error: {response.StatusCode} - {errorContent}",
                        Data = false
                    };
                }
            }
            catch (Exception ex)
            {
                return new ApiResponse<bool>
                {
                    IsSuccess = false,
                    Message = $"Exception: {ex.Message}",
                    Data = false
                };
            }
        }
    }
}
