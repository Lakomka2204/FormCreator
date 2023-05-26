using ClassLibraryModel;
using Microsoft.Extensions.Primitives;
using System.IdentityModel.Tokens.Jwt;

namespace FCApi.Services
{
    public class TokenService : ITokenService
    {
        private readonly IJWT jwt;
        private readonly IUserService userService;

        public TokenService(IJWT jwt, IUserService userService)
        {
            this.jwt = jwt;
            this.userService = userService;
        }
        public enum ValidationState
        {
            None,
            Valid,
            NoUser,
            InvalidId,
            InvalidAuth,
            NoAuth
        }
        public static string GetStatus(ValidationState state)
        {
            return state switch
            {
                ValidationState.Valid => state.ToString(),
                ValidationState.NoUser => "User not found.",
                ValidationState.InvalidId => "GUID parse error",
                ValidationState.InvalidAuth or ValidationState.NoAuth => "No token.",
                _ => "Internal error.",
            };
        }
        public StringValues CreateAuthorizationToken(UserModel? user)
        {
            if (user == null) return "Bearer No user";
            return jwt.EncryptToken(user);
        }
        public StringValues CreateAuthorizationToken(Guid id)
        {
            return jwt.EncryptTokenID(id);
        }
        public string? GetClientPassword(StringValues authorization)
        {
            string? bearer = authorization.FirstOrDefault();
            if (bearer == null)
                return null;
            string token = bearer.Split(' ')[1];
            var claims = jwt.DecryptToken(token);
            if (claims == null) return null;
            return claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.UniqueName)?.Value;
        }
        public ValidationState ValidateRequestToken(StringValues authorization, out UserModel? user)
        {
            user = null;
            try
            {
                if (authorization.Count == 0)
                    return ValidationState.NoAuth;
                string? bearer = authorization.FirstOrDefault();
                if (bearer == null)
                    return ValidationState.InvalidAuth;
                string token = bearer.Split(' ')[1];
                string localUserId = jwt.DecryptTokenID(token);
                if (!Guid.TryParse(localUserId, out Guid uid))
                    return ValidationState.InvalidId;
                user = userService.GetUser(uid);
                if (user != null)
                    return ValidationState.Valid;
                else
                    return ValidationState.NoUser;
            }
            catch
            {
                return ValidationState.None;
            }
        }
    }
}
