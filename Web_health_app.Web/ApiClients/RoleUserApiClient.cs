using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Web_health_app.Models.Models;
using Web_health_app.Web.Authentication;

namespace Web_health_app.Web.ApiClients
{
    public class RoleUserApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly AuthenticationStateProvider _authStateProvider;
        private readonly ProtectedLocalStorage _localStorage;

        public RoleUserApiClient(HttpClient httpClient,
                                AuthenticationStateProvider authenticationStateProvider,
                                ProtectedLocalStorage localStorage)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _authStateProvider = authenticationStateProvider as AuthenticationStateProvider
                ?? throw new ArgumentException(nameof(authenticationStateProvider));
            _localStorage = localStorage ?? throw new ArgumentNullException(nameof(localStorage));
        }

        /// <summary>
        /// Set authorization header with token from local storage
        /// </summary>
        public async Task SetAuthorizeHeader()
        {
            var token = (await _localStorage.GetAsync<string>("authToken")).Value;
            if (token != null)
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
        }

        /// <summary>
        /// Assign roles to user
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="roleIds">List of role IDs to assign</param>
        /// <returns>API response indicating success or failure</returns>
        public async Task<ApiResponse<object>> AssignRolesToUserAsync(Guid userId, List<string> roleIds)
        {
            await SetAuthorizeHeader();
            try
            {
                var assignmentDto = new UserRoleAssignmentDto
                {
                    UserId = userId,
                    RoleIds = roleIds
                };

                var response = await _httpClient.PostAsJsonAsync("api/roleuser/assign-roles-to-user", assignmentDto);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<dynamic>();
                    return new ApiResponse<object>
                    {
                        IsSuccess = true,
                        Message = "Roles assigned successfully",
                        Data = result
                    };
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return new ApiResponse<object>
                    {
                        IsSuccess = false,
                        Message = $"Failed to assign roles: {response.ReasonPhrase}",
                        Data = errorContent
                    };
                }
            }
            catch (Exception ex)
            {
                return new ApiResponse<object>
                {
                    IsSuccess = false,
                    Message = $"Error assigning roles: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Replace all roles for a user
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="roleIds">New list of role IDs</param>
        /// <returns>API response indicating success or failure</returns>
        public async Task<ApiResponse<object>> ReplaceUserRolesAsync(Guid userId, List<string> roleIds)
        {
            await SetAuthorizeHeader();
            try
            {
                var response = await _httpClient.PutAsJsonAsync($"api/roleuser/user/{userId}/roles", roleIds);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<dynamic>();
                    return new ApiResponse<object>
                    {
                        IsSuccess = true,
                        Message = "User roles updated successfully",
                        Data = result
                    };
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return new ApiResponse<object>
                    {
                        IsSuccess = false,
                        Message = $"Failed to update user roles: {response.ReasonPhrase}",
                        Data = errorContent
                    };
                }
            }
            catch (Exception ex)
            {
                return new ApiResponse<object>
                {
                    IsSuccess = false,
                    Message = $"Error updating user roles: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Get all roles assigned to a user
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>API response with user roles</returns>
        public async Task<ApiResponse<UserRolesResponse>> GetUserRolesAsync(Guid userId)
        {
            await SetAuthorizeHeader();
            try
            {
                var response = await _httpClient.GetAsync($"api/roleuser/user/{userId}/roles");

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<UserRolesResponse>();
                    return new ApiResponse<UserRolesResponse>
                    {
                        IsSuccess = true,
                        Message = "User roles retrieved successfully",
                        Data = result
                    };
                }
                else
                {
                    return new ApiResponse<UserRolesResponse>
                    {
                        IsSuccess = false,
                        Message = $"Failed to get user roles: {response.ReasonPhrase}"
                    };
                }
            }
            catch (Exception ex)
            {
                return new ApiResponse<UserRolesResponse>
                {
                    IsSuccess = false,
                    Message = $"Error getting user roles: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Remove roles from user
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="roleIds">List of role IDs to remove</param>
        /// <returns>API response indicating success or failure</returns>
        public async Task<ApiResponse<object>> RemoveRolesFromUserAsync(Guid userId, List<string> roleIds)
        {
            await SetAuthorizeHeader();
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Delete, $"api/roleuser/user/{userId}/roles")
                {
                    Content = JsonContent.Create(roleIds)
                };

                var response = await _httpClient.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<dynamic>();
                    return new ApiResponse<object>
                    {
                        IsSuccess = true,
                        Message = "Roles removed successfully",
                        Data = result
                    };
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return new ApiResponse<object>
                    {
                        IsSuccess = false,
                        Message = $"Failed to remove roles: {response.ReasonPhrase}",
                        Data = errorContent
                    };
                }
            }
            catch (Exception ex)
            {
                return new ApiResponse<object>
                {
                    IsSuccess = false,
                    Message = $"Error removing roles: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Check if user has specific role
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="roleId">Role ID</param>
        /// <returns>API response with boolean result</returns>
        public async Task<ApiResponse<UserHasRoleResponse>> CheckUserHasRoleAsync(Guid userId, string roleId)
        {
            await SetAuthorizeHeader();
            try
            {
                var response = await _httpClient.GetAsync($"api/roleuser/user/{userId}/has-role/{roleId}");

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<UserHasRoleResponse>();
                    return new ApiResponse<UserHasRoleResponse>
                    {
                        IsSuccess = true,
                        Message = "User role check completed",
                        Data = result
                    };
                }
                else
                {
                    return new ApiResponse<UserHasRoleResponse>
                    {
                        IsSuccess = false,
                        Message = $"Failed to check user role: {response.ReasonPhrase}"
                    };
                }
            }
            catch (Exception ex)
            {
                return new ApiResponse<UserHasRoleResponse>
                {
                    IsSuccess = false,
                    Message = $"Error checking user role: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Remove all roles from user
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>API response indicating success or failure</returns>
        public async Task<ApiResponse<object>> RemoveAllUserRolesAsync(Guid userId)
        {
            await SetAuthorizeHeader();
            try
            {
                var response = await _httpClient.DeleteAsync($"api/roleuser/user/{userId}/roles/all");

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<dynamic>();
                    return new ApiResponse<object>
                    {
                        IsSuccess = true,
                        Message = "All roles removed successfully",
                        Data = result
                    };
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return new ApiResponse<object>
                    {
                        IsSuccess = false,
                        Message = $"Failed to remove all roles: {response.ReasonPhrase}",
                        Data = errorContent
                    };
                }
            }
            catch (Exception ex)
            {
                return new ApiResponse<object>
                {
                    IsSuccess = false,
                    Message = $"Error removing all roles: {ex.Message}"
                };
            }
        }
    }

    // Supporting classes for API responses
    public class UserRolesResponse
    {
        [JsonPropertyName("userId")]
        public Guid UserId { get; set; }

        [JsonPropertyName("roles")]
        public List<RoleInfoDto> Roles { get; set; } = new();

        [JsonPropertyName("totalRoles")]
        public int TotalRoles { get; set; }
    }

    public class UserHasRoleResponse
    {
        [JsonPropertyName("userId")]
        public Guid UserId { get; set; }

        [JsonPropertyName("roleId")]
        public string RoleId { get; set; } = string.Empty;

        [JsonPropertyName("hasRole")]
        public bool HasRole { get; set; }
    }
}
