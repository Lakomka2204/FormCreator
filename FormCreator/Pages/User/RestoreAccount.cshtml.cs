using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;
using System.Text;
using ClassLibraryModel;
using Microsoft.AspNetCore.Http.Extensions;

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

                QueryBuilder qb = new QueryBuilder
            {
                { "token", token },
            };

                string endpoint = $"api/auth/restoreaccount{qb.ToQueryString()}";
                var response = await client.PostAsync(endpoint, null);
                var resString = await response.Content.ReadAsStringAsync();
                var res = JsonSerializer.Deserialize<ServerResponse>(resString);
                if (!response.IsSuccessStatusCode)
                {
                    ModelState.AddModelError(string.Empty, res.error);
                    return Page();
                }
                HttpContext.Response.Cookies.Append("jwt", res.token);
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
