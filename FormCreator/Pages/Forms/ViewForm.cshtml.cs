using ClassLibraryModel;
using FormCreator.Pages.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace FormCreator.Pages.Forms
{
    [Authorize]
    public class ViewFormModel : UserAPIModel
    {
        private readonly IHttpClientFactory httpClientFactory;

        public ViewFormModel(IHttpClientFactory httpClientFactory, IJWT jwt)
            : base(jwt)
        {
            this.httpClientFactory = httpClientFactory;
        }
        public FormModel? Form { get; set; }
        [BindProperty]
        public Submission Submission { get; set; }
        public bool SelfForm { get; set; }
        public bool IsSubmitted { get; set; }
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }
            using var client = httpClientFactory.CreateClient("FCApiClient");
            string token = Request.Cookies["jwt"];
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            string endpoint = "api/v1/forms/submissions/submit";
            for (int i = 0; i < Submission.Submissions.Count; i++)
            {
                if (Submission.Submissions[i].Answer != null) continue;
                var actualValue = Request.Form.FirstOrDefault(x => x.Key.Contains($"[{i}].Answer"));
                switch (Submission.Submissions[i].QuestionType)
                {
                    case QuestionType.None:
                        throw new Exception("crazy fucker how did you do questiontype 0, go back monke to your cave");
                    case QuestionType.ShortText:
                    case QuestionType.LongText:
                        Submission.Submissions[i].Answer = actualValue.Value[0];
                        break;
                    case QuestionType.Time:
                        Submission.Submissions[i].Answer = TimeSpan.Parse(actualValue.Value[0] ?? "00:00:00").ToString("hh\\:mm\\:ss");
                        break;
                    case QuestionType.Date:
                        Submission.Submissions[i].Answer = DateTime.Parse(actualValue.Value[0] ?? DateTime.MinValue.ToString()).ToString("yyyy-MM-dd");
                        break;
                    case QuestionType.SingleOption:
                        Submission.Submissions[i].Answer = int.Parse(actualValue.Value[0] ?? "0");
                        break;
                    case QuestionType.MultipleOptions:
                        Submission.Submissions[i].Answer = actualValue.Value.Select(int.Parse).ToList();
                        break;
                }
            }
            var json = JsonSerializer.Serialize(Submission);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await client.PostAsync(endpoint, content);
            string resString = await response.Content.ReadAsStringAsync();
            var res = JsonSerializer.Deserialize<ServerResponse>(resString);
            if (!response.IsSuccessStatusCode)
            {
                TempData["UserError"] = res.error;
                return Page();
            }
            if (res.submissionModelResponse == null)
            {
                TempData["UserError"] = "Form was not submitted";
            }
            IsSubmitted = true; // todo submission window
            Submission = null;
            Form = null;
            return Page();
        }
        public async Task<IActionResult> OnGetAsync(string? id)
        {
            string token = HttpContext.Request.Cookies["jwt"];
            using var client = httpClientFactory.CreateClient("FCApiClient");
            string endpoint = $"api/v1/forms/form?id={id}";
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var response = await client.GetAsync(endpoint);
            string resString = await response.Content.ReadAsStringAsync();
            var res = JsonSerializer.Deserialize<ServerResponse>(resString);
            if (!response.IsSuccessStatusCode)
            {
                TempData["UserError"] = res.error;
                return Page();
            }
            Form = res?.formModelResponse;
            SelfForm = Form?.OwnerId == GetCookieUser()?.Id;
            return Page();

        }
    }
}
