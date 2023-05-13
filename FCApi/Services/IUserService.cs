using ClassLibraryModel;

namespace FCApi.Services
{
    public interface IUserService
    {
        enum VerifyStatus { Verified, WrongCode, AlreadyVerified }
        enum LoginStatus { NoStatus, Success, NoUser, WrongPassword, AccountLocked, AccountDeleted }
        List<UserModel> GetUsers();
        UserModel GetUser(Guid id);
        UserModel? RegisterUser(UserModel user);
        bool UpdateUser(Guid id, UserModel user);
        bool RemoveUser(Guid id);
        VerifyStatus VerifyEmailCode(Guid userId, string code);
        UserModel? GetUserByLoginAndPassword(string login, string password, out LoginStatus loginStatus);
        bool SetDeletionDate(Guid id, out DateTime deletionDate);
    }
}
