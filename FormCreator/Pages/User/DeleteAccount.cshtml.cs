using FormCreator.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Server.HttpSys;
using System.ComponentModel.DataAnnotations;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Xml.Linq;

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
        [BindProperty]
        public DeleteAccountClassModel DeleteAccountData { get; set; }
        public async Task<IActionResult> OnGetAsync(string? code, string? password, string? emailId)
        {
            try
            {
                string token = HttpContext.Request.Cookies["jwt"];
                if (password == null)
                {
                    TempData["DeleteAccountError"] = "No password";
                    return RedirectToPage("/User/User", new { id = HttpContext.User.Identity.Name });
                }
                if (!Guid.TryParse(emailId,out Guid eId))
                {
                    TempData["DeleteAccountError"] = "You need to send email.";
                    return RedirectToPage("/User/User", new { id = HttpContext.User.Identity.Name });
                }
                if (code == null)
                {
                    TempData["DeleteAccountError"] = "No code";
                    return RedirectToPage("/User/User", new { id = HttpContext.User.Identity.Name });
                }
                string query = $"api/v1/user/deleteaccount";
                var client = httpClientFactory.CreateClient("FCApiClient");

                DeleteAccountClassModel body = new DeleteAccountClassModel()
                {
                    Password = password,
                    Code = code,
                    EmailId = eId
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
