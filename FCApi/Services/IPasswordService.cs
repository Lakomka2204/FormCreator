using ClassLibraryModel;

namespace FCApi.Services
{
    public interface IPasswordService
    {
        bool VerifyPassword(UserModel userObj, string hashed, string original);
        string HashPassword(UserModel userObj, string pass);
    }
}
