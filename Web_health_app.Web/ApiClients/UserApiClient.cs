using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Web_health_app.Models.Models;
using Web_health_app.Web.Authentication;

namespace Web_health_app.Web.ApiClients
{
    public class UserApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly AuthenticationStateProvider _authStateProvider;
        private readonly ProtectedLocalStorage _localStorage;

        public UserApiClient(HttpClient httpClient,
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
        /// Get all users with pagination and search
        /// </summary>
        /// <param name="pageNumber">Page number (default: 1)</param>
        /// <param name="pageSize">Page size (default: 10)</param>
        /// <param name="searchTerm">Search term for filtering</param>
        /// <returns>API response with users and pagination info</returns>
        public async Task<ApiResponse<UsersApiResponse>> GetAllUsersAsync(int pageNumber = 1, int pageSize = 10, string? searchTerm = null)
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

                var queryString = string.Join("&", queryParams);
                var response = await _httpClient.GetAsync($"api/user?{queryString}");

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<UsersApiResponse>();
                    return new ApiResponse<UsersApiResponse>
                    {
                        IsSuccess = true,
                        Message = "Users retrieved successfully",
                        Data = result
                    };
                }
                else
                {
                    return new ApiResponse<UsersApiResponse>
                    {
                        IsSuccess = false,
                        Message = $"Failed to retrieve users: {response.ReasonPhrase}"
                    };
                }
            }
            catch (Exception ex)
            {
                return new ApiResponse<UsersApiResponse>
                {
                    IsSuccess = false,
                    Message = $"Error retrieving users: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Get user by ID
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>API response with user information</returns>
        public async Task<ApiResponse<UserInfoDto>> GetUserByIdAsync(Guid userId)
        {
            try
            {
                await SetAuthorizeHeader();

                var response = await _httpClient.GetAsync($"api/user/{userId}");

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<UserInfoDto>();
                    return new ApiResponse<UserInfoDto>
                    {
                        IsSuccess = true,
                        Message = "User retrieved successfully",
                        Data = result
                    };
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return new ApiResponse<UserInfoDto>
                    {
                        IsSuccess = false,
                        Message = "User not found"
                    };
                }
                else
                {
                    return new ApiResponse<UserInfoDto>
                    {
                        IsSuccess = false,
                        Message = $"Failed to retrieve user: {response.ReasonPhrase}"
                    };
                }
            }
            catch (Exception ex)
            {
                return new ApiResponse<UserInfoDto>
                {
                    IsSuccess = false,
                    Message = $"Error retrieving user: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Get user by username
        /// </summary>
        /// <param name="username">Username</param>
        /// <returns>API response with user information</returns>
        public async Task<ApiResponse<UserInfoDto>> GetUserByUsernameAsync(string username)
        {
            try
            {
                await SetAuthorizeHeader();

                var response = await _httpClient.GetAsync($"api/user/username/{Uri.EscapeDataString(username)}");

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<UserInfoDto>();
                    return new ApiResponse<UserInfoDto>
                    {
                        IsSuccess = true,
                        Message = "User retrieved successfully",
                        Data = result
                    };
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return new ApiResponse<UserInfoDto>
                    {
                        IsSuccess = false,
                        Message = "User not found"
                    };
                }
                else
                {
                    return new ApiResponse<UserInfoDto>
                    {
                        IsSuccess = false,
                        Message = $"Failed to retrieve user: {response.ReasonPhrase}"
                    };
                }
            }
            catch (Exception ex)
            {
                return new ApiResponse<UserInfoDto>
                {
                    IsSuccess = false,
                    Message = $"Error retrieving user: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Create new user
        /// </summary>
        /// <param name="createUserDto">User creation data</param>
        /// <returns>API response with created user information</returns>
        public async Task<ApiResponse<UserInfoDto>> CreateUserAsync(CreateUserDto createUserDto)
        {
            try
            {
                await SetAuthorizeHeader();

                var response = await _httpClient.PostAsJsonAsync("api/user", createUserDto);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<UserInfoDto>();
                    return new ApiResponse<UserInfoDto>
                    {
                        IsSuccess = true,
                        Message = "User created successfully",
                        Data = result
                    };
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return new ApiResponse<UserInfoDto>
                    {
                        IsSuccess = false,
                        Message = $"Failed to create user: {response.ReasonPhrase}. {errorContent}"
                    };
                }
            }
            catch (Exception ex)
            {
                return new ApiResponse<UserInfoDto>
                {
                    IsSuccess = false,
                    Message = $"Error creating user: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Update user information
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="updateUserDto">Updated user data</param>
        /// <returns>API response with updated user information</returns>
        public async Task<ApiResponse<UserInfoDto>> UpdateUserAsync(Guid userId, UpdateUserDto updateUserDto)
        {
            try
            {
                await SetAuthorizeHeader();

                var response = await _httpClient.PutAsJsonAsync($"api/user/{userId}", updateUserDto);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<UserInfoDto>();
                    return new ApiResponse<UserInfoDto>
                    {
                        IsSuccess = true,
                        Message = "User updated successfully",
                        Data = result
                    };
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return new ApiResponse<UserInfoDto>
                    {
                        IsSuccess = false,
                        Message = "User not found"
                    };
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return new ApiResponse<UserInfoDto>
                    {
                        IsSuccess = false,
                        Message = $"Failed to update user: {response.ReasonPhrase}. {errorContent}"
                    };
                }
            }
            catch (Exception ex)
            {
                return new ApiResponse<UserInfoDto>
                {
                    IsSuccess = false,
                    Message = $"Error updating user: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Delete user (soft delete)
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>API response indicating success or failure</returns>
        public async Task<ApiResponse<object>> DeleteUserAsync(Guid userId)
        {
            try
            {
                await SetAuthorizeHeader();

                var response = await _httpClient.DeleteAsync($"api/user/{userId}");

                if (response.IsSuccessStatusCode)
                {
                    return new ApiResponse<object>
                    {
                        IsSuccess = true,
                        Message = "User deleted successfully"
                    };
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return new ApiResponse<object>
                    {
                        IsSuccess = false,
                        Message = "User not found"
                    };
                }
                else
                {
                    return new ApiResponse<object>
                    {
                        IsSuccess = false,
                        Message = $"Failed to delete user: {response.ReasonPhrase}"
                    };
                }
            }
            catch (Exception ex)
            {
                return new ApiResponse<object>
                {
                    IsSuccess = false,
                    Message = $"Error deleting user: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Permanently delete user
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>API response indicating success or failure</returns>
        public async Task<ApiResponse<object>> HardDeleteUserAsync(Guid userId)
        {
            try
            {
                await SetAuthorizeHeader();

                var response = await _httpClient.DeleteAsync($"api/user/{userId}/permanent");

                if (response.IsSuccessStatusCode)
                {
                    return new ApiResponse<object>
                    {
                        IsSuccess = true,
                        Message = "User permanently deleted successfully"
                    };
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return new ApiResponse<object>
                    {
                        IsSuccess = false,
                        Message = "User not found"
                    };
                }
                else
                {
                    return new ApiResponse<object>
                    {
                        IsSuccess = false,
                        Message = $"Failed to permanently delete user: {response.ReasonPhrase}"
                    };
                }
            }
            catch (Exception ex)
            {
                return new ApiResponse<object>
                {
                    IsSuccess = false,
                    Message = $"Error permanently deleting user: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Check if username exists
        /// </summary>
        /// <param name="username">Username to check</param>
        /// <returns>API response with boolean indicating if username exists</returns>
        public async Task<ApiResponse<UsernameCheckResponse>> CheckUsernameAsync(string username)
        {
            try
            {
                await SetAuthorizeHeader();

                var response = await _httpClient.GetAsync($"api/user/check-username/{Uri.EscapeDataString(username)}");

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<UsernameCheckResponse>();
                    return new ApiResponse<UsernameCheckResponse>
                    {
                        IsSuccess = true,
                        Message = "Username check completed",
                        Data = result
                    };
                }
                else
                {
                    return new ApiResponse<UsernameCheckResponse>
                    {
                        IsSuccess = false,
                        Message = $"Failed to check username: {response.ReasonPhrase}"
                    };
                }
            }
            catch (Exception ex)
            {
                return new ApiResponse<UsernameCheckResponse>
                {
                    IsSuccess = false,
                    Message = $"Error checking username: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Get current user information from JWT token
        /// </summary>
        /// <returns>API response with current user information</returns>
        public async Task<ApiResponse<UserInfoDto>> GetCurrentUserAsync()
        {
            try
            {
                await SetAuthorizeHeader();

                var response = await _httpClient.GetAsync("api/user/me");

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<UserInfoDto>();
                    return new ApiResponse<UserInfoDto>
                    {
                        IsSuccess = true,
                        Message = "Current user retrieved successfully",
                        Data = result
                    };
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return new ApiResponse<UserInfoDto>
                    {
                        IsSuccess = false,
                        Message = "Current user not found"
                    };
                }
                else
                {
                    return new ApiResponse<UserInfoDto>
                    {
                        IsSuccess = false,
                        Message = $"Failed to retrieve current user: {response.ReasonPhrase}"
                    };
                }
            }
            catch (Exception ex)
            {
                return new ApiResponse<UserInfoDto>
                {
                    IsSuccess = false,
                    Message = $"Error retrieving current user: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Get users by comparing security level
        /// </summary>
        /// <param name="level">Security level to compare</param>
        /// <returns>API response with list of users</returns>
        public async Task<ApiResponse<List<UserInfoDto>>> GetUsersByCompareSecurityLevelAsync(int level)
        {
            await SetAuthorizeHeader();
            try
            {
                var response = await _httpClient.GetAsync($"api/user/GetUsersByCompareSecurityLevel/{level}");

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<List<UserInfoDto>>();
                    return new ApiResponse<List<UserInfoDto>>
                    {
                        IsSuccess = true,
                        Message = "Users retrieved successfully",
                        Data = result
                    };
                }
                else
                {
                    return new ApiResponse<List<UserInfoDto>>
                    {
                        IsSuccess = false,
                        Message = $"Failed to get users by security level: {response.ReasonPhrase}"
                    };
                }
            }
            catch (Exception ex)
            {
                return new ApiResponse<List<UserInfoDto>>
                {
                    IsSuccess = false,
                    Message = $"Error getting users by security level: {ex.Message}"
                };
            }
        }


        public async Task<ActionResult<bool>> FirstChangePassword(ChangePasswordModel changePasswordModel)
        {


            await SetAuthorizeHeader();

            try
            {

                var response = await _httpClient.PostAsJsonAsync("api/user/firstchangepassword", changePasswordModel);
                if (response.IsSuccessStatusCode)
                {
                    return true; // Password changed successfully
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Failed to change password: {response.ReasonPhrase}. {errorContent}");
                    return false; // Failed to change password
                }



            }
            catch (Exception ex)
            {

                return false; // Error occurred while changing password

            }



        }


        public async Task<ActionResult<bool>> ChangePassword(ChangePasswordModel changePasswordModel)
        {


            await SetAuthorizeHeader();

            try
            {

                var response = await _httpClient.PostAsJsonAsync("api/user/changepassword", changePasswordModel);
                if (response.IsSuccessStatusCode)
                {
                    return true; // Password changed successfully
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Failed to change password: {response.ReasonPhrase}. {errorContent}");
                    return false; // Failed to change password
                }

            }
            catch (Exception ex)
            {

                return false; // Error occurred while changing password

            }



        }

        /// <summary>
        /// Search users with advanced filtering
        /// </summary>
        /// <param name="searchDto">Search criteria</param>
        /// <param name="pageNumber">Page number (default: 1)</param>
        /// <param name="pageSize">Page size (default: 10)</param>
        /// <returns>Filtered users response</returns>
        public async Task<ApiResponse<UsersApiResponse?>> SearchUsersAsync(UserSearchDto searchDto , int pageNumber = 1, int pageSize = 10)
        {
            try
            {
                await SetAuthorizeHeader();

              
                var response = await _httpClient.PostAsJsonAsync($"api/User/search?pageNumber={pageNumber}&pageSize={pageSize}",  searchDto);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<UsersApiResponse>();

                    return new ApiResponse<UsersApiResponse>
                    {
                        IsSuccess = true,
                        Message = "Users retrieved successfully",
                        Data = result
                    };
                }
                else
                {
                    Console.WriteLine($"Error searching users: {response.StatusCode} - {response.ReasonPhrase}");
                    return null;
                }



               

                
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in SearchUsersAsync: {ex.Message}");
                return null;
            }
        }


    }

    // Supporting classes for API responses
    public class UsersApiResponse
    {
        [JsonPropertyName("users")]
        public List<UserInfoDto> Users { get; set; } = new();

        [JsonPropertyName("pagination")]
        public UsersPaginationInfo Pagination { get; set; } = new();
    }

    public class UsersPaginationInfo
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

    public class UsernameCheckResponse
    {
        [JsonPropertyName("username")]
        public string Username { get; set; } = string.Empty;

        [JsonPropertyName("exists")]
        public bool Exists { get; set; }
    }

    // Generic API Response class (if not already defined elsewhere)

}
