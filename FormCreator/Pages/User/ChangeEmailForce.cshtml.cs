using ClassLibraryModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace FormCreator.Pages.User
{
    [Authorize]
    public class ChangeEmailForceModel : PageModel
    {
        private readonly IHttpClientFactory httpClientFactory;

        public ChangeEmailForceModel(IHttpClientFactory httpClientFactory)
        {
            this.httpClientFactory = httpClientFactory;
        }
        public async Task<IActionResult> OnGetAsync(string? email)
        {
            try
            {
                if (email == null) return Redirect($"/user/{User.Identity.Name}");
                string token = Request.Cookies["jwt"];
                ChangeEmailClassModel body = new ChangeEmailClassModel()
                {
                    NewEmail = email,
                    Code = "",
                    EmailId = Guid.Empty,
                    Password = ""
                };
                var json = JsonSerializer.Serialize(body);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                string endpoint = $"api/v1/user/changeemailforce";
                using var client = httpClientFactory.CreateClient("FCApiClient");
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var response = await client.PostAsync(endpoint, content);
                var resString = await response.Content.ReadAsStringAsync();
                var res = JsonSerializer.Deserialize<ServerResponse>(resString);
                if (!response.IsSuccessStatusCode)
                {
                    TempData["UserError"] = res.error;
                    return Redirect($"/user/{User.Identity.Name}");
                }
                Response.Cookies.Append("jwt", res.token);
                return Redirect("/verifyemail");
            }
            catch (HttpRequestException)
            {
                TempData["UserError"] = "Service is unavailable. Please try again later";
                return Redirect($"/user/{User.Identity.Name}");
            }
            catch (Exception e)
            {
                TempData["UserError"] = e.Message;
                return Redirect($"/user/{User.Identity.Name}");
            }
        }
    }
}
