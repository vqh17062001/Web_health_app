using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components.Authorization;
using Web_health_app.Web;
using Web_health_app.Web.ApiClients;
using Web_health_app.Web.Authentication;
using Web_health_app.Web.Components;


var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();
builder.AddRedisOutputCache("cache");

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
var apiBase = builder.Configuration["ApiSettings:BaseUrl"];

// typed clients
builder.Services.AddHttpClient<WeatherApiClient>(c => c.BaseAddress = new Uri(apiBase));
builder.Services.AddHttpClient<LoginApiClient>(c => c.BaseAddress = new Uri(apiBase));
builder.Services.AddHttpClient<UserApiClient>(c => c.BaseAddress = new Uri(apiBase));
builder.Services.AddHttpClient<RoleApiClient>(c => c.BaseAddress = new Uri(apiBase));
// … thêm client khác …

// Add HttpContextAccessor (required for JwtTokenService)
builder.Services.AddHttpContextAccessor();
builder.Services.AddAuthentication();
builder.Services.AddCascadingAuthenticationState();

// Configure authentication with cookies



// local storage, DI service
builder.Services.AddScoped<AuthenticationStateProvider, CustonAuthStateProvider>();




var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

// Add authentication middleware before output cache
app.UseAuthentication();
app.UseAuthorization();




app.UseOutputCache();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapDefaultEndpoints();

app.Run();
