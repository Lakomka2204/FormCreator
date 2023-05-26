using ClassLibraryModel;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;

namespace FormCreator
{
    public class AuthMiddleware : IMiddleware
    {
        private readonly IJWT jwtService;
        private readonly IHttpClientFactory httpClientFactory;

        public AuthMiddleware(IJWT jwtService, IHttpClientFactory httpClientFactory)
        {
            this.jwtService = jwtService;
            this.httpClientFactory = httpClientFactory;
        }
        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            try
            {

                string token = context.Request.Cookies["jwt"];
                using var httpClient = httpClientFactory.CreateClient("FCApiClient");
                httpClient.Timeout = TimeSpan.FromMilliseconds(500);
                string endpoint = "/";
                if (!string.IsNullOrEmpty(token))
                {
                    IEnumerable<Claim> claims = jwtService.DecryptToken(token);

                    if (claims != null)
                    {
                        endpoint += $"api/v1/auth/verifytoken";
                        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer",token);
                        var response = await httpClient.GetAsync(endpoint);
                        if (response.IsSuccessStatusCode)
                        {
                            string? id = claims.FirstOrDefault(x => x.Type == "Id")?.Value;
                            bool emailVerified = bool.Parse(claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Gender)?.Value ?? "False");

                            context.User = new ClaimsPrincipal(new ClaimsIdentity(new[]
                            {
                        new Claim(ClaimTypes.Name, id),
                        new Claim(ClaimTypes.Role, "user"),
                        new Claim(ClaimTypes.Gender, emailVerified.ToString())
                        }, "jwt"));
                        }
                        else
                        {
                            context.Response.Cookies.Delete("jwt");
                            context.User = null;
                        }
                    }
                }
                await httpClient.GetAsync(endpoint);
            }
            catch (HttpRequestException)
            {
                context.Items["UserError"] = "Service is unavailable. Please try again later";
            }
            catch (TaskCanceledException)
            {
                context.Items["UserError"] = "Service is unavailable. Please try again later";
            }
            finally
            {
                await next(context);
            }

        }
    }
}
