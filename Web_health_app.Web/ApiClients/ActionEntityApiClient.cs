using System.Text;
using System.Text.Json;
using Web_health_app.Models.Models;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using System.Net.Http.Headers;

namespace Web_health_app.Web.ApiClients
{
    public class ActionEntityApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly AuthenticationStateProvider _authStateProvider;
        private readonly ProtectedLocalStorage _localStorage;
        private readonly JsonSerializerOptions _jsonOptions;

        public ActionEntityApiClient(HttpClient httpClient,
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

        #region Action API Methods

        /// <summary>
        /// Get all actions with pagination
        /// </summary>
        public async Task<ApiResponse<ActionsApiResponse>> GetAllActionsAsync(int pageNumber = 1, int pageSize = 10, string? searchTerm = null, bool includeInactive = false)
        {
            try
            {
                await SetAuthorizeHeader();

                var query = $"?pageNumber={pageNumber}&pageSize={pageSize}";

                if (!string.IsNullOrEmpty(searchTerm))
                    query += $"&searchTerm={Uri.EscapeDataString(searchTerm)}";

                if (includeInactive)
                    query += "&includeInactive=true";

                var response = await _httpClient.GetAsync($"api/actionentity/actions{query}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<ActionsApiResponse>(content, _jsonOptions);

                    return new ApiResponse<ActionsApiResponse>
                    {
                        IsSuccess = true,
                        Message = "Actions retrieved successfully",
                        Data = result ?? new ActionsApiResponse()
                    };
                }
                else
                {
                    return new ApiResponse<ActionsApiResponse>
                    {
                        IsSuccess = false,
                        Message = $"Failed to retrieve actions: {response.StatusCode}",
                        Data = new ActionsApiResponse()
                    };
                }
            }
            catch (Exception ex)
            {
                return new ApiResponse<ActionsApiResponse>
                {
                    IsSuccess = false,
                    Message = $"Error retrieving actions: {ex.Message}",
                    Data = new ActionsApiResponse()
                };
            }
        }

        /// <summary>
        /// Get action by ID
        /// </summary>
        public async Task<ApiResponse<ActionInfoDto?>> GetActionByIdAsync(string actionId)
        {
            try
            {
                await SetAuthorizeHeader();

                var response = await _httpClient.GetAsync($"api/actionentity/actions/{actionId}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<ActionInfoDto>(content, _jsonOptions);

                    return new ApiResponse<ActionInfoDto?>
                    {
                        IsSuccess = true,
                        Message = "Action retrieved successfully",
                        Data = result
                    };
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return new ApiResponse<ActionInfoDto?>
                    {
                        IsSuccess = false,
                        Message = "Action not found",
                        Data = null
                    };
                }
                else
                {
                    return new ApiResponse<ActionInfoDto?>
                    {
                        IsSuccess = false,
                        Message = $"Failed to retrieve action: {response.StatusCode}",
                        Data = null
                    };
                }
            }
            catch (Exception ex)
            {
                return new ApiResponse<ActionInfoDto?>
                {
                    IsSuccess = false,
                    Message = $"Error retrieving action: {ex.Message}",
                    Data = null
                };
            }
        }

        /// <summary>
        /// Update action
        /// </summary>
        public async Task<ApiResponse<ActionInfoDto?>> UpdateActionAsync(string actionId, UpdateActionDto updateActionDto)
        {
            try
            {
                await SetAuthorizeHeader();

                var json = JsonSerializer.Serialize(updateActionDto, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PutAsync($"api/actionentity/actions/{actionId}", content);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<ActionInfoDto>(responseContent, _jsonOptions);

                    return new ApiResponse<ActionInfoDto?>
                    {
                        IsSuccess = true,
                        Message = "Action updated successfully",
                        Data = result
                    };
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return new ApiResponse<ActionInfoDto?>
                    {
                        IsSuccess = false,
                        Message = "Action not found",
                        Data = null
                    };
                }
                else
                {
                    return new ApiResponse<ActionInfoDto?>
                    {
                        IsSuccess = false,
                        Message = $"Failed to update action: {response.StatusCode}",
                        Data = null
                    };
                }
            }
            catch (Exception ex)
            {
                return new ApiResponse<ActionInfoDto?>
                {
                    IsSuccess = false,
                    Message = $"Error updating action: {ex.Message}",
                    Data = null
                };
            }
        }

        /// <summary>
        /// Get all active actions
        /// </summary>
        public async Task<ApiResponse<List<ActionInfoDto>>> GetActiveActionsAsync()
        {
            try
            {
                await SetAuthorizeHeader();

                var response = await _httpClient.GetAsync("api/actionentity/actions/active");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<ApiResponse<List<ActionInfoDto>>>(content, _jsonOptions);

                    return result;
                }
                else
                {
                    return new ApiResponse<List<ActionInfoDto>>
                    {
                        IsSuccess = false,
                        Message = $"Failed to retrieve active actions: {response.StatusCode}",
                        Data = new List<ActionInfoDto>()
                    };
                }
            }
            catch (Exception ex)
            {
                return new ApiResponse<List<ActionInfoDto>>
                {
                    IsSuccess = false,
                    Message = $"Error retrieving active actions: {ex.Message}",
                    Data = new List<ActionInfoDto>()
                };
            }
        }

        #endregion

        #region Entity API Methods

        /// <summary>
        /// Get all entities with pagination
        /// </summary>
        public async Task<ApiResponse<EntitiesApiResponse>> GetAllEntitiesAsync(int pageNumber = 1, int pageSize = 1000, string? searchTerm = null)
        {
            try
            {
                await SetAuthorizeHeader();

                var query = $"?pageNumber={pageNumber}&pageSize={pageSize}";

                if (!string.IsNullOrEmpty(searchTerm))
                    query += $"&searchTerm={Uri.EscapeDataString(searchTerm)}";

                var response = await _httpClient.GetAsync($"api/actionentity/entities{query}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<EntitiesApiResponse>(content, _jsonOptions);

                    return new ApiResponse<EntitiesApiResponse>
                    {
                        IsSuccess = true,
                        Message = "Entities retrieved successfully",
                        Data = result ?? new EntitiesApiResponse()
                    };
                }
                else
                {
                    return new ApiResponse<EntitiesApiResponse>
                    {
                        IsSuccess = false,
                        Message = $"Failed to retrieve entities: {response.StatusCode}",
                        Data = new EntitiesApiResponse()
                    };
                }
            }
            catch (Exception ex)
            {
                return new ApiResponse<EntitiesApiResponse>
                {
                    IsSuccess = false,
                    Message = $"Error retrieving entities: {ex.Message}",
                    Data = new EntitiesApiResponse()
                };
            }
        }

        /// <summary>
        /// Get entity by ID
        /// </summary>
        public async Task<ApiResponse<EntityInfoDto?>> GetEntityByIdAsync(string entityId)
        {
            try
            {
                await SetAuthorizeHeader();

                var response = await _httpClient.GetAsync($"api/actionentity/entities/{entityId}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<EntityInfoDto>(content, _jsonOptions);

                    return new ApiResponse<EntityInfoDto?>
                    {
                        IsSuccess = true,
                        Message = "Entity retrieved successfully",
                        Data = result
                    };
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return new ApiResponse<EntityInfoDto?>
                    {
                        IsSuccess = false,
                        Message = "Entity not found",
                        Data = null
                    };
                }
                else
                {
                    return new ApiResponse<EntityInfoDto?>
                    {
                        IsSuccess = false,
                        Message = $"Failed to retrieve entity: {response.StatusCode}",
                        Data = null
                    };
                }
            }
            catch (Exception ex)
            {
                return new ApiResponse<EntityInfoDto?>
                {
                    IsSuccess = false,
                    Message = $"Error retrieving entity: {ex.Message}",
                    Data = null
                };
            }
        }

        /// <summary>
        /// Update entity
        /// </summary>
        public async Task<ApiResponse<EntityInfoDto?>> UpdateEntityAsync(string entityId, UpdateEntityDto updateEntityDto)
        {
            try
            {
                await SetAuthorizeHeader();

                var json = JsonSerializer.Serialize(updateEntityDto, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PutAsync($"api/actionentity/entities/{entityId}", content);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<EntityInfoDto>(responseContent, _jsonOptions);

                    return new ApiResponse<EntityInfoDto?>
                    {
                        IsSuccess = true,
                        Message = "Entity updated successfully",
                        Data = result
                    };
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return new ApiResponse<EntityInfoDto?>
                    {
                        IsSuccess = false,
                        Message = "Entity not found",
                        Data = null
                    };
                }
                else
                {
                    return new ApiResponse<EntityInfoDto?>
                    {
                        IsSuccess = false,
                        Message = $"Failed to update entity: {response.StatusCode}",
                        Data = null
                    };
                }
            }
            catch (Exception ex)
            {
                return new ApiResponse<EntityInfoDto?>
                {
                    IsSuccess = false,
                    Message = $"Error updating entity: {ex.Message}",
                    Data = null
                };
            }
        }

        /// <summary>
        /// Get entities by minimum security level
        /// </summary>
        public async Task<ApiResponse<List<EntityInfoDto>>> GetEntitiesBySecurityLevelAsync(byte minSecurityLevel)
        {
            try
            {
                await SetAuthorizeHeader();

                var response = await _httpClient.GetAsync($"api/actionentity/entities/by-security-level/{minSecurityLevel}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<ApiResponse<List<EntityInfoDto>>>(content, _jsonOptions);

                    return result;
                }
                else
                {
                    return new ApiResponse<List<EntityInfoDto>>
                    {
                        IsSuccess = false,
                        Message = $"Failed to retrieve entities: {response.StatusCode}",
                        Data = new List<EntityInfoDto>()
                    };
                }
            }
            catch (Exception ex)
            {
                return new ApiResponse<List<EntityInfoDto>>
                {
                    IsSuccess = false,
                    Message = $"Error retrieving entities: {ex.Message}",
                    Data = new List<EntityInfoDto>()
                };
            }
        }

        #endregion
    }
}
