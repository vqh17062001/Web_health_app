namespace Web_health_app.Web.Services
{
    public interface IJwtTokenService
    {
        Task StoreTokenAsync(string token);
        Task<string?> GetTokenAsync();
        Task RemoveTokenAsync();
    }
}
