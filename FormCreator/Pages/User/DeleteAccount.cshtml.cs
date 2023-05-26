using ClassLibraryModel;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Server.HttpSys;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace FormCreator.Pages.User
{
    [Authorize]
    public class DeleteAccountModel : PageModel
    {
        private readonly IHttpClientFactory httpClientFactory;

        public DeleteAccountModel(IHttpClientFactory httpClientFactory)
        {
            this.httpClientFactory = httpClientFactory;
        }
        public async Task<IActionResult> OnGetAsync(string? password)
        {
            try
            {
                string token = HttpContext.Request.Cookies["jwt"];
                if (password == null)
                {

                    TempData["DeleteAccountError"] = "No password";
                    return Redirect($"/user/{HttpContext.User.Identity.Name}");
                }

                string query = $"api/v1/user/deleteaccount";
                var client = httpClientFactory.CreateClient("FCApiClient");

                DeleteAccountClassModel body = new DeleteAccountClassModel()
                {
                    Password = password,
                };
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var json = JsonSerializer.Serialize(body);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await client.PutAsync(query, content);
                var responseContent = await response.Content.ReadAsStringAsync();

                var res = JsonSerializer.Deserialize<ServerResponse>(responseContent);
                if (!response.IsSuccessStatusCode)
                {
                    TempData["DeleteAccountError"] = res.error;
                    return Redirect($"/user/{HttpContext.User.Identity.Name}");
                }

                HttpContext.Response.Cookies.Delete("jwt");
                TempData["AccountDeletionDate"] = res.stringResponse;
                return Redirect($"/index");
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
