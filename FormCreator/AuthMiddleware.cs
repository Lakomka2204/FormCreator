using ClassLibraryModel;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;

namespace FormCreator
{
    public class AuthMiddleware : IMiddleware
    {
        private readonly IJWT jwtService;
        private readonly IHttpClientFactory httpClientFactory;
        private readonly ILogger logger;

        public AuthMiddleware(IJWT jwtService, IHttpClientFactory httpClientFactory, ILogger logger)
        {
            this.jwtService = jwtService;
            this.httpClientFactory = httpClientFactory;
            this.logger = logger;
        }
        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            try
            {
                string token = context.Request.Cookies["jwt"];
                using var httpClient = httpClientFactory.CreateClient("FCApiClient");
                //httpClient.Timeout = TimeSpan.FromMilliseconds(500);
                string endpoint = "/api/v1";
                if (!string.IsNullOrEmpty(token))
                {
                    IEnumerable<Claim> claims = jwtService.DecryptToken(token);

                    if (claims != null)
                    {
                        endpoint += $"/auth/verifytoken";
                        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                        var response = await httpClient.GetAsync(endpoint);
                        if (response.IsSuccessStatusCode)
                        {
                            string newToken = response.Headers.GetValues("Authorization").FirstOrDefault();
                            IEnumerable<Claim> newClaims = jwtService.DecryptToken(newToken);
                            bool emailVerified = bool.Parse(newClaims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Gender)?.Value ?? "False");
                            string? id = newClaims.FirstOrDefault(x => x.Type == "Id")?.Value;
                            context.Response.Cookies.Append("jwt", newToken);
                            context.User = new ClaimsPrincipal(new ClaimsIdentity(new[]
                            {
                        new Claim(ClaimTypes.Name, id),
                        new Claim(ClaimTypes.Role, "user"),
                        new Claim(ClaimTypes.Gender, emailVerified.ToString())
                        }, "jwt"));
                        }
                        else
                        {
                            string resString = await response.Content.ReadAsStringAsync();
                            var res = JsonSerializer.Deserialize<ServerResponse>(resString);
                            context.Items["UserError"] = res.error;
                            context.Response.Cookies.Delete("jwt");
                            context.User = null;
                        }
                    }
                }
                await httpClient.GetAsync(endpoint);
            }
            catch (HttpRequestException e)
            {
                logger.LogError(e, "Http Exception");
                context.Items["UserError"] = "Service is unavailable. Please try again later";
            }
            catch (TaskCanceledException e)
            {
                logger.LogError(e,"Task cancelled");
                context.Items["UserError"] = "Service is unavailable. Please try again later";
            }
            finally
            {
                await next(context);
            }

        }
    }
}
