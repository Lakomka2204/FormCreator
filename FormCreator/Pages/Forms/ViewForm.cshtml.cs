using ClassLibraryModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Headers;

namespace FormCreator.Pages.Forms
{
    public class ViewFormModel : PageModel
    {
        private readonly IHttpClientFactory httpClientFactory;

        public ViewFormModel(IHttpClientFactory httpClientFactory)
        {
            this.httpClientFactory = httpClientFactory;
        }
        public FormModelV2 Form { get; set; }
        public async Task<IActionResult> OnGetAsync(string? id)
        {
            string token = HttpContext.Request.Cookies["jwt"];
            using var client = httpClientFactory.CreateClient("FCApiClient");
            string endpoint = "api/v1/forms/whatever";
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var res = await client.GetAsync(endpoint);
            string resString = await res.Content.ReadAsStringAsync();
            Form = new FormModelV2
            {
                Name = $"[{res.StatusCode}] {resString}"
            };
            return Page();
       
        }
    }
}
