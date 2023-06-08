using FormCreator.Models;
using FormCreator.Services;
using FormCreator;
using FormCreator.Pages.Shared;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MongoDB.Driver;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.Configure<DbConfig>(
    builder.Configuration.GetSection(nameof(DbConfig)));
builder.Services.AddSingleton<IDbConfig>(sp => sp.GetRequiredService<IOptions<DbConfig>>().Value);
builder.Configuration["JwtConfig:SecretKey"] = Environment.GetEnvironmentVariable("JWT_SECRET");
builder.Services.Configure<JwtConfig>(
    builder.Configuration.GetSection(nameof(JwtConfig)));
builder.Services.AddSingleton<IJWTConfig>(sp => sp.GetRequiredService<IOptions<JwtConfig>>().Value);
builder.Services.AddSingleton<IMongoClient>(s => new MongoClient(string.Format(builder.Configuration.GetValue<string>("DbConfig:ConnectionString"),Environment.GetEnvironmentVariable("DB_USR"),Environment.GetEnvironmentVariable("DB_PWD"))));
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IFormService, FormService>();
builder.Services.AddScoped<IPasswordService, PasswordService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<ISubmissionService, SubmissionService>();
builder.Services.AddScoped<IJWT, JWT>();
builder.Services.AddScoped<ILogger, Logger<string>>();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = false,
                ValidateIssuerSigningKey = true,
                ValidIssuer = builder.Configuration["JwtConfig:Issuer"],
                ValidAudience = builder.Configuration["JwtConfig:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JwtConfig:SecretKey"]))
            };
        });
builder.Services.AddAuthorization();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
});
builder.Services.AddVersionedApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});

builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                new string[] {}
            }
        });
});
builder.Services.ConfigureOptions<ConfigureSwaggerVersioning>();

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
builder.Services.AddHttpClient("FCApiClient")
.ConfigurePrimaryHttpMessageHandler(() =>
{
    return new HttpClientHandler
    {
        ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true
    };
}).ConfigureHttpClient(client =>
{
    client.BaseAddress = ub.Uri;

});
builder.Services.AddScoped<ILogger, Logger<string>>();
builder.Services.AddScoped<IJWT, JWT>();
builder.Services.AddScoped<UserHeaderModel>();
builder.Services.AddAuthentication("jwt")
    .AddCookie("jwt", options =>
    {
        options.Cookie.Name = "jwt";
        options.LoginPath = "/login";
        options.LogoutPath = "/logout";
        options.ReturnUrlParameter = "ret";

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
if (builder.Environment.IsProduction())
    builder.WebHost.UseStaticWebAssets();
var app = builder.Build();
    app.UseMiddleware<AuthMiddleware>();
var provider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();

if (!app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        foreach (var desc in provider.ApiVersionDescriptions)
        {
            options.SwaggerEndpoint($"/swagger/{desc.GroupName}/swagger.json", $"FCApi {desc.GroupName}");
        }
    });
    app.UseExceptionHandler("/Pub/Error");
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
