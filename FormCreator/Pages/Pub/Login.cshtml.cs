using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Net.Http;
using System.Text.Json;
using System.Text;
using System.Xml.Linq;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Sockets;
using FormCreator.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using FormCreator.Services;

namespace FormCreator.Pages.Public
{
    public class LoginModel : PageModel
    {
        private readonly IHttpClientFactory httpClientFactory;
        private readonly IJWT jwtService;

        public LoginModel(IHttpClientFactory httpClientFactory, IJWT jwtConfig)
        {
            this.httpClientFactory = httpClientFactory;
            jwtService = jwtConfig;
        }
        public IActionResult OnGet()
        {
            if (User.Identity.IsAuthenticated)
                return Redirect($"/user/{User.Identity.Name}");
            else return Page();
        }
        public async Task<IActionResult> OnPostAsync(string? ret)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return Page();
                }

                var client = httpClientFactory.CreateClient("FCApiClient");
                var registrationEndpoint = "api/v1/auth/login";
                var body = new
                {
                    login = Input.Login,
                    password = Input.Password,
                };

                var json = JsonSerializer.Serialize(body);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync(registrationEndpoint, content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    var res = JsonSerializer.Deserialize<ServerResponse>(responseContent);
                    if (response.StatusCode == System.Net.HttpStatusCode.BadRequest && res.error.Contains("deleted"))
                    {
                        TempData["AccountDeletionDate"] = res.stringResponse;
                        TempData["AccountRestoreToken"] = res.token;
                        return Page();
                    }
                    ModelState.AddModelError("", res.error);

                    return Page();
                }

                string token = response.Headers.FirstOrDefault(x => x.Key == "Authorization").Value.FirstOrDefault();
                HttpContext.Response.Cookies.Append("jwt", token);
                var claims = jwtService.DecryptToken(token);
                bool emailVerified = bool.Parse(claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Gender)?.Value ?? "False");
                string id = claims.FirstOrDefault(x => x.Type == "Id")?.Value ?? Guid.Empty.ToString();
                if (emailVerified)
                    if (!string.IsNullOrEmpty(ret))
                        return Redirect(ret);
                    else
                        return Redirect($"/user/{id}");
                else
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
        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Required(ErrorMessage = "This field is required.")]
            [Display(Prompt = "Email or username")]
            public string Login { get; set; }


            [Required(ErrorMessage = "This field is required.")]
            [DataType(DataType.Password)]
            [Display(Prompt = "Password")]
            public string Password { get; set; }
        }
    }
}
