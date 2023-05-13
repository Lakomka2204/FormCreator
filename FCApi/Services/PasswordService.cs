using ClassLibraryModel;
using Microsoft.AspNetCore.Identity;

namespace FCApi.Services
{
    public class PasswordService : IPasswordService
    {
        public bool VerifyPassword(UserModel userObj, string hashed, string original)
        {
            PasswordHasher<UserModel> passwordHasher = new();
            return passwordHasher.VerifyHashedPassword(userObj, hashed, original) == PasswordVerificationResult.Success;
        }
        public string HashPassword(UserModel userObj, string pass)
        {
            PasswordHasher<UserModel> passwordHasher = new();
            return passwordHasher.HashPassword(userObj, pass);
        }
    }
}
