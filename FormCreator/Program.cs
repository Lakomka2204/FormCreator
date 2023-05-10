using ClassLibraryModel;
using FormCreator;
using FormCreator.Pages.Shared;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Options;
using System.IO;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddHttpClient();
builder.Services.AddHttpContextAccessor();
builder.Services.Configure<JwtConfig>(
    builder.Configuration.GetSection(nameof(JwtConfig)));
builder.Services.AddSingleton<IJWTConfig>(sp => sp.GetRequiredService<IOptions<JwtConfig>>().Value);

builder.Services.Configure<BackendConfig>(
    builder.Configuration.GetSection(nameof(BackendConfig)));
builder.Services.AddSingleton<IBackendConfig>(sp => sp.GetRequiredService<IOptions<BackendConfig>>().Value);
builder.Services.Configure<CookiePolicyOptions>(options =>
{
    options.CheckConsentNeeded = c => true;
    options.MinimumSameSitePolicy = SameSiteMode.None;
});

UriBuilder ub = new()
{
    Scheme = builder.Configuration.GetValue<string>("BackendConfig:Scheme"),
    Host = builder.Configuration.GetValue<string>("BackendConfig:Hostname"),
    Port = builder.Configuration.GetValue<int>("BackendConfig:Port"),
};
builder.Services.AddHttpClient("FCApiClient", client =>
{
    client.BaseAddress = ub.Uri;
});
builder.Services.AddScoped<IJWT, JWT>();
builder.Services.AddScoped<UserHeaderModel>();
builder.Services.AddAuthentication("jwt")
    .AddCookie("jwt", options =>
    {
        options.Cookie.Name = "jwt";
        options.LoginPath = "/login";
        options.LogoutPath = "/logout";

        options.Events = new CookieAuthenticationEvents()
        {
            OnRedirectToLogin = ctx =>
            {

                ctx.Properties.Items["UserError"] = "Please login to access this page.";
                ctx.Response.Redirect(ctx.RedirectUri);
                return Task.CompletedTask;
            },
        };
    });
builder.Services.AddAuthorization();
builder.Services.AddScoped<AuthMiddleware>();
var app = builder.Build();
app.UseMiddleware<AuthMiddleware>();
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.MapControllers();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapFallbackToPage("/", "/index");
app.MapRazorPages();

app.Run();
