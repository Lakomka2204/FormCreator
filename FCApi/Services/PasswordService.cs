using ClassLibraryModel;
using Microsoft.AspNetCore.Identity;
using System.Buffers.Text;

namespace FCApi.Services
{
    public class PasswordService : IPasswordService
    {
        private readonly ILogger logger;

        public PasswordService(ILogger logger)
        {
            this.logger = logger;
        }
        public bool VerifyPassword(UserModel userObj, string hashed, string original)
        {
            try
            {

            PasswordHasher<UserModel> passwordHasher = new();
            return passwordHasher.VerifyHashedPassword(userObj, hashed, original) == PasswordVerificationResult.Success;
                
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex,"Was trying to verify password");
                return false;
            }
        }
        public string HashPassword(UserModel userObj, string pass)
        {
            try
            {
            PasswordHasher<UserModel> passwordHasher = new();
            return passwordHasher.HashPassword(userObj, pass);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Was trying to hash password");
                return "";
            }
        }
    }
}
