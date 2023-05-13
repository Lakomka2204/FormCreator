using ClassLibraryModel;
using FormCreator.Pages.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;

namespace FormCreator.Pages.Forms
{
    public class MyFormsModel : PageModel
    {
        private readonly IHttpClientFactory httpClientFactory;

        public MyFormsModel(IHttpClientFactory httpClientFactory)
        {
            this.httpClientFactory = httpClientFactory;
        }
        public UserModel? FCUser { get; set; }
        public List<FormModel> Forms { get; set; }
        public bool SelfAccount { get; private set; }
        public async Task<IActionResult> OnGetAsync(string? id)
        {
            try
            {
                string token = HttpContext.Request.Cookies["jwt"];
                string userEndpoint;
                if (HttpContext.User.Identity.IsAuthenticated && HttpContext.User.Identity.Name == id)
                {
                    userEndpoint = $"api/user/?token={token}";
                    SelfAccount = true;
                }
                else
                    userEndpoint = $"api/user/{id}";


                using var client = httpClientFactory.CreateClient("FCApiClient");
                var response = await client.GetAsync(userEndpoint);
                var responseString = await response.Content.ReadAsStringAsync();
                var res = JsonSerializer.Deserialize<ServerResponse>(responseString);

                if (!response.IsSuccessStatusCode)
                {

                    if (res.error.Contains("permission"))
                    {
                        FCUser = new UserModel()
                        {
                            Username = "Anonymous user",
                            AnonymousView = false,
                        };
                    }
                    else
                        ModelState.AddModelError(string.Empty, res.error);
                }
                else
                    FCUser = res.userModelResponse;
                // getting forms
                string formEndpoint;
                if (SelfAccount)
                {
                    formEndpoint = $"api/forms/my?token={token}";
                }
                else
                    formEndpoint = $"api/forms?uid={id}";
                var r2 = await client.GetAsync(formEndpoint);
                string r2String = await r2.Content.ReadAsStringAsync();
                var r2s = JsonSerializer.Deserialize<ServerResponse>(r2String);
                if (!r2.IsSuccessStatusCode)
                {
                    ModelState.AddModelError(string.Empty, res.error);
                    return Page();
                }
                if (r2s.formsModelResponse == null)
                    Forms = new List<FormModel>(0);
                else
                    Forms = r2s.formsModelResponse;
                return Page();
            }
            catch (Exception e)
            {
                TempData["UserError"] = e.Message;
                return Page();
            }
        }
    }
}
