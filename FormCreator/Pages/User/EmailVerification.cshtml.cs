using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using FormCreator.Pages.Shared;
using System.Text;
using System.Text.Json;
using System.ComponentModel.DataAnnotations;
using ClassLibraryModel;
using Microsoft.AspNetCore.Authorization;

namespace FormCreator.Pages.User
{
    [Authorize]
    public class EmailVerificationModel : UserAPIModel
    {
        private readonly IHttpClientFactory httpClientFactory;

        [BindProperty]
        [Display(Name = "Verification Code")]
        [Range(100000, 999999, ErrorMessage = "Verification Code must be a 6-digit number")]
        public string VerificationCode { get; set; }


        public EmailVerificationModel(IJWT jwtService, IHttpClientFactory httpClientFactory)
            : base(jwtService)
        {
            this.httpClientFactory = httpClientFactory;
        }

        public async Task<IActionResult> OnPostAsync()
        {
            try
            {

                if (!ModelState.IsValid) return Page();
                UserModel? user = GetCookieUser();
                if (user == null)
                {
                    TempData["UserError"] = "Cookies are missing, please relog again.";
                    return RedirectToPage("/login");
                }
                var client = httpClientFactory.CreateClient("FCApiClient");
                var body = new
                {
                    userId = user.Id,
                    code = VerificationCode,
                };
                var json = JsonSerializer.Serialize(body);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PutAsync("api/auth/verifyemail", content);
                var responseContent = await response.Content.ReadAsStringAsync();
                var res = JsonSerializer.Deserialize<ServerResponse>(responseContent);
                if (response.IsSuccessStatusCode || res.error == "Already verified.")
                {
                    //HttpContext.Response.Cookies.Delete("jwt");
                    HttpContext.Response.Cookies.Append("jwt", res.token);
                    return RedirectToPage("/index");
                }
                else
                {
                    TempData["EmailVerificationError"] = res.error;
                    //ModelState.AddModelError(string.Empty, res.error);
                    return Page();
                }
            }
            catch (Exception e)
            {
                TempData["UserError"] = e.Message;
                return Page();
            }
        }
    }

    public record VerifyEmailRequest(Guid userId, string code);
}