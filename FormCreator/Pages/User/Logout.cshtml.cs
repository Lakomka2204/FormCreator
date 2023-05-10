using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FormCreator.Pages.User
{
    [Authorize]
    public class LogoutModel : PageModel
    {
        public IActionResult OnGet()
        {
            HttpContext.Response.Cookies.Delete("jwt");
            return RedirectToPage("/index");
        }
    }
}
