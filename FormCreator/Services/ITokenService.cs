using FormCreator.Models;
using Microsoft.Extensions.Primitives;
using static FormCreator.Services.TokenService;

namespace FormCreator.Services
{
    public interface ITokenService
    {
        ValidationState ValidateRequestToken(StringValues authorization, out UserModel? user);
        StringValues CreateAuthorizationToken(Guid id);
        StringValues CreateAuthorizationToken(UserModel user);
        string? GetClientPassword(StringValues authorization);
    }
}
