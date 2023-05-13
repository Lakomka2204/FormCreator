using ClassLibraryModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;
using System.Text;
using FormCreator.Pages.Shared;

namespace FormCreator.Pages.Forms
{
    [Authorize]
    public class CreateFormModel : UserAPIModel
    {
        private readonly IHttpClientFactory httpClientFactory;

        public CreateFormModel(IHttpClientFactory httpClientFactory, IJWT jwt)
            : base(jwt)
        {
            this.httpClientFactory = httpClientFactory;
        }
        public async Task<IActionResult> OnGetAsync()
        {
            using var client = httpClientFactory.CreateClient("FCApiClient");
            string endpoint = "api/forms/create";
            FormAlterModel model = new FormAlterModel()
            {
                Form = new FormModel()
                {
                    Name = "New form",
                    Description = "",
                    CanBeSearched = false,
                    FormElements = new List<BaseFormElementModel>(1)
                        {
                            new ShortTextFormElementModel()
                            {
                                Question = "What do you want to ask?",
                            }
                        },
                },
                Token = Request.Cookies["jwt"],
            };
            var json = JsonSerializer.Serialize(model);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await client.PostAsync(endpoint, content);
            var resString = await response.Content.ReadAsStringAsync();
            var res = JsonSerializer.Deserialize<ServerResponse>(resString);
            if (!response.IsSuccessStatusCode)
            {
                TempData["UserError"] = res.error;
                return Redirect($"/forms/{GetCookieUser()?.Id}");
            }
            return Redirect($"/edit/{res.stringResponse}");
        }
    }
}
