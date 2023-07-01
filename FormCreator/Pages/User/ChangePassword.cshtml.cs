using System.ComponentModel.DataAnnotations;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using FormCreator.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FormCreator.Pages.User
{
    [Authorize]
    public class ChangePasswordModel : PageModel
    {
        private readonly IHttpClientFactory httpClientFactory;
        [Required(ErrorMessage = "This field is required.")]
        [BindProperty]
        [DataType(DataType.Password)]
        [Display(Prompt = "Old Password")]
        public string OldPassword { get; set; }
        [Required(ErrorMessage = "This field is required.")]
        [BindProperty]
        [DataType(DataType.Password)]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at most {1} characters long.", MinimumLength = 6)]
        [Display(Name = "New Password",Prompt = "New Password")]
        public string NewPassword { get; set; }

        [BindProperty]
        [DataType(DataType.Password)]
        [Required(ErrorMessage = "This field is required.")]
        [Display(Prompt = "Confirm new password")]
        [Compare(nameof(NewPassword), ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmNewPassword { get; set; }

        public string ErrorMessage { get; set; }

        public ChangePasswordModel(IHttpClientFactory httpClientFactory)
        {
            this.httpClientFactory = httpClientFactory;
        }
        public IActionResult OnGetAsync()
        {
            bool emailVerified = bool.Parse(User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Gender)?.Value ?? "false");
            if (!emailVerified)
            {
                TempData["UserError"] = "You need to verify email in order to change password.";
                return Redirect($"/user/{HttpContext.User.Identity.Name}");
            }
            return Page();
        }
        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return Page();
                }

                var token = Request.Cookies["jwt"];
                if (string.IsNullOrEmpty(token))
                {
                    ErrorMessage = "You are not authorized to perform this action. Please log in.";
                    return Page();
                }

                var payload = new
                {
                    token,
                    oldPassword = OldPassword,
                    newPassword = NewPassword
                };

                var jsonPayload = JsonSerializer.Serialize(payload);
                var stringContent = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
                var client = httpClientFactory.CreateClient("FCApiClient");
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var response = await client.PostAsync("api/v1/user/changepass", stringContent);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    return RedirectToPage("/User/Logout");
                }
                else
                {
                    var res = JsonSerializer.Deserialize<ServerResponse>(responseContent);
                    ErrorMessage = res?.error ?? "An error occurred while changing your password. Please try again.";
                    return Page();
                }
            }
            catch (HttpRequestException)
            {
                ErrorMessage = "Service is unavailable. Please try again later";
                return Page();
            }
            catch (Exception e)
            {
                ErrorMessage = e.Message;
                return Page();
            }
        }
    }
}