using System.Security.Claims;

namespace ClassLibraryModel
{
    public interface IJWT
    {
        string EncryptTokenID(Guid id);
        string EncryptToken(UserModel? user);
        string DecryptTokenID(string token);
        IEnumerable<Claim> DecryptToken(string token);
    }
}
