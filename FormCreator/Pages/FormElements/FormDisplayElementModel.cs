using FormCreator.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FormCreator.Pages.FormElements
{
    public class FormDisplayElementModel : PageModel
    {
        public GeneralFormElementModel Element { get; set; }
        public GeneralFormSubmissionModel Submission { get; set; }
        public List<MSObject> MultiSubmissions { get; set; }
        public class MSObject
        {
            public GeneralFormSubmissionModel Submission { get; set; }
            public Guid Id { get; set; }
            public string Username { get; set; }
        }
        public FormDisplayMode Mode { get; set; }
    }
}
