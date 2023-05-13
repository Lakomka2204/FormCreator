using ClassLibraryModel;
using FormCreator.Pages.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;

namespace FormCreator.Pages.Forms
{
    [Authorize]
    public class DeleteFormModel : UserAPIModel
    {
        private readonly IHttpClientFactory httpClientFactory;

        public DeleteFormModel(IHttpClientFactory httpClientFactory,IJWT jwt)
            :base(jwt)
        {
            this.httpClientFactory = httpClientFactory;
        }
        public async Task<IActionResult> OnGet(string? id)
        {
            QueryBuilder qb = new QueryBuilder
            {
                { "formId", id },
                { "token", Request.Cookies["jwt"] }
            };
            string deleteEndpoint = $"api/forms/delete{qb.ToQueryString()}";
            using var client = httpClientFactory.CreateClient("FCApiClient");
            var res = await client.DeleteAsync(deleteEndpoint);
            var resString = await res.Content.ReadAsStringAsync();
            var resp = JsonSerializer.Deserialize<ServerResponse>(resString);
            if (!res.IsSuccessStatusCode)
            {
                TempData["DeletedFormError"] = resp.error;
            }
            TempData["DeletedFormName"] = resp.stringResponse;
            return Redirect($"/forms/{GetCookieUser()?.Id}");
        }
    }
}
