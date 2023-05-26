using ClassLibraryModel;
using Microsoft.Extensions.Primitives;
using static FCApi.Services.TokenService;

namespace FCApi.Services
{
    public interface ITokenService
    {
        ValidationState ValidateRequestToken(StringValues authorization, out UserModel? user);
        StringValues CreateAuthorizationToken(Guid id);
        StringValues CreateAuthorizationToken(UserModel user);
        string? GetClientPassword(StringValues authorization);
    }
}
