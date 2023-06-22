using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http;
using System.Text.Json;
using System.Text;
using FormCreator.Models;
using System.ComponentModel.DataAnnotations;
using System.Net.Http.Headers;

namespace FormCreator.Pages.User
{
    public class PageUserModel : PageModel
    {
        private readonly IHttpClientFactory httpClientFactory;

        public PageUserModel(IHttpClientFactory httpClientFactory)
        {
            this.httpClientFactory = httpClientFactory;
        }
        [BindProperty]
        public Guid Id { get; set; }
        [BindProperty]
        public DeleteAccountClassModel DeleteAccountData { get; set; }
        public async Task<IActionResult> OnGetAsync(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                {
                    return NotFound();
                }
                var client = httpClientFactory.CreateClient("FCApiClient");
                string registrationEndpoint;
                if (HttpContext.User.Identity.IsAuthenticated && HttpContext.User.Identity.Name == id)
                {
                    string token = HttpContext.Request.Cookies["jwt"];
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                    registrationEndpoint = $"api/v1/user/self";
                    SelfAccount = true;
                }
                else
                    registrationEndpoint = $"api/v1/user/{id}";

                var response = await client.GetAsync(registrationEndpoint);
                var responseContent = await response.Content.ReadAsStringAsync();
                var res = JsonSerializer.Deserialize<ServerResponse>(responseContent);
                if (response.IsSuccessStatusCode)
                    FCUser = res.userModelResponse;
                else
                    ModelState.AddModelError(string.Empty, res.error);
                return Page();
            }
            catch (HttpRequestException)
            {
                TempData["UserError"] = "Service is unavailable. Please try again later";
                return Page();
            }
            catch (Exception e)
            {
                TempData["UserError"] = e.Message;
                return Page();
            }
        }
        [BindProperty]
        public bool SelfAccount { get; set; }
        [BindProperty]
        public UserModel FCUser { get; set; }
    }

}
