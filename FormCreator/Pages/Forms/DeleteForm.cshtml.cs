using FormCreator.Models;
using FormCreator.Pages.Shared;
using FormCreator.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Headers;
using System.Text.Json;

namespace FormCreator.Pages.Forms
{
    [Authorize]
    public class DeleteFormModel : UserAPIModel
    {
        private readonly IHttpClientFactory httpClientFactory;

        public DeleteFormModel(IHttpClientFactory httpClientFactory, IJWT jwt)
            : base(jwt)
        {
            this.httpClientFactory = httpClientFactory;
        }
        public async Task<IActionResult> OnGet(string? id)
        {
            string deleteEndpoint = $"api/v1/forms/delete?id={id}";
            using var client = httpClientFactory.CreateClient("FCApiClient");
            string token = Request.Cookies["jwt"];
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var res = await client.DeleteAsync(deleteEndpoint);
            var resString = await res.Content.ReadAsStringAsync();
            var resp = JsonSerializer.Deserialize<ServerResponse>(resString);
            if (!res.IsSuccessStatusCode)
            {
                TempData["DeletedFormError"] = resp.error;
            }
            TempData["DeletedFormName"] = resp.stringResponse;
            return Redirect($"/forms/user/{GetCookieUser()?.Id}");
        }
    }
}
