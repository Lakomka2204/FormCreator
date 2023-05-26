using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;
using System.Text;
using ClassLibraryModel;
using Microsoft.AspNetCore.Http.Extensions;
using System.Net.Http.Headers;

namespace FormCreator.Pages.User
{
    public class RestoreAccountModel : PageModel
    {
        private readonly IHttpClientFactory httpClientFactory;

        public RestoreAccountModel(IHttpClientFactory httpClientFactory)
        {
            this.httpClientFactory = httpClientFactory;
        }
        public async Task<IActionResult> OnPostAsync(string? token)
        {
            try
            {

                if (token == null) return Redirect("/login");
                using var client = httpClientFactory.CreateClient("FCApiClient");

                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                string endpoint = $"api/v1/auth/restoreaccount";
                var response = await client.PostAsync(endpoint, null);
                var resString = await response.Content.ReadAsStringAsync();
                var res = JsonSerializer.Deserialize<ServerResponse>(resString);
                if (!response.IsSuccessStatusCode)
                {
                    ModelState.AddModelError(string.Empty, res.error);
                    return Page();
                }
                string newToken = response.Headers.FirstOrDefault(x => x.Key == "Authorization").Value.FirstOrDefault();
                HttpContext.Response.Cookies.Append("jwt", newToken);
                if (res.userModelResponse.EmailVerified)
                    return Redirect($"/user/{res.userModelResponse.Id}");
                else
                    return Redirect("/verifyemail");
            }
            catch (Exception e)
            {
                TempData["UserError"] = e.Message;
                return Redirect("/login");
            }
        }

    }
}
