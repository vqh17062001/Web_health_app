using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Web_health_app.Models.Models;
using Web_health_app.Web.Authentication;

namespace Web_health_app.Web.ApiClients
{
    public class RoleApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly AuthenticationStateProvider _authStateProvider;
        private readonly ProtectedLocalStorage _localStorage;

        public RoleApiClient(HttpClient httpClient,
                            AuthenticationStateProvider authenticationStateProvider,
                            ProtectedLocalStorage localStorage)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _authStateProvider = authenticationStateProvider ?? throw new ArgumentException(nameof(authenticationStateProvider));
            _localStorage = localStorage ?? throw new ArgumentNullException(nameof(localStorage));
        }

        /// <summary>
        /// Set authorization header with token from local storage
        /// </summary>
        public async Task SetAuthorizeHeader()
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
        /// Get all roles with pagination and search
        /// </summary>
        /// <param name="pageNumber">Page number (default: 1)</param>
        /// <param name="pageSize">Page size (default: 10)</param>
        /// <param name="searchTerm">Search term for filtering</param>
        /// <param name="includeInactive">Include inactive roles</param>
        /// <returns>API response with roles and pagination info</returns>
        public async Task<ApiResponse<RolesApiResponse>> GetAllRolesAsync(int pageNumber = 1, int pageSize = 10, string? searchTerm = null, bool includeInactive = false)
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

                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    queryParams.Add($"searchTerm={Uri.EscapeDataString(searchTerm)}");
                }

                var queryString = string.Join("&", queryParams);
                var response = await _httpClient.GetAsync($"api/role?{queryString}");

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<RolesApiResponse>();
                    return new ApiResponse<RolesApiResponse>
                    {
                        IsSuccess = true,
                        Message = "Roles retrieved successfully",
                        Data = result
                    };
                }
                else
                {
                    return new ApiResponse<RolesApiResponse>
                    {
                        IsSuccess = false,
                        Message = $"Failed to retrieve roles: {response.ReasonPhrase}"
                    };
                }
            }
            catch (Exception ex)
            {
                return new ApiResponse<RolesApiResponse>
                {
                    IsSuccess = false,
                    Message = $"Error retrieving roles: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Get roles with user count
        /// </summary>
        /// <param name="pageNumber">Page number (default: 1)</param>
        /// <param name="pageSize">Page size (default: 10)</param>
        /// <param name="searchTerm">Search term for filtering</param>
        /// <param name="includeInactive">Include inactive roles</param>
        /// <returns>API response with roles with user count and pagination info</returns>
        public async Task<ApiResponse<RolesWithUserCountApiResponse>> GetRolesWithUserCountAsync(int pageNumber = 1, int pageSize = 10, string? searchTerm = null, bool includeInactive = true)
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

                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    queryParams.Add($"searchTerm={Uri.EscapeDataString(searchTerm)}");
                }

                var queryString = string.Join("&", queryParams);
                var response = await _httpClient.GetAsync($"api/role/with-user-count?{queryString}");

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<RolesWithUserCountApiResponse>();
                    return new ApiResponse<RolesWithUserCountApiResponse>
                    {
                        IsSuccess = true,
                        Message = "Roles with user count retrieved successfully",
                        Data = result
                    };
                }
                else
                {
                    return new ApiResponse<RolesWithUserCountApiResponse>
                    {
                        IsSuccess = false,
                        Message = $"Failed to retrieve roles with user count: {response.ReasonPhrase}"
                    };
                }
            }
            catch (Exception ex)
            {
                return new ApiResponse<RolesWithUserCountApiResponse>
                {
                    IsSuccess = false,
                    Message = $"Error retrieving roles with user count: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Get role by ID
        /// </summary>
        /// <param name="roleId">Role ID</param>
        /// <returns>API response with role information</returns>
        public async Task<ApiResponse<RoleInfoDto>> GetRoleByIdAsync(string roleId)
        {
            try
            {
                await SetAuthorizeHeader();

                var response = await _httpClient.GetAsync($"api/role/{Uri.EscapeDataString(roleId)}");

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<RoleInfoDto>();
                    return new ApiResponse<RoleInfoDto>
                    {
                        IsSuccess = true,
                        Message = "Role retrieved successfully",
                        Data = result
                    };
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return new ApiResponse<RoleInfoDto>
                    {
                        IsSuccess = false,
                        Message = "Role not found"
                    };
                }
                else
                {
                    return new ApiResponse<RoleInfoDto>
                    {
                        IsSuccess = false,
                        Message = $"Failed to retrieve role: {response.ReasonPhrase}"
                    };
                }
            }
            catch (Exception ex)
            {
                return new ApiResponse<RoleInfoDto>
                {
                    IsSuccess = false,
                    Message = $"Error retrieving role: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Get all active roles (for dropdown lists)
        /// </summary>
        /// <returns>API response with active roles</returns>
        public async Task<ApiResponse<List<RoleInfoDto>>> GetActiveRolesAsync()
        {
            try
            {
                await SetAuthorizeHeader();

                var response = await _httpClient.GetAsync("api/role/active");

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<List<RoleInfoDto>>();
                    return new ApiResponse<List<RoleInfoDto>>
                    {
                        IsSuccess = true,
                        Message = "Active roles retrieved successfully",
                        Data = result
                    };
                }
                else
                {
                    return new ApiResponse<List<RoleInfoDto>>
                    {
                        IsSuccess = false,
                        Message = $"Failed to retrieve active roles: {response.ReasonPhrase}"
                    };
                }
            }
            catch (Exception ex)
            {
                return new ApiResponse<List<RoleInfoDto>>
                {
                    IsSuccess = false,
                    Message = $"Error retrieving active roles: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Create new role
        /// </summary>
        /// <param name="createRoleDto">Role creation data</param>
        /// <returns>API response with created role information</returns>
        public async Task<ApiResponse<RoleInfoDto>> CreateRoleAsync(CreateRoleDto createRoleDto)
        {
            try
            {
                await SetAuthorizeHeader();

                var response = await _httpClient.PostAsJsonAsync("api/role", createRoleDto);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<RoleInfoDto>();
                    return new ApiResponse<RoleInfoDto>
                    {
                        IsSuccess = true,
                        Message = "Role created successfully",
                        Data = result
                    };
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return new ApiResponse<RoleInfoDto>
                    {
                        IsSuccess = false,
                        Message = $"Failed to create role: {response.ReasonPhrase}. {errorContent}"
                    };
                }
            }
            catch (Exception ex)
            {
                return new ApiResponse<RoleInfoDto>
                {
                    IsSuccess = false,
                    Message = $"Error creating role: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Delete role (soft delete)
        /// </summary>
        /// <param name="roleId">Role ID</param>
        /// <returns>API response indicating success or failure</returns>
        public async Task<ApiResponse<object>> DeleteRoleAsync(string roleId)
        {
            try
            {
                await SetAuthorizeHeader();

                var response = await _httpClient.DeleteAsync($"api/role/{Uri.EscapeDataString(roleId)}");

                if (response.IsSuccessStatusCode)
                {
                    return new ApiResponse<object>
                    {
                        IsSuccess = true,
                        Message = "Role deleted successfully"
                    };
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return new ApiResponse<object>
                    {
                        IsSuccess = false,
                        Message = "Role not found"
                    };
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return new ApiResponse<object>
                    {
                        IsSuccess = false,
                        Message = $"Failed to delete role: {response.ReasonPhrase}. {errorContent}"
                    };
                }
            }
            catch (Exception ex)
            {
                return new ApiResponse<object>
                {
                    IsSuccess = false,
                    Message = $"Error deleting role: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Permanently delete role
        /// </summary>
        /// <param name="roleId">Role ID</param>
        /// <returns>API response indicating success or failure</returns>
        public async Task<ApiResponse<object>> HardDeleteRoleAsync(string roleId)
        {
            try
            {
                await SetAuthorizeHeader();

                var response = await _httpClient.DeleteAsync($"api/role/{Uri.EscapeDataString(roleId)}/permanent");

                if (response.IsSuccessStatusCode)
                {
                    return new ApiResponse<object>
                    {
                        IsSuccess = true,
                        Message = "Role permanently deleted successfully"
                    };
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return new ApiResponse<object>
                    {
                        IsSuccess = false,
                        Message = "Role not found"
                    };
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return new ApiResponse<object>
                    {
                        IsSuccess = false,
                        Message = $"Failed to permanently delete role: {response.ReasonPhrase}. {errorContent}"
                    };
                }
            }
            catch (Exception ex)
            {
                return new ApiResponse<object>
                {
                    IsSuccess = false,
                    Message = $"Error permanently deleting role: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Check if role ID exists
        /// </summary>
        /// <param name="roleId">Role ID to check</param>
        /// <returns>API response with boolean indicating if role ID exists</returns>
        public async Task<ApiResponse<RoleIdCheckResponse>> CheckRoleIdAsync(string roleId)
        {
            try
            {
                await SetAuthorizeHeader();

                var response = await _httpClient.GetAsync($"api/role/check-role-id/{Uri.EscapeDataString(roleId)}");

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<RoleIdCheckResponse>();
                    return new ApiResponse<RoleIdCheckResponse>
                    {
                        IsSuccess = true,
                        Message = "Role ID check completed",
                        Data = result
                    };
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return new ApiResponse<RoleIdCheckResponse>
                    {
                        IsSuccess = false,
                        Message = $"Failed to check role ID: {response.ReasonPhrase}. {errorContent}"
                    };
                }
            }
            catch (Exception ex)
            {
                return new ApiResponse<RoleIdCheckResponse>
                {
                    IsSuccess = false,
                    Message = $"Error checking role ID: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Get users assigned to a specific role
        /// </summary>
        /// <param name="roleId">Role ID</param>
        /// <returns>API response with users in role</returns>
        public async Task<ApiResponse<UsersInRoleResponse>> GetUsersInRoleAsync(string roleId)
        {
            try
            {
                await SetAuthorizeHeader();

                var response = await _httpClient.GetAsync($"api/role/{Uri.EscapeDataString(roleId)}/users");

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<UsersInRoleResponse>();
                    return new ApiResponse<UsersInRoleResponse>
                    {
                        IsSuccess = true,
                        Message = "Users in role retrieved successfully",
                        Data = result
                    };
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return new ApiResponse<UsersInRoleResponse>
                    {
                        IsSuccess = false,
                        Message = "Role not found"
                    };
                }
                else
                {
                    return new ApiResponse<UsersInRoleResponse>
                    {
                        IsSuccess = false,
                        Message = $"Failed to retrieve users in role: {response.ReasonPhrase}"
                    };
                }
            }
            catch (Exception ex)
            {
                return new ApiResponse<UsersInRoleResponse>
                {
                    IsSuccess = false,
                    Message = $"Error retrieving users in role: {ex.Message}"
                };
            }
        }
    }

    // Supporting classes for API responses
    public class RolesApiResponse
    {
        [JsonPropertyName("roles")]
        public List<RoleInfoDto> Roles { get; set; } = new();

        [JsonPropertyName("pagination")]
        public RolesPaginationInfo Pagination { get; set; } = new();
    }

    public class RolesWithUserCountApiResponse
    {
        [JsonPropertyName("roles")]
        public List<RoleWithUserCountDto> Roles { get; set; } = new();

        [JsonPropertyName("pagination")]
        public RolesPaginationInfo Pagination { get; set; } = new();
    }

    public class RolesPaginationInfo
    {
        [JsonPropertyName("currentPage")]
        public int CurrentPage { get; set; }

        [JsonPropertyName("pageSize")]
        public int PageSize { get; set; }

        [JsonPropertyName("totalCount")]
        public int TotalCount { get; set; }

        [JsonPropertyName("totalPages")]
        public int TotalPages { get; set; }

        [JsonPropertyName("hasNextPage")]
        public bool HasNextPage { get; set; }

        [JsonPropertyName("hasPreviousPage")]
        public bool HasPreviousPage { get; set; }
    }

    public class RoleIdCheckResponse
    {
        [JsonPropertyName("roleId")]
        public string RoleId { get; set; } = string.Empty;

        [JsonPropertyName("exists")]
        public bool Exists { get; set; }
    }

    public class UsersInRoleResponse
    {
        [JsonPropertyName("roleId")]
        public string RoleId { get; set; } = string.Empty;

        [JsonPropertyName("users")]
        public List<UserInfoDto> Users { get; set; } = new();

        [JsonPropertyName("totalUsers")]
        public int TotalUsers { get; set; }
    }
}
