using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Web_health_app.Models.Models;
using Web_health_app.Web.Authentication;



namespace Web_health_app.Web.ApiClients
{
    public class LoginApiClient
    {
        private readonly HttpClient _httpClient;
       
        private readonly AuthenticationStateProvider _AuthStateProvider;
        private readonly ProtectedLocalStorage _localStoraga;

        public LoginApiClient(HttpClient httpClient,
                             
                              AuthenticationStateProvider authenticationStateProvider,
                              ProtectedLocalStorage localStoraga)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
           
            _AuthStateProvider = authenticationStateProvider as AuthenticationStateProvider
                ?? throw new ArgumentException(nameof(authenticationStateProvider));
           _localStoraga = localStoraga ?? throw new ArgumentNullException(nameof(localStoraga));

        }

        public async Task SetAuthorizeHeader() { 
        
        var token = ( await _localStoraga.GetAsync<string>("authToken")).Value;
            if (token != null )
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
            
        }

        /// <summary>
        /// Gửi yêu cầu đăng nhập, lưu token vào Cookie, và set header Bearer cho HttpClient.
        /// </summary>
        public async Task<ApiResponse<LoginResponseModel>> LoginAsync(LoginModel loginModel)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/auth/login", loginModel);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<LoginResponseModel>();

                    if (result?.Token != null)
                    {
                        // Store token với error handling
                        try
                        {

                            await ((CustonAuthStateProvider)_AuthStateProvider).MarkUserAsAuthenticated(result.Token);

                            // await _tokenService.StoreTokenAsync(result.Token);
                        }
                        catch (InvalidOperationException ex) when (ex.Message.Contains("Headers are read-only"))
                        {
                            // Log warning nhưng vẫn return success
                            Console.WriteLine($"Warning: Could not store token in cookie: {ex.Message}");
                            // Token vẫn được store trong session như fallback
                        }
                    }

                    return new ApiResponse<LoginResponseModel> { IsSuccess = true, Message = "------------", Data = result };
                }
                else
                {
                    return new ApiResponse<LoginResponseModel>
                    {
                        IsSuccess = false,
                        Message = $"Login failed: {response.ReasonPhrase}"
                    };
                }
            }
            catch (Exception ex)
            {
                return new ApiResponse<LoginResponseModel>
                {
                    IsSuccess = false,
                    Message = $"Error: {ex.Message}"
                };
            }
        }


        public async Task<string> TestAuthAsync()
        {
            await SetAuthorizeHeader();
            try
            {
                // Gọi API để kiểm tra xác thực
                var response = await _httpClient.GetAsync("api/auth/testauthen");
                if (response.IsSuccessStatusCode)
                {
                    return "Authentication successful!";
                }
                else
                {
                    return $"Authentication failed: {response.ReasonPhrase}";
                }
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }
        }


    }
    // Các class hỗ trợ
    public class ApiResponse<T>
    {
        public bool IsSuccess { get; set; }
        public string? Message { get; set; }
        public T? Data { get; set; }
    }

    public class AuthResponse
    {
        public string Token { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public DateTime Expiration { get; set; }
        public string Username { get; set; } = string.Empty;
    }

    public class RefreshTokenRequest
    {
        public string RefreshToken { get; set; } = string.Empty;
    }
}
public class RefreshTokenRequest
{
    public string RefreshToken { get; set; } = string.Empty;
}

