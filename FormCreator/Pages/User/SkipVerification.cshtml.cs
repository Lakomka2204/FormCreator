using FormCreator.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;
using System.Text;

namespace FormCreator.Pages.User
{
    public class SkipVerificationModel : PageModel
    {
        private readonly IHttpClientFactory httpClientFactory;

        public SkipVerificationModel(IHttpClientFactory httpClientFactory)
        {
            this.httpClientFactory = httpClientFactory;
        }
        public async Task<IActionResult> OnPostAsync(string id)
        {
            try
            {
                var client = httpClientFactory.CreateClient("FCApiClient");
                var body = new
                {
                    userId = id,
                    code = "000000"
                };
                var json = JsonSerializer.Serialize(body);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await client.PutAsync("api/v1/auth/skipverification", content);
                var responseContent = await response.Content.ReadAsStringAsync();
                string? token;
                if (!response.IsSuccessStatusCode)
                {
                    var res = JsonSerializer.Deserialize<ServerResponse>(responseContent);
                    if (res?.error == "Already verified.")
                    {
                        token = response.Headers.GetValues("Authorization").FirstOrDefault();
                        Response.Cookies.Delete("jwt");
                        Response.Cookies.Append("jwt", token);
                        return Redirect($"/user/{id}");
                    }
                    else
                    {
                        TempData["EmailVerificationError"] = res.error;
                        return Page();
                    }
                }
                token = response.Headers.GetValues("Authorization").FirstOrDefault();
                Response.Cookies.Delete("jwt");
                Response.Cookies.Append("jwt", token);
                return Redirect($"/user/{id}");
            }
            catch (Exception e)
            {
                TempData["UserError"] = e.Message;
                return Page();
            }
        }
    }
}
