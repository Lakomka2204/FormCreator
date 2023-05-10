using ClassLibraryModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MongoDB.Bson.IO;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace FormCreator.Pages.User
{
    [Authorize]
    public class ChangeEmailModel : PageModel
    {
        [BindProperty]
        [Required(ErrorMessage = "This field is required.")]
        [Display(Prompt = "Your current password")]
        public string Password { get; set; }
        [BindProperty]
        [Required(ErrorMessage = "This field is required.")]

        [Display(Prompt = "Email verification code")]
        [Range(100000, 999999, ErrorMessage = "Verification Code must be a 6-digit number")]
        public string OldCode { get; set; }
        [BindProperty]
        [Required(ErrorMessage = "This field is required.")]
        [Display(Prompt = "New email")]
        public string NewEmail { get; set; }
        [BindProperty]
        [Required(ErrorMessage = "You should send code before changing email.")]
        public string EmailId { get; set; }

        public IHttpClientFactory httpClientFactory;

        public ChangeEmailModel(IHttpClientFactory httpClientFactory)
        {
            this.httpClientFactory = httpClientFactory;
        }
        public IActionResult OnGet()
        {
            bool emailVerified = bool.Parse(User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Gender)?.Value ?? "false");
            if (!emailVerified)
            {
                TempData["UserError"] = "You need to verify email in order to change it.";
                return Redirect($"/user/{User.Identity.Name}");
            }
            else return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            try
            {

                if (!ModelState.IsValid)
                    return Page();
                RequiredAttribute att = new()
                {
                    ErrorMessage = "Missing ID, please resend email code."
                };
                if (!att.IsValid(EmailId))
                {
                    ModelState.AddModelError(nameof(EmailId), att.ErrorMessage);
                    return Page();
                }
                string endpoint = "api/user/changeemail1";
                using var client = httpClientFactory.CreateClient("FCApiClient");
                ChangeEmailClassModel body = new ChangeEmailClassModel()
                {
                    Code = OldCode,
                    Token = HttpContext.Request.Cookies["jwt"],
                    Password = Password,
                    NewEmail = NewEmail,
                    EmailId = Guid.Parse(EmailId),
                };

                var json = JsonSerializer.Serialize(body);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await client.PutAsync(endpoint, content);
                var resString = await response.Content.ReadAsStringAsync();
                var res = JsonSerializer.Deserialize<ServerResponse>(resString);

                if (!response.IsSuccessStatusCode)
                {
                    ModelState.AddModelError(string.Empty, res.error);
                    return Page();
                }
                Response.Cookies.Append("jwt", res.token);

                //HttpContext.Response.Cookies.Append("jwt", res.token);
                return Redirect("/verifyemail");
            }
            catch (HttpRequestException)
            {
                TempData["UserError"] = "Service is unavailable. Please try again later";
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
