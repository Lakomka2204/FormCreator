using ClassLibraryModel;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FormCreator.Pages.FormElements
{
    public class FormDisplayElementModel : PageModel
    {
        public GeneralFormElementModel Element { get; set; }
        public GeneralFormSubmissionModel Submission { get; set; }
        public List<Tuple<GeneralFormSubmissionModel,Guid>> MultiSubmissions { get; set; }

        public FormDisplayMode Mode { get; set; }
    }
}
