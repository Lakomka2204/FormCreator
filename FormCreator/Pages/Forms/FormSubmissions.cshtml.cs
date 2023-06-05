using ClassLibraryModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Headers;
using System.Text.Json;

namespace FormCreator.Pages.Forms
{
    public class FormSubmissionsModel : PageModel
    {
        private readonly IHttpClientFactory httpClientFactory;

        public FormModel Form { get; set; }
        public List<Submission> Submissions { get; set; }
        public List<UserModel> AssociatedUsers { get; set; }
        public FormSubmissionsModel(IHttpClientFactory httpClientFactory)
        {
            this.httpClientFactory = httpClientFactory;
        }
        public enum Display
        {
            All,
            User,
            Question
        }
        public Display DisplayType { get; set; }
        public Guid? SelectedUser { get; set; }
        public async Task<IActionResult> OnGet(string? id, string? by,string? u)
        {
            string token = Request.Cookies["jwt"];
            using var client = httpClientFactory.CreateClient("FCApiClient");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            string subEndpoint = $"/api/v1/forms/submissions/form?formId={id}";
            using var response = await client.GetAsync(subEndpoint);
            var resString = await response.Content.ReadAsStringAsync();
            var res = JsonSerializer.Deserialize<ServerResponse>(resString);
            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError(string.Empty, res.error);
                return Page();
            }
            if (res.submissionsModelResponse == null)
            {
                ModelState.AddModelError(string.Empty, "Could not retrieve submissions.");
                return Page();
            }
            if (res?.submissionsModelResponse.Count == 0)
                Submissions = new List<Submission>(0);
            else
                Submissions = res.submissionsModelResponse;
            if (Enum.TryParse(by, out Display dType))
                DisplayType = dType;
            else
                DisplayType = Display.All;
            
            string formEndpoint = $"/api/v1/forms/form?id={id}";
            using var r2 = await client.GetAsync(formEndpoint);
            var r2S = await r2.Content.ReadAsStringAsync();
            var r2re = JsonSerializer.Deserialize<ServerResponse>(r2S);
            if (!r2.IsSuccessStatusCode)
            {
                ModelState.AddModelError(string.Empty, r2re.error);
                return Page();
            }
            if (r2re.formModelResponse == null)
            {
                ModelState.AddModelError(string.Empty, "Could not retrieve form.");
                return Page();
            }
            Form = r2re.formModelResponse;
            AssociatedUsers ??= new List<UserModel>();
            string getSubUserEndpoint = "/api/v1/user/";
            foreach (var sub in Submissions)
            {
                if (AssociatedUsers.Any(x => x.Id == sub.UserId))
                    continue;
                using var rUser = await client.GetAsync(getSubUserEndpoint + sub.UserId);
                var r = JsonSerializer.Deserialize<ServerResponse>(await rUser.Content.ReadAsStringAsync());
                if (rUser.IsSuccessStatusCode)
                    AssociatedUsers.Add(r.userModelResponse);
                else
                    AssociatedUsers.Add(new UserModel() { Username = "Anonymous", Id = sub.UserId });
            }
            if (DisplayType == Display.User)
                if (Guid.TryParse(u, out Guid userId))
                    SelectedUser = userId;
                else
                    SelectedUser = AssociatedUsers.FirstOrDefault()?.Id;
            return Page();
        }
    }
}
