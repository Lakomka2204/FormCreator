using ClassLibraryModel;
using FCApi.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.IdentityModel.Tokens.Jwt;

namespace FormCreator.Pages.Shared
{
    public class UserAPIModel : PageModel
    {
        public IJWT JwtService { get; }
        public UserAPIModel(IJWT jwtService)
        {
            JwtService = jwtService;
            
        }
        public UserModel? GetCookieUser()
        {
            string jwtToken = HttpContext.Request.Cookies["jwt"] ?? string.Empty;
            var ss = JwtService.DecryptToken(jwtToken);
            if (ss == null)
            {
                return null;
            }

            var user = new UserModel
            {
                Email = ss?.FirstOrDefault(x => x.Type == "email")?.Value ?? "No email",
                Username = ss?.FirstOrDefault(x => x.Type == "sub")?.Value ?? "No username",
                EmailVerified = bool.Parse(ss?.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Gender)?.Value ?? "False")
            };
            string sid = ss?.FirstOrDefault(x => x.Type == "Id")?.Value ?? Guid.Empty.ToString();
            if (!Guid.TryParse(sid, out Guid id))
                return null;
            user.Id = id;
            return user;
        }
    }
}

