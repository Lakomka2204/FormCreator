using ClassLibraryModel;

namespace FCApi.Services
{
    public interface IFormService
    {
        FormModel GetForm(Guid id);
        List<FormModel> GetFormsByUser(Guid uid);
        FormModel? CreateForm(FormModel model);
        bool DeleteForm(Guid id);
        List<FormModel> SearchForm(string name);
        bool EditForm(FormModel model);
    }
}
