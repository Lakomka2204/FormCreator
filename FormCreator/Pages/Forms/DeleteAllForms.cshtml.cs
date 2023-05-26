using ClassLibraryModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Headers;
using System.Text.Json;

namespace FormCreator.Pages.Forms
{
    public class DeleteAllFormsModel : PageModel
    {
        private readonly IHttpClientFactory httpClientFactory;

        public DeleteAllFormsModel(IHttpClientFactory httpClientFactory)
        {
            this.httpClientFactory = httpClientFactory;
        }
        public async Task<IActionResult> OnGetAsync(string id)
        {
            try
            {
                string endpoint = "api/v1/forms/deleteall";
                using var client = httpClientFactory.CreateClient("FCApiClient");
                string token = Request.Cookies["jwt"];
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var response = await client.DeleteAsync(endpoint);
                string resString = await response.Content.ReadAsStringAsync();
                if (!response.IsSuccessStatusCode)
                {
                    var res = JsonSerializer.Deserialize<ServerResponse>(resString);
                    TempData["DeletedFormsError"] = res.error;
                }
                return Redirect($"/forms/user/{id}");
            }
            catch (Exception e)
            {
                TempData["DeletedFormsError"] = e.Message;
                return Redirect($"/forms/user/{id}");
            }
        }
    }
}
