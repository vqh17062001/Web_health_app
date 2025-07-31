using System.Text;
using System.Text.Json;
using Web_health_app.Models.Models;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using System.Net.Http.Headers;

namespace Web_health_app.Web.ApiClients
{
    public class PermissionApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly AuthenticationStateProvider _authStateProvider;
        private readonly ProtectedLocalStorage _localStorage;
        private readonly JsonSerializerOptions _jsonOptions;

        public PermissionApiClient(HttpClient httpClient,
                                 AuthenticationStateProvider authStateProvider,
                                 ProtectedLocalStorage localStorage)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _authStateProvider = authStateProvider ?? throw new ArgumentNullException(nameof(authStateProvider));
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

        #region Permission CRUD Methods

        /// <summary>
        /// Get all permissions with pagination
        /// </summary>
        public async Task<ApiResponse<PermissionsApiResponse>> GetAllPermissionsAsync(int pageNumber = 1, int pageSize = 10, string? searchTerm = null)
        {
            try
            {
                await SetAuthorizeHeader();

                var query = $"?pageNumber={pageNumber}&pageSize={pageSize}";

                if (!string.IsNullOrEmpty(searchTerm))
                    query += $"&searchTerm={Uri.EscapeDataString(searchTerm)}";

                var response = await _httpClient.GetAsync($"api/permission{query}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<PermissionsApiResponse>(content, _jsonOptions);

                    return new ApiResponse<PermissionsApiResponse>
                    {
                        IsSuccess = true,
                        Message = "Permissions retrieved successfully",
                        Data = result ?? new PermissionsApiResponse()
                    };
                }
                else
                {
                    return new ApiResponse<PermissionsApiResponse>
                    {
                        IsSuccess = false,
                        Message = $"Failed to retrieve permissions: {response.StatusCode}",
                        Data = new PermissionsApiResponse()
                    };
                }
            }
            catch (Exception ex)
            {
                return new ApiResponse<PermissionsApiResponse>
                {
                    IsSuccess = false,
                    Message = $"Error retrieving permissions: {ex.Message}",
                    Data = new PermissionsApiResponse()
                };
            }
        }

        /// <summary>
        /// Get permission by ID
        /// </summary>
        public async Task<ApiResponse<PermissionInfoDto?>> GetPermissionByIdAsync(string permissionId)
        {
            try
            {
                await SetAuthorizeHeader();

                var response = await _httpClient.GetAsync($"api/permission/{permissionId}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<PermissionInfoDto>(content, _jsonOptions);

                    return new ApiResponse<PermissionInfoDto?>
                    {
                        IsSuccess = true,
                        Message = "Permission retrieved successfully",
                        Data = result
                    };
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return new ApiResponse<PermissionInfoDto?>
                    {
                        IsSuccess = false,
                        Message = "Permission not found",
                        Data = null
                    };
                }
                else
                {
                    return new ApiResponse<PermissionInfoDto?>
                    {
                        IsSuccess = false,
                        Message = $"Failed to retrieve permission: {response.StatusCode}",
                        Data = null
                    };
                }
            }
            catch (Exception ex)
            {
                return new ApiResponse<PermissionInfoDto?>
                {
                    IsSuccess = false,
                    Message = $"Error retrieving permission: {ex.Message}",
                    Data = null
                };
            }
        }

        /// <summary>
        /// Create a new permission
        /// </summary>
        public async Task<ApiResponse<PermissionInfoDto?>> CreatePermissionAsync(CreatePermissionDto createPermissionDto)
        {
            try
            {
                await SetAuthorizeHeader();

                var json = JsonSerializer.Serialize(createPermissionDto, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("api/permission", content);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<PermissionInfoDto>(responseContent, _jsonOptions);

                    return new ApiResponse<PermissionInfoDto?>
                    {
                        IsSuccess = true,
                        Message = "Permission created successfully",
                        Data = result
                    };
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return new ApiResponse<PermissionInfoDto?>
                    {
                        IsSuccess = false,
                        Message = $"Invalid data: {errorContent}",
                        Data = null
                    };
                }
                else
                {
                    return new ApiResponse<PermissionInfoDto?>
                    {
                        IsSuccess = false,
                        Message = $"Failed to create permission: {response.StatusCode}",
                        Data = null
                    };
                }
            }
            catch (Exception ex)
            {
                return new ApiResponse<PermissionInfoDto?>
                {
                    IsSuccess = false,
                    Message = $"Error creating permission: {ex.Message}",
                    Data = null
                };
            }
        }

        /// <summary>
        /// Update permission
        /// </summary>
        public async Task<ApiResponse<PermissionInfoDto?>> UpdatePermissionAsync(string permissionId, UpdatePermissionDto updatePermissionDto)
        {
            try
            {
                await SetAuthorizeHeader();

                var json = JsonSerializer.Serialize(updatePermissionDto, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PutAsync($"api/permission/{permissionId}", content);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<PermissionInfoDto>(responseContent, _jsonOptions);

                    return new ApiResponse<PermissionInfoDto?>
                    {
                        IsSuccess = true,
                        Message = "Permission updated successfully",
                        Data = result
                    };
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return new ApiResponse<PermissionInfoDto?>
                    {
                        IsSuccess = false,
                        Message = "Permission not found",
                        Data = null
                    };
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return new ApiResponse<PermissionInfoDto?>
                    {
                        IsSuccess = false,
                        Message = $"Invalid data: {errorContent}",
                        Data = null
                    };
                }
                else
                {
                    return new ApiResponse<PermissionInfoDto?>
                    {
                        IsSuccess = false,
                        Message = $"Failed to update permission: {response.StatusCode}",
                        Data = null
                    };
                }
            }
            catch (Exception ex)
            {
                return new ApiResponse<PermissionInfoDto?>
                {
                    IsSuccess = false,
                    Message = $"Error updating permission: {ex.Message}",
                    Data = null
                };
            }
        }

        /// <summary>
        /// Delete permission
        /// </summary>
        public async Task<ApiResponse<bool>> DeletePermissionAsync(string permissionId)
        {
            try
            {
                await SetAuthorizeHeader();

                var response = await _httpClient.DeleteAsync($"api/permission/{permissionId}/hard");

                if (response.IsSuccessStatusCode)
                {
                    return new ApiResponse<bool>
                    {
                        IsSuccess = true,
                        Message = "Permission deleted successfully",
                        Data = true
                    };
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return new ApiResponse<bool>
                    {
                        IsSuccess = false,
                        Message = "Permission not found",
                        Data = false
                    };
                }
                else
                {
                    return new ApiResponse<bool>
                    {
                        IsSuccess = false,
                        Message = $"Failed to delete permission: {response.StatusCode}",
                        Data = false
                    };
                }
            }
            catch (Exception ex)
            {
                return new ApiResponse<bool>
                {
                    IsSuccess = false,
                    Message = $"Error deleting permission: {ex.Message}",
                    Data = false
                };
            }
        }

        #endregion

        #region Permission Relations Methods

        /// <summary>
        /// Get permissions by role ID
        /// </summary>
        public async Task<ApiResponse<List<PermissionInfoDto>>> GetPermissionsByRoleIdAsync(string roleId)
        {
            try
            {
                await SetAuthorizeHeader();

                var response = await _httpClient.GetAsync($"api/permission/by-role/{roleId}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<ApiResponse<List<PermissionInfoDto>>>(content, _jsonOptions);

                    return result;
                }
                else
                {
                    return new ApiResponse<List<PermissionInfoDto>>
                    {
                        IsSuccess = false,
                        Message = $"Failed to retrieve role permissions: {response.StatusCode}",
                        Data = new List<PermissionInfoDto>()
                    };
                }
            }
            catch (Exception ex)
            {
                return new ApiResponse<List<PermissionInfoDto>>
                {
                    IsSuccess = false,
                    Message = $"Error retrieving role permissions: {ex.Message}",
                    Data = new List<PermissionInfoDto>()
                };
            }
        }

        /// <summary>
        /// Get available actions for permissions
        /// </summary>
        public async Task<ApiResponse<List<ActionInfoDto>>> GetAvailableActionsAsync()
        {
            try
            {
                await SetAuthorizeHeader();

                var response = await _httpClient.GetAsync("api/permission/actions");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<List<ActionInfoDto>>(content, _jsonOptions);

                    return new ApiResponse<List<ActionInfoDto>>
                    {
                        IsSuccess = true,
                        Message = "Available actions retrieved successfully",
                        Data = result ?? new List<ActionInfoDto>()
                    };
                }
                else
                {
                    return new ApiResponse<List<ActionInfoDto>>
                    {
                        IsSuccess = false,
                        Message = $"Failed to retrieve available actions: {response.StatusCode}",
                        Data = new List<ActionInfoDto>()
                    };
                }
            }
            catch (Exception ex)
            {
                return new ApiResponse<List<ActionInfoDto>>
                {
                    IsSuccess = false,
                    Message = $"Error retrieving available actions: {ex.Message}",
                    Data = new List<ActionInfoDto>()
                };
            }
        }

        /// <summary>
        /// Get available entities for permissions
        /// </summary>
        public async Task<ApiResponse<List<EntityInfoDto>>> GetAvailableEntitiesAsync()
        {
            try
            {
                await SetAuthorizeHeader();

                var response = await _httpClient.GetAsync("api/permission/entities");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<List<EntityInfoDto>>(content, _jsonOptions);

                    return new ApiResponse<List<EntityInfoDto>>
                    {
                        IsSuccess = true,
                        Message = "Available entities retrieved successfully",
                        Data = result ?? new List<EntityInfoDto>()
                    };
                }
                else
                {
                    return new ApiResponse<List<EntityInfoDto>>
                    {
                        IsSuccess = false,
                        Message = $"Failed to retrieve available entities: {response.StatusCode}",
                        Data = new List<EntityInfoDto>()
                    };
                }
            }
            catch (Exception ex)
            {
                return new ApiResponse<List<EntityInfoDto>>
                {
                    IsSuccess = false,
                    Message = $"Error retrieving available entities: {ex.Message}",
                    Data = new List<EntityInfoDto>()
                };
            }
        }

        /// <summary>
        /// Bulk create permissions for a role
        /// </summary>
        public async Task<ApiResponse<List<PermissionInfoDto>>> BulkCreatePermissionsAsync(string roleId, List<CreatePermissionDto> permissions)
        {
            try
            {
                await SetAuthorizeHeader();

                var json = JsonSerializer.Serialize(permissions, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync($"api/permission/bulk/{roleId}", content);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<List<PermissionInfoDto>>(responseContent, _jsonOptions);

                    return new ApiResponse<List<PermissionInfoDto>>
                    {
                        IsSuccess = true,
                        Message = "Permissions created successfully",
                        Data = result ?? new List<PermissionInfoDto>()
                    };
                }
                else
                {
                    return new ApiResponse<List<PermissionInfoDto>>
                    {
                        IsSuccess = false,
                        Message = $"Failed to create permissions: {response.StatusCode}",
                        Data = new List<PermissionInfoDto>()
                    };
                }
            }
            catch (Exception ex)
            {
                return new ApiResponse<List<PermissionInfoDto>>
                {
                    IsSuccess = false,
                    Message = $"Error creating permissions: {ex.Message}",
                    Data = new List<PermissionInfoDto>()
                };
            }
        }

        #endregion
    }
}
