using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Web_health_app.ApiService.Models;
using Web_health_app.Web.Services;

namespace Web_health_app.Web.ApiClients
{
    public class LoginApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly IJwtTokenService _tokenService;

        public LoginApiClient(HttpClient httpClient,
                              IJwtTokenService tokenService)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
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

                    if (result?.Token != null )
                    {
                        // Store token với error handling
                        try
                        {
                            await _tokenService.StoreTokenAsync(result.Token);
                        }
                        catch (InvalidOperationException ex) when (ex.Message.Contains("Headers are read-only"))
                        {
                            // Log warning nhưng vẫn return success
                            Console.WriteLine($"Warning: Could not store token in cookie: {ex.Message}");
                            // Token vẫn được store trong session như fallback
                        }
                    }

                    return  new ApiResponse<LoginResponseModel> { IsSuccess = true, Message = "------------" ,Data =result  };
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

        /// <summary>
        /// Xoá token trên client và Cookie.
        /// </summary>
        public async Task<ApiResponse<bool>> LogoutAsync()
        {
            try
            {
                // Xoá token từ cookie
                await _tokenService.RemoveTokenAsync();

                // Bỏ header Authorization
                _httpClient.DefaultRequestHeaders.Authorization = null;

                return new ApiResponse<bool>
                {
                    IsSuccess = true,
                    Data = true
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<bool>
                {
                    IsSuccess = false,
                    Message = $"Có lỗi khi logout: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Gửi refresh token lên API, cập nhật token mới vào Cookie và header.
        /// </summary>
        public async Task<ApiResponse<AuthResponse>> RefreshTokenAsync(string refreshToken)
        {
            if (string.IsNullOrWhiteSpace(refreshToken))
                return new ApiResponse<AuthResponse>
                {
                    IsSuccess = false,
                    Message = "Refresh token không hợp lệ."
                };

            try
            {
                var req = new RefreshTokenRequest { RefreshToken = refreshToken };
                var response = await _httpClient.PostAsJsonAsync("api/auth/refresh-token", req);

                if (response.IsSuccessStatusCode)
                {
                    var auth = await response.Content.ReadFromJsonAsync<AuthResponse>();
                    if (auth is not null)
                    {
                        // Cập nhật token mới trong cookie
                        await _tokenService.StoreTokenAsync(auth.Token);

                        _httpClient.DefaultRequestHeaders.Authorization =
                            new AuthenticationHeaderValue("Bearer", auth.Token);

                        return new ApiResponse<AuthResponse>
                        {
                            IsSuccess = true,
                            Data = auth
                        };
                    }
                }

                var err = await response.Content.ReadAsStringAsync();
                return new ApiResponse<AuthResponse>
                {
                    IsSuccess = false,
                    Message = $"Refresh token thất bại: {err}"
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<AuthResponse>
                {
                    IsSuccess = false,
                    Message = $"Lỗi khi refresh token: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Kiểm tra xem user đã có token hợp lệ trong Cookie chưa.
        /// </summary>
        public async Task<bool> IsUserAuthenticatedAsync()
        {
            var token = await _tokenService.GetTokenAsync();
            return !string.IsNullOrEmpty(token);
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

