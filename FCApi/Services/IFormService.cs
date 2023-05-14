using ClassLibraryModel;

namespace FCApi.Services
{
    public interface IFormService
    {
        FormModelV2 GetForm(Guid id);
        List<FormModelV2> GetFormsByUser(Guid uid);
        FormModelV2? CreateForm(FormModelV2 model);
        bool DeleteForm(Guid id);
        List<FormModelV2> SearchForm(string name);
        bool EditForm(FormModelV2 model);
    }
}
