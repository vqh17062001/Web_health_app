using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Web_health_app.Web.Authentication
{
    public class CustonAuthStateProvider(ProtectedLocalStorage localStorage) : AuthenticationStateProvider

    {
        


        public async override Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var token = (await localStorage.GetAsync<string>("authToken")).Value;
            var identity = string.IsNullOrEmpty(token) ? 
                new ClaimsIdentity() : 
                GetClaimIdentity(token);
            var user = new ClaimsPrincipal(identity);
            return new AuthenticationState(user);
        }

        public async Task MarkUserAsAuthenticated(string token)
        {
            await localStorage.SetAsync("authToken", token);
            var identity = GetClaimIdentity(token);
            var user = new ClaimsPrincipal(identity);

            NotifyAuthenticationStateChanged(
                Task.FromResult(new AuthenticationState(user))
            );

        }

        private ClaimsIdentity GetClaimIdentity(string token)
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);
            var claims = jwtToken.Claims.ToList();
            var identity = new ClaimsIdentity(claims, "jwt");
            return identity;
        }

        public async Task MarkUserAsLoggedOut()
        {
            await localStorage.DeleteAsync("authToken");
            var anonymous = new ClaimsIdentity();
            var user = new ClaimsPrincipal(anonymous);
            NotifyAuthenticationStateChanged(
                Task.FromResult(new AuthenticationState(user))
            );
        }
    }
}
