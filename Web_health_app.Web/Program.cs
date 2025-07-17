using Web_health_app.Web;
using Web_health_app.Web.ApiClients;
using Web_health_app.Web.Components;
using Web_health_app.Web.Services;
using Microsoft.AspNetCore.Authentication.Cookies;

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
// … thêm client khác …

// Add HttpContextAccessor (required for JwtTokenService)
builder.Services.AddHttpContextAccessor();

// Configure authentication with cookies
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "JWTCookie";
        options.Cookie.HttpOnly = true; // Prevents JavaScript access
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest; // Use 'Always' in production
        options.Cookie.SameSite = SameSiteMode.Strict;
        options.ExpireTimeSpan = TimeSpan.FromHours(1);
        options.SlidingExpiration = true;
    });

// local storage, DI service
//builder.Services.AddScoped<ILocalStorageService, LocalStorageService>();
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();

builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X-CSRF-TOKEN";
    options.Cookie.Name = "__Host-X-CSRF-TOKEN";
    options.Cookie.SameSite = SameSiteMode.Strict;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
});



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
//app.UseAntiforgery();

// Middleware
app.UseRouting();
app.UseAntiforgery();

app.UseOutputCache();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapDefaultEndpoints();

app.Run();
