using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;

namespace Web_health_app.Web.Authentication
{
    public class CustonAuthStateProvider(ProtectedLocalStorage localStorage) : AuthenticationStateProvider

    {

        private ClaimsPrincipal _anonymous = new ClaimsPrincipal(new ClaimsIdentity());

        public async override Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var token = (await localStorage.GetAsync<string>("authToken")).Value;

            if (string.IsNullOrWhiteSpace(token))
                return new AuthenticationState(_anonymous);

            var identity = new ClaimsIdentity(ParseClaimsFromJwt(token), "jwt");

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


        private IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
        {
            var claims = new List<Claim>();
            var payload = jwt.Split('.')[1];
            var jsonBytes = ParseBase64WithoutPadding(payload);
            var keyValuePairs = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonBytes);
            string role = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role";
            if (keyValuePairs.ContainsKey(role))
            {
                if (keyValuePairs[role] is JsonElement roleElement && roleElement.ValueKind == JsonValueKind.Array)
                {
                    foreach (var r in roleElement.EnumerateArray())
                        claims.Add(new Claim(ClaimTypes.Role, r.GetString()));
                }
                else
                {
                    claims.Add(new Claim(ClaimTypes.Role, keyValuePairs[role].ToString()));
                }
            }

            foreach (var kvp in keyValuePairs)
            {
                claims.Add(new Claim(kvp.Key, kvp.Value.ToString()));
            }

            return claims;
        }

        private byte[] ParseBase64WithoutPadding(string base64)
        {
            switch (base64.Length % 4)
            {
                case 2: base64 += "=="; break;
                case 3: base64 += "="; break;
            }
            return Convert.FromBase64String(base64);
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
