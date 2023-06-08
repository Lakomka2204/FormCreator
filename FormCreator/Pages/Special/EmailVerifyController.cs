using FormCreator.Models;
using FormCreator.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.JsonWebTokens;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace FormCreator.Pages
{
    [Route("evc")]
    public class EmailVerifyController : ControllerBase
    {
        private readonly IHttpClientFactory httpClientFactory;
        private readonly IJWT jWT;

        public EmailVerifyController(IHttpClientFactory httpClientFactory, IJWT jWT)
        {
            this.httpClientFactory = httpClientFactory;
            this.jWT = jWT;
        }
        [HttpPost("o")]
        public async Task<IActionResult> SendOld(string data)
        {
            try
            {
                byte[] encdata = Convert.FromBase64String(data);
                string inputPassword = Encoding.UTF8.GetString(encdata);
                string token = HttpContext.Request.Cookies["jwt"];
                var claims = jWT.DecryptToken(token);
                var encodedPassword = claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.UniqueName)?.Value;
                using var client = httpClientFactory.CreateClient("FCApiClient");
                QueryBuilder qb = new QueryBuilder
            {
                { "e", encodedPassword },
                { "d", inputPassword }
            };

                string verifyPassEndpoint = $"api/v1/auth/pwdcompare{qb.ToQueryString().Value}";
                var res = await client.PostAsync(verifyPassEndpoint, null);
                if (res.IsSuccessStatusCode)
                {
                    string sendEmailEndpoint = $"api/v1/user/changeemail0";
                    ChangeEmailClassModel body = new ChangeEmailClassModel()
                    {
                        Code = "bazinga",
                        Password = inputPassword,
                        NewEmail = "example@mail.com",
                    };
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                    var json = JsonSerializer.Serialize(body);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");
                    var r2 = await client.PutAsync(sendEmailEndpoint, content);
                    var r2String = await r2.Content.ReadAsStringAsync();
                    var r2J = JsonSerializer.Deserialize<ServerResponse>(r2String);
                    if (!r2.IsSuccessStatusCode)
                    {
                        return StatusCode((int)r2.StatusCode, r2J.error);
                    }
                    return Ok(r2J.stringResponse);
                }
                else
                    return Unauthorized("Wrong password.");
            }
            catch (Exception e)
            {
                return StatusCode(500, e.Message);
            }
        }
    }
}
