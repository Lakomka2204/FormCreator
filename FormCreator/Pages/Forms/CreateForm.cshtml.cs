using ClassLibraryModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;
using System.Text;
using FormCreator.Pages.Shared;
using System.Net.Http.Headers;

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
            string endpoint = "api/v1/forms/create";
            FormModelV2 model = new FormModelV2()
            {
                Name = "New form",
                Description = "",
                CanBeSearched = false,
                FormElements = new List<GeneralFormElementModel>(1)
                        {
                            new GeneralFormElementModel()
                            {
                                Question = "example question",
                                Answer = "example answer",
                                QuestionType = QuestionType.ShortText,
                            }
                        },
            };
            string token = Request.Cookies["jwt"];
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var json = JsonSerializer.Serialize(model);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await client.PostAsync(endpoint, content);
            var resString = await response.Content.ReadAsStringAsync();
            var res = JsonSerializer.Deserialize<ServerResponse>(resString);
            if (!response.IsSuccessStatusCode)
            {
                TempData["UserError"] = res.error;
                return Redirect($"/forms/user/{GetCookieUser()?.Id}");
            }
            return Redirect($"/forms/edit/{res.stringResponse}");
        }
    }
}
