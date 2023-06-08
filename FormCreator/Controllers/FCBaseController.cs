using FormCreator.Models;
using FormCreator.Services;
using Microsoft.AspNetCore.Mvc;

namespace FormCreator.Controllers
{
    [Route("api/v{version:apiVersion}/")]
    [ApiController]
    [ApiVersion("1")]
    public class FCBaseController : ControllerBase
    {
        [HttpGet("")]
        public IActionResult Test()
        {
            return Ok();
        }
        public readonly IUserService userService;
        public readonly IJWT jwt;
        public FCBaseController(IUserService userService, IJWT jwt)
        {
            this.userService = userService;
            this.jwt = jwt;
        }
        internal AuthStatus CheckAuth(Func<UserModel, bool?>? compare, out string? jwtError, out UserModel? user)
        {
            jwtError = null;
            user = null;
            var jwt = HttpContext.Request.Cookies["jwt"];
            if (jwt == null)
                return AuthStatus.NoJwt;
            var id = this.jwt.DecryptTokenID(jwt);
            if (!Guid.TryParse(id, out Guid uid))
            {
                jwtError = jwt;
                return AuthStatus.ParseTokenFail;
            }
            var dbUser = userService.GetUser(uid);
            user = dbUser;
            var compareResult = compare?.Invoke(dbUser);
            return compareResult ?? false ? AuthStatus.Pass : AuthStatus.CompareFail;
        }
        public enum AuthStatus { NoJwt, ParseTokenFail, CompareFail, Pass }
    }
}
