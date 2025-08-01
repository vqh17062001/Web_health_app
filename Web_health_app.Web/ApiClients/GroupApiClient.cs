using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using System.Net.Http.Headers;
using System.Text.Json;
using Web_health_app.Models.Models;

namespace Web_health_app.Web.ApiClients
{
    public class GroupApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly AuthenticationStateProvider _authStateProvider;
        private readonly ProtectedLocalStorage _localStorage;

        public GroupApiClient(HttpClient httpClient,
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
        /// Get all groups with pagination and search
        /// </summary>
        /// <param name="pageNumber">Page number (default: 1)</param>
        /// <param name="pageSize">Page size (default: 10)</param>
        /// <param name="searchTerm">Search term for filtering</param>
        /// <param name="includeInactive">Include inactive groups</param>
        /// <returns>API response with groups and pagination info</returns>
        public async Task<ApiResponse<GroupsApiResponse>> GetAllGroupsAsync(int pageNumber = 1, int pageSize = 10, string? searchTerm = null, bool includeInactive = false)
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
                var response = await _httpClient.GetAsync($"api/group?{queryString}");

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<GroupsApiResponse>();
                    return new ApiResponse<GroupsApiResponse>
                    {
                        IsSuccess = true,
                        Message = "Groups retrieved successfully",
                        Data = result
                    };
                }
                else
                {
                    return new ApiResponse<GroupsApiResponse>
                    {
                        IsSuccess = false,
                        Message = $"Failed to retrieve groups: {response.ReasonPhrase}"
                    };
                }
            }
            catch (Exception ex)
            {
                return new ApiResponse<GroupsApiResponse>
                {
                    IsSuccess = false,
                    Message = $"Error retrieving groups: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Get group by ID
        /// </summary>
        /// <param name="groupId">Group ID</param>
        /// <returns>API response with group information</returns>
        public async Task<ApiResponse<GroupInfoDto>> GetGroupByIdAsync(string groupId)
        {
            try
            {
                await SetAuthorizeHeader();
                var response = await _httpClient.GetAsync($"api/group/{groupId}");

                if (response.IsSuccessStatusCode)
                {
                    var group = await response.Content.ReadFromJsonAsync<GroupInfoDto>();
                    return new ApiResponse<GroupInfoDto>
                    {
                        IsSuccess = true,
                        Message = "Group retrieved successfully",
                        Data = group
                    };
                }
                else
                {
                    return new ApiResponse<GroupInfoDto>
                    {
                        IsSuccess = false,
                        Message = $"Failed to retrieve group: {response.ReasonPhrase}"
                    };
                }
            }
            catch (Exception ex)
            {
                return new ApiResponse<GroupInfoDto>
                {
                    IsSuccess = false,
                    Message = $"Error retrieving group: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Get group with detailed information (users and roles)
        /// </summary>
        /// <param name="groupId">Group ID</param>
        /// <returns>API response with group detailed information</returns>
        public async Task<ApiResponse<GroupDetailDto>> GetGroupDetailAsync(string groupId)
        {
            try
            {
                await SetAuthorizeHeader();
                var response = await _httpClient.GetAsync($"api/group/{groupId}/detail");

                if (response.IsSuccessStatusCode)
                {
                    var groupDetail = await response.Content.ReadFromJsonAsync<GroupDetailDto>();
                    return new ApiResponse<GroupDetailDto>
                    {
                        IsSuccess = true,
                        Message = "Group details retrieved successfully",
                        Data = groupDetail
                    };
                }
                else
                {
                    return new ApiResponse<GroupDetailDto>
                    {
                        IsSuccess = false,
                        Message = $"Failed to retrieve group details: {response.ReasonPhrase}"
                    };
                }
            }
            catch (Exception ex)
            {
                return new ApiResponse<GroupDetailDto>
                {
                    IsSuccess = false,
                    Message = $"Error retrieving group details: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Get all active groups (for dropdown lists)
        /// </summary>
        /// <returns>API response with list of active groups</returns>
        public async Task<ApiResponse<List<GroupInfoDto>>> GetActiveGroupsAsync()
        {
            try
            {
                await SetAuthorizeHeader();
                var response = await _httpClient.GetAsync("api/group/active");

                if (response.IsSuccessStatusCode)
                {
                    var groups = await response.Content.ReadFromJsonAsync<List<GroupInfoDto>>();
                    return new ApiResponse<List<GroupInfoDto>>
                    {
                        IsSuccess = true,
                        Message = "Active groups retrieved successfully",
                        Data = groups ?? new List<GroupInfoDto>()
                    };
                }
                else
                {
                    return new ApiResponse<List<GroupInfoDto>>
                    {
                        IsSuccess = false,
                        Message = $"Failed to retrieve active groups: {response.ReasonPhrase}",
                        Data = new List<GroupInfoDto>()
                    };
                }
            }
            catch (Exception ex)
            {
                return new ApiResponse<List<GroupInfoDto>>
                {
                    IsSuccess = false,
                    Message = $"Error retrieving active groups: {ex.Message}",
                    Data = new List<GroupInfoDto>()
                };
            }
        }

        /// <summary>
        /// Create new group
        /// </summary>
        /// <param name="createGroupDto">Group creation data</param>
        /// <returns>API response with created group information</returns>
        public async Task<ApiResponse<GroupInfoDto>> CreateGroupAsync(CreateGroupDto createGroupDto)
        {
            try
            {
                await SetAuthorizeHeader();
                var response = await _httpClient.PostAsJsonAsync("api/group", createGroupDto);

                if (response.IsSuccessStatusCode)
                {
                    var createdGroup = await response.Content.ReadFromJsonAsync<GroupInfoDto>();
                    return new ApiResponse<GroupInfoDto>
                    {
                        IsSuccess = true,
                        Message = "Group created successfully",
                        Data = createdGroup
                    };
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return new ApiResponse<GroupInfoDto>
                    {
                        IsSuccess = false,
                        Message = $"Failed to create group: {response.ReasonPhrase}",
                        Data = null
                    };
                }
            }
            catch (Exception ex)
            {
                return new ApiResponse<GroupInfoDto>
                {
                    IsSuccess = false,
                    Message = $"Error creating group: {ex.Message}",
                    Data = null
                };
            }
        }

        /// <summary>
        /// Update group information
        /// </summary>
        /// <param name="groupId">Group ID</param>
        /// <param name="updateGroupDto">Updated group data</param>
        /// <returns>API response with updated group information</returns>
        public async Task<ApiResponse<GroupInfoDto>> UpdateGroupAsync(string groupId, UpdateGroupDto updateGroupDto)
        {
            try
            {
                await SetAuthorizeHeader();
                var response = await _httpClient.PutAsJsonAsync($"api/group/{groupId}", updateGroupDto);

                if (response.IsSuccessStatusCode)
                {
                    var updatedGroup = await response.Content.ReadFromJsonAsync<GroupInfoDto>();
                    return new ApiResponse<GroupInfoDto>
                    {
                        IsSuccess = true,
                        Message = "Group updated successfully",
                        Data = updatedGroup
                    };
                }
                else
                {
                    return new ApiResponse<GroupInfoDto>
                    {
                        IsSuccess = false,
                        Message = $"Failed to update group: {response.ReasonPhrase}",
                        Data = null
                    };
                }
            }
            catch (Exception ex)
            {
                return new ApiResponse<GroupInfoDto>
                {
                    IsSuccess = false,
                    Message = $"Error updating group: {ex.Message}",
                    Data = null
                };
            }
        }

        /// <summary>
        /// Delete group (soft delete)
        /// </summary>
        /// <param name="groupId">Group ID</param>
        /// <returns>API response indicating success or failure</returns>
        public async Task<ApiResponse<object>> DeleteGroupAsync(string groupId)
        {
            try
            {
                await SetAuthorizeHeader();
                var response = await _httpClient.DeleteAsync($"api/group/{groupId}");

                if (response.IsSuccessStatusCode)
                {
                    return new ApiResponse<object>
                    {
                        IsSuccess = true,
                        Message = "Group deleted successfully"
                    };
                }
                else
                {
                    return new ApiResponse<object>
                    {
                        IsSuccess = false,
                        Message = $"Failed to delete group: {response.ReasonPhrase}"
                    };
                }
            }
            catch (Exception ex)
            {
                return new ApiResponse<object>
                {
                    IsSuccess = false,
                    Message = $"Error deleting group: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Permanently delete group
        /// </summary>
        /// <param name="groupId">Group ID</param>
        /// <returns>API response indicating success or failure</returns>
        public async Task<ApiResponse<object>> HardDeleteGroupAsync(string groupId)
        {
            try
            {
                await SetAuthorizeHeader();
                var response = await _httpClient.DeleteAsync($"api/group/{groupId}/permanent");

                if (response.IsSuccessStatusCode)
                {
                    return new ApiResponse<object>
                    {
                        IsSuccess = true,
                        Message = "Group permanently deleted successfully"
                    };
                }
                else
                {
                    return new ApiResponse<object>
                    {
                        IsSuccess = false,
                        Message = $"Failed to permanently delete group: {response.ReasonPhrase}"
                    };
                }
            }
            catch (Exception ex)
            {
                return new ApiResponse<object>
                {
                    IsSuccess = false,
                    Message = $"Error permanently deleting group: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Check if group ID exists
        /// </summary>
        /// <param name="groupId">Group ID to check</param>
        /// <returns>API response with boolean indicating if group ID exists</returns>
        public async Task<ApiResponse<bool>> CheckGroupIdExistsAsync(string groupId)
        {
            try
            {
                await SetAuthorizeHeader();
                var response = await _httpClient.GetAsync($"api/group/check-group-id/{groupId}");

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<dynamic>();
                    return new ApiResponse<bool>
                    {
                        IsSuccess = true,
                        Message = "Group ID check completed",
                        Data = result?.exists ?? false
                    };
                }
                else
                {
                    return new ApiResponse<bool>
                    {
                        IsSuccess = false,
                        Message = $"Failed to check group ID: {response.ReasonPhrase}",
                        Data = false
                    };
                }
            }
            catch (Exception ex)
            {
                return new ApiResponse<bool>
                {
                    IsSuccess = false,
                    Message = $"Error checking group ID: {ex.Message}",
                    Data = false
                };
            }
        }

        /// <summary>
        /// Get groups with user count
        /// </summary>
        /// <param name="pageNumber">Page number (default: 1)</param>
        /// <param name="pageSize">Page size (default: 10)</param>
        /// <param name="searchTerm">Search term for filtering</param>
        /// <param name="includeInactive">Include inactive groups</param>
        /// <returns>API response with groups with user count and pagination info</returns>
        public async Task<ApiResponse<GroupsWithUserCountApiResponse>> GetGroupsWithUserCountAsync(int pageNumber = 1, int pageSize = 10, string? searchTerm = null, bool includeInactive = true)
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
                var response = await _httpClient.GetAsync($"api/group/with-user-count?{queryString}");

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<GroupsWithUserCountApiResponse>();
                    return new ApiResponse<GroupsWithUserCountApiResponse>
                    {
                        IsSuccess = true,
                        Message = "Groups with user count retrieved successfully",
                        Data = result
                    };
                }
                else
                {
                    return new ApiResponse<GroupsWithUserCountApiResponse>
                    {
                        IsSuccess = false,
                        Message = $"Failed to retrieve groups with user count: {response.ReasonPhrase}"
                    };
                }
            }
            catch (Exception ex)
            {
                return new ApiResponse<GroupsWithUserCountApiResponse>
                {
                    IsSuccess = false,
                    Message = $"Error retrieving groups with user count: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Add users to group
        /// </summary>
        /// <param name="groupId">Group ID</param>
        /// <param name="userIds">List of user IDs to add</param>
        /// <returns>API response indicating success or failure</returns>
        public async Task<ApiResponse<object>> AddUsersToGroupAsync(string groupId, List<Guid> userIds)
        {
            try
            {
                await SetAuthorizeHeader();
                var assignmentDto = new GroupUserAssignmentDto
                {
                    GroupId = groupId,
                    UserIds = userIds
                };

                var response = await _httpClient.PostAsJsonAsync($"api/group/{groupId}/users", assignmentDto);

                if (response.IsSuccessStatusCode)
                {
                    return new ApiResponse<object>
                    {
                        IsSuccess = true,
                        Message = "Users added to group successfully"
                    };
                }
                else
                {
                    return new ApiResponse<object>
                    {
                        IsSuccess = false,
                        Message = $"Failed to add users to group: {response.ReasonPhrase}"
                    };
                }
            }
            catch (Exception ex)
            {
                return new ApiResponse<object>
                {
                    IsSuccess = false,
                    Message = $"Error adding users to group: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Remove users from their groups
        /// </summary>
        /// <param name="userIds">List of user IDs to remove from groups</param>
        /// <returns>API response indicating success or failure</returns>
        public async Task<ApiResponse<object>> RemoveUsersFromGroupAsync(List<Guid> userIds)
        {
            try
            {
                await SetAuthorizeHeader();
                var response = await _httpClient.DeleteAsync("api/group/users");

                var request = new HttpRequestMessage(HttpMethod.Delete, "api/group/users")
                {
                    Content = JsonContent.Create(userIds)
                };
                response = await _httpClient.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    return new ApiResponse<object>
                    {
                        IsSuccess = true,
                        Message = "Users removed from groups successfully"
                    };
                }
                else
                {
                    return new ApiResponse<object>
                    {
                        IsSuccess = false,
                        Message = $"Failed to remove users from groups: {response.ReasonPhrase}"
                    };
                }
            }
            catch (Exception ex)
            {
                return new ApiResponse<object>
                {
                    IsSuccess = false,
                    Message = $"Error removing users from groups: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Remove specific users from a specific group
        /// </summary>
        /// <param name="groupId">Group ID</param>
        /// <param name="userIds">List of user IDs to remove from the group</param>
        /// <returns>API response indicating success or failure</returns>
        public async Task<ApiResponse<object>> RemoveUsersFromSpecificGroupAsync(string groupId, List<Guid> userIds)
        {
            try
            {
                await SetAuthorizeHeader();

                var request = new HttpRequestMessage(HttpMethod.Delete, $"api/group/{groupId}/users")
                {
                    Content = JsonContent.Create(userIds)
                };
                var response = await _httpClient.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    return new ApiResponse<object>
                    {
                        IsSuccess = true,
                        Message = "Users removed from group successfully"
                    };
                }
                else
                {
                    return new ApiResponse<object>
                    {
                        IsSuccess = false,
                        Message = $"Failed to remove users from group: {response.ReasonPhrase}"
                    };
                }
            }
            catch (Exception ex)
            {
                return new ApiResponse<object>
                {
                    IsSuccess = false,
                    Message = $"Error removing users from group: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Get all users in a group
        /// </summary>
        /// <param name="groupId">Group ID</param>
        /// <returns>API response with list of users in the group</returns>
        public async Task<ApiResponse<List<UserInfoDto>>> GetGroupUsersAsync(string groupId)
        {
            try
            {
                await SetAuthorizeHeader();
                var response = await _httpClient.GetAsync($"api/group/{groupId}/users");

                if (response.IsSuccessStatusCode)
                {
                    var users = await response.Content.ReadFromJsonAsync<List<UserInfoDto>>();
                    return new ApiResponse<List<UserInfoDto>>
                    {
                        IsSuccess = true,
                        Message = "Group users retrieved successfully",
                        Data = users ?? new List<UserInfoDto>()
                    };
                }
                else
                {
                    return new ApiResponse<List<UserInfoDto>>
                    {
                        IsSuccess = false,
                        Message = $"Failed to retrieve group users: {response.ReasonPhrase}",
                        Data = new List<UserInfoDto>()
                    };
                }
            }
            catch (Exception ex)
            {
                return new ApiResponse<List<UserInfoDto>>
                {
                    IsSuccess = false,
                    Message = $"Error retrieving group users: {ex.Message}",
                    Data = new List<UserInfoDto>()
                };
            }
        }

        /// <summary>
        /// Get all groups that a user belongs to
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>API response with list of groups the user belongs to</returns>
        public async Task<ApiResponse<List<GroupInfoDto>>> GetUserGroupsAsync(Guid userId)
        {
            try
            {
                await SetAuthorizeHeader();
                var response = await _httpClient.GetAsync($"api/group/user/{userId}");

                if (response.IsSuccessStatusCode)
                {
                    var groups = await response.Content.ReadFromJsonAsync<List<GroupInfoDto>>();
                    return new ApiResponse<List<GroupInfoDto>>
                    {
                        IsSuccess = true,
                        Message = "User groups retrieved successfully",
                        Data = groups ?? new List<GroupInfoDto>()
                    };
                }
                else
                {
                    return new ApiResponse<List<GroupInfoDto>>
                    {
                        IsSuccess = false,
                        Message = $"Failed to retrieve user groups: {response.ReasonPhrase}",
                        Data = new List<GroupInfoDto>()
                    };
                }
            }
            catch (Exception ex)
            {
                return new ApiResponse<List<GroupInfoDto>>
                {
                    IsSuccess = false,
                    Message = $"Error retrieving user groups: {ex.Message}",
                    Data = new List<GroupInfoDto>()
                };
            }
        }

        /// <summary>
        /// Move user to different group
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="newGroupId">New group ID (null to remove from all groups)</param>
        /// <returns>API response indicating success or failure</returns>
        public async Task<ApiResponse<object>> MoveUserToGroupAsync(Guid userId, string? newGroupId)
        {
            try
            {
                await SetAuthorizeHeader();
                var response = await _httpClient.PutAsJsonAsync($"api/group/user/{userId}/move", newGroupId);

                if (response.IsSuccessStatusCode)
                {
                    var message = newGroupId == null
                        ? "User removed from all groups successfully"
                        : "User moved to new group successfully";

                    return new ApiResponse<object>
                    {
                        IsSuccess = true,
                        Message = message
                    };
                }
                else
                {
                    return new ApiResponse<object>
                    {
                        IsSuccess = false,
                        Message = $"Failed to move user to group: {response.ReasonPhrase}"
                    };
                }
            }
            catch (Exception ex)
            {
                return new ApiResponse<object>
                {
                    IsSuccess = false,
                    Message = $"Error moving user to group: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Search groups with advanced filters
        /// </summary>
        /// <param name="searchDto">Search criteria</param>
        /// <param name="pageNumber">Page number (default: 1)</param>
        /// <param name="pageSize">Page size (default: 10)</param>
        /// <returns>API response with filtered groups and pagination</returns>
        public async Task<ApiResponse<GroupsApiResponse>> SearchGroupsAsync(GroupSearchDto searchDto, int pageNumber = 1, int pageSize = 10)
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
                var response = await _httpClient.PostAsJsonAsync($"api/group/search?{queryString}", searchDto);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<GroupsApiResponse>();
                    return new ApiResponse<GroupsApiResponse>
                    {
                        IsSuccess = true,
                        Message = "Groups search completed successfully",
                        Data = result
                    };
                }
                else
                {
                    return new ApiResponse<GroupsApiResponse>
                    {
                        IsSuccess = false,
                        Message = $"Failed to search groups: {response.ReasonPhrase}"
                    };
                }
            }
            catch (Exception ex)
            {
                return new ApiResponse<GroupsApiResponse>
                {
                    IsSuccess = false,
                    Message = $"Error searching groups: {ex.Message}"
                };
            }
        }
        /// <summary>
        /// Add roles to group
        /// </summary>
        /// <param name="groupId">Group ID</param>
        /// <param name="roleIds">List of role IDs to add</param>
        /// <param name="note">Optional note for the assignment</param>
        /// <returns>API response indicating success or failure</returns>
        public async Task<ApiResponse<object>> AddRolesToGroupAsync(string groupId, List<string> roleIds, string? note = null)
        {
            try
            {
                await SetAuthorizeHeader();
                var assignmentDto = new GroupRoleAssignmentDto
                {
                    GroupId = groupId,
                    RoleIds = roleIds,
                    Note = note
                };

                var response = await _httpClient.PostAsJsonAsync($"api/group/{groupId}/roles", assignmentDto);

                if (response.IsSuccessStatusCode)
                {
                    return new ApiResponse<object>
                    {
                        IsSuccess = true,
                        Message = "Roles added to group successfully"
                    };
                }
                else
                {
                    return new ApiResponse<object>
                    {
                        IsSuccess = false,
                        Message = $"Failed to add roles to group: {response.ReasonPhrase}"
                    };
                }
            }
            catch (Exception ex)
            {
                return new ApiResponse<object>
                {
                    IsSuccess = false,
                    Message = $"Error adding roles to group: {ex.Message}"
                };
            }
        }
    }
}
