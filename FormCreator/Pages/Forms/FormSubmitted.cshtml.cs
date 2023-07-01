using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FormCreator.Pages.Forms
{
    public class FormSubmittedModel : PageModel
    {
        [BindProperty]
        public bool HasID { get; set; }
        public IActionResult OnGet()
        {
            string fid = Request.Cookies["FORM_ID"];
            HasID = fid != null && Guid.TryParse(fid, out _);
            if (HasID)
                Response.Cookies.Delete("FORM_ID");
            return Page();
        }
    }
}
