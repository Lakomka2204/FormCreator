using ClassLibraryModel;
using FCApi.Models;
using FormCreator.Pages.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;

namespace FormCreator.Pages.Forms
{
    [Authorize]
    public class MyFormsModel : PageModel
    {
        private readonly IHttpClientFactory httpClientFactory;

        public MyFormsModel(IHttpClientFactory httpClientFactory)
        {
            this.httpClientFactory = httpClientFactory;
        }
        public UserModel? FCUser { get; set; }
        public FormModel[] Forms { get; set; }
        public async Task<IActionResult> OnGetAsync()
        {
            string userGetEndpoint = "/api/user?token=" + Request.Cookies["jwt"];
            using var client = httpClientFactory.CreateClient("FCApiClient");
            var response = await client.GetAsync(userGetEndpoint);
            var responseString = await response.Content.ReadAsStringAsync();
            var res = JsonSerializer.Deserialize<ServerResponse>(responseString);
            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError(string.Empty, res.error);
                return Page();
            }
            FCUser = res.userModelResponse;
            // getting forms
            Forms = new FormModel[4];
            Forms[0] = new FormModel()
            {
                CanBeSearched = true,
                Id = Guid.Empty,
                Name = "mockup namsadsde",
                OwnerId = FCUser.Id,
            };
            Forms[1] = new FormModel()
            {
                CanBeSearched = true,
                Id = Guid.Empty,
                Name = "mockupasdasdasd name",
                OwnerId = FCUser.Id,
            };
            Forms[2] = new FormModel()
            {
                CanBeSearched = false,
                Id = Guid.Empty,
                Name = "mockqwdqwdup name",
                OwnerId = FCUser.Id,
            };
            Forms[3] = new FormModel()
            {
                CanBeSearched = true,
                Id = Guid.Empty,
                Name = "mockup 23423423424243243",
                OwnerId = FCUser.Id,
            };
            return Page();
        }
    }
}
