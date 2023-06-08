using FormCreator.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;
using System.Web;

namespace FormCreator.Pages.User
{
    [Authorize]
    public class ChangeVisibilityModel : PageModel
    {
        private readonly IHttpClientFactory httpClientFactory;

        public ChangeVisibilityModel(IHttpClientFactory httpClientFactory)
        {
            this.httpClientFactory = httpClientFactory;
        }
        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                bool emailVerified = bool.Parse(User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Gender)?.Value ?? "false");
                if (!emailVerified)
                {
                    TempData["UserError"] = "You need to verify email in order to change visibility.";
                    return Redirect($"/user/{HttpContext.User.Identity.Name}");
                }
                string token = HttpContext.Request.Cookies["jwt"];
                var client = httpClientFactory.CreateClient("FCApiClient");
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                string query = $"api/v1/user/changevisibility";
                var response = await client.GetAsync(query);
                var responseContent = await response.Content.ReadAsStringAsync();

                var res = JsonSerializer.Deserialize<ServerResponse>(responseContent);
                if (!response.IsSuccessStatusCode)
                {
                    TempData["ChangeVisibilityError"] = res.error;
                }
                TempData["ChangeVisibilityStatus"] = res.boolResponse ?? false ? "Visible" : "Hidden";
                return Redirect($"/user/{HttpContext.User.Identity.Name}");
            }
            catch (HttpRequestException)
            {
                TempData["UserError"] = "Service is unavailable. Please try again later";
                return Redirect($"/user/{HttpContext.User.Identity.Name}");
            }
            catch (Exception e)
            {
                TempData["UserError"] = e.ToString();
                return Redirect($"/user/{HttpContext.User.Identity.Name}");
            }
        }
    }
}
