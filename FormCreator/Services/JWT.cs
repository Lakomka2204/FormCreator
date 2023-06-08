using FormCreator.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace FormCreator.Services
{
    public class JWT : IJWT
    {
        private readonly IJWTConfig config;

        public JWT(IJWTConfig config)
        {
            this.config = config;
        }
        public string EncryptToken(UserModel? user)
        {
            var key = Encoding.ASCII.GetBytes(config.SecretKey);
            var tokenDescriptor = new SecurityTokenDescriptor()
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(nameof(UserModel.Id),user?.Id.ToString() ?? string.Empty),
                    new Claim(JwtRegisteredClaimNames.Email,user?.Email ?? string.Empty),
                    new Claim(JwtRegisteredClaimNames.Sub,user?.Username ?? string.Empty),
                    new Claim(JwtRegisteredClaimNames.UniqueName,user?.Password ?? string.Empty),
                    new Claim(JwtRegisteredClaimNames.Gender,user?.EmailVerified.ToString() ?? "False"),
                    new Claim(JwtRegisteredClaimNames.Jti,Guid.NewGuid().ToString())
                }),
                Expires = DateTime.UtcNow.AddMonths(1),
                Issuer = config.Issuer,
                Audience = config.Audience,
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha512Signature)
            };
            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var jwtToken = tokenHandler.WriteToken(token);
            return jwtToken;
        }

        public string DecryptTokenID(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(config.SecretKey);
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = config.Issuer,
                ValidateAudience = true,
                ValidAudience = config.Audience,
                ValidateLifetime = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuerSigningKey = true
            };

            try
            {
                var claimsPrincipal = tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);
                var jwtToken = (JwtSecurityToken)validatedToken;
                var id = jwtToken.Claims.FirstOrDefault(x => x.Type == nameof(UserModel.Id))?.Value;

                if (id != null)
                    return id;
                else
                    return "Token is missing user ID claim.";
            }
            catch (Exception ex)
            {
#if DEBUG
                return "Token validation failed: " + ex.Message;
#else
                return "Token validation failed.";
#endif
            }
        }
        public IEnumerable<Claim> DecryptToken(string token)
        {
            if (string.IsNullOrWhiteSpace(token)) return null;
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(config.SecretKey);
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = config.Issuer,
                ValidateAudience = true,
                ValidAudience = config.Audience,
                ValidateLifetime = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuerSigningKey = true
            };

            try
            {
                var claimsPrincipal = tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);
                var jwtToken = (JwtSecurityToken)validatedToken;
                return jwtToken.Claims;
            }
            catch
            {
                return null;
            }
            
        }

        public string EncryptTokenID(Guid id)
        {
            var key = Encoding.ASCII.GetBytes(config.SecretKey);
            var tokenDescriptor = new SecurityTokenDescriptor()
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(nameof(UserModel.Id),id.ToString() ?? string.Empty),
                    new Claim(JwtRegisteredClaimNames.Jti,Guid.NewGuid().ToString())
                }),
                Expires = DateTime.UtcNow.AddMonths(1),
                Issuer = config.Issuer,
                Audience = config.Audience,
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha512Signature)
            };
            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var jwtToken = tokenHandler.WriteToken(token);
            return jwtToken;
        }
    }
}
