using System.ComponentModel.DataAnnotations;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using ClassLibraryModel;
using FormCreator.Pages.Shared;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FormCreator.Pages.Public
{
    public class RegisterModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public RegisterModel(
            IHttpClientFactory httpClientFactory
            )
        {
            _httpClientFactory = httpClientFactory;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Required(ErrorMessage = "This field is required.")]
            [EmailAddress]
            [Display(Prompt = "Email address (required)")]
            public string Email { get; set; }

            [Display(Prompt = "Username")]
            [RegularExpression(@"^[a-z0-9_\.]+$", ErrorMessage = "Username can only consist of lowercase letters, numbers, periods, and underscores")]
            public string? Username { get; set; }

            [Required(ErrorMessage = "This field is required.")]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at most {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Prompt = "Password (required)")]
            public string Password { get; set; }

            [Required(ErrorMessage = "This field is required.")]
            [DataType(DataType.Password)]
            [Display(Prompt = "Confirm Password (required)")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }
        }

        public IActionResult OnGet()
        {
            if (HttpContext?.User?.Identity?.IsAuthenticated ?? false)
                return Redirect($"/user/{HttpContext.User.Identity.Name}");
            else return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var client = _httpClientFactory.CreateClient("FCApiClient");
            var registrationEndpoint = "api/v1/auth/register";
            var username = Input.Username ?? Input.Email.Split('@')[0];
            var body = new
            {
                email = Input.Email,
                username,
                password = Input.Password,
                confirmPassword = Input.Password,
            };

            var json = JsonSerializer.Serialize(body);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            try
            {

                var response = await client.PostAsync(registrationEndpoint, content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                var res = JsonSerializer.Deserialize<ServerResponse>(responseContent);
                    ModelState.AddModelError("", res.error);

                    return Page();
                }
                string token = response.Headers.FirstOrDefault(x => x.Key == "Authorization").Value.FirstOrDefault();
                HttpContext.Response.Cookies.Append("jwt", token);
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
