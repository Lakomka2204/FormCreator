using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text;
using System.Text.Json;

namespace FormCreator.Pages.User
{
    public class SettingsModel : PageModel
    {
        public bool Dark { get; set; }
        public IActionResult OnPost()
        {
            SetDarkState();
            Dark = !Dark;
            HttpContext.Response.Cookies.Append("local_dark", Convert.ToBase64String(Encoding.UTF8.GetBytes(Dark.ToString())));
            return RedirectToPage("/User/Settings");
        }
        public void SetDarkState()
        {
            string b64 = Request.Cookies["local_dark"];
            if (b64 == null)
            {
                Dark = false;
                return;
            }
            string val = Encoding.UTF8.GetString(Convert.FromBase64String(b64));
            Dark = bool.Parse(val);
        }
        public void OnGet()
        {
            SetDarkState();
            HttpContext.Response.Cookies.Append("local_dark", Convert.ToBase64String(Encoding.UTF8.GetBytes(Dark.ToString())));
        }
    }
}
