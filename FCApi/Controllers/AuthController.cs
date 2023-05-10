using ClassLibraryModel;
using FCApi.Models;
using FCApi.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace FCApi.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IUserService userService;
        private readonly IJWT jwt;
        private readonly IPasswordService passwordService;

        public AuthController(IUserService userService, IJWT jwt, IPasswordService passwordService)
        {
            this.userService = userService;
            this.jwt = jwt;
            this.passwordService = passwordService;
        }
        [HttpGet("verifytoken")]
        public IActionResult VerifyToken(string? token)
        {
            if (string.IsNullOrWhiteSpace(token))
                return BadRequest(new { error = "No token." });
            var claims = jwt.DecryptToken(token);
            if (claims == null) return Unauthorized(new { error = "Invalid token." });
            var id = claims.FirstOrDefault(x => x.Type == "Id")?.Value;
            if (!Guid.TryParse(id, out Guid uid))
                return BadRequest(new { error = "Bad token." });
            var user = userService.GetUser(uid);
            var pwd = claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.UniqueName)?.Value;
            if (string.IsNullOrWhiteSpace(pwd))
                return BadRequest(new { error = "Bad pwd." });
            if (user.Password == pwd)
                return Ok(new { stringResponse = "Pass match." });
            else
                return Unauthorized(new { error = "Pass mismatch." });
        }
        [HttpPost("pwdcompare")]
        public IActionResult PasswordCompare(string e, string d)
        {
            if (passwordService.VerifyPassword(null, e, d))
                return Ok();
            else return Unauthorized();
        }
        [HttpPost("register")]
        public IActionResult Register(UserRegModel regUser)
        {
            var user = new UserModel()
            {
                Username = regUser.Username ?? regUser.Email.Split('@')[0],
                Email = regUser.Email,
                EmailVerified = false,
                FormsAvailable = 1,
                CreatedAt = DateTime.UtcNow,
                AnonymousView = false,
                Id = Guid.NewGuid(),
                AccountState = AccountState.Active,
                Password = regUser.Password,
                LastPasswordChangeTime = DateTime.UtcNow,
            };

            
            var regDbUser = userService.RegisterUser(user);
            if (regDbUser == null)
                return Unauthorized(new { error = "User already exists with this email" });

            return Ok(new { token = jwt.EncryptToken(user) });
        }
        [HttpPut("verifyemail")]
        public IActionResult VerifyEmail([FromBody] EmailVerificationRequestModel code)
        {
            if (code.Code == null)
                return Unauthorized(new { error = "No code" });
            var user = userService.GetUser(code.UserId);

            var verifyStatus = userService.VerifyEmailCode(code.UserId, code.Code);
            switch (verifyStatus)
            {
                case IUserService.VerifyStatus.Verified:
                    user.EmailVerified = true;
                    return Ok(new { token = jwt.EncryptToken(user) });
                case IUserService.VerifyStatus.AlreadyVerified:
                    return BadRequest(new { error = "Already verified.", token = jwt.EncryptToken(user) });
                case IUserService.VerifyStatus.WrongCode:
                    return Unauthorized(new { error = "Wrong code." });
            }
            return NotFound("Out of bounds.");
        }
        [HttpPost("login")]
        public IActionResult Login(UserLogModel model)
        {
            if (string.IsNullOrWhiteSpace(model.Login))
                return Unauthorized(new { error = "No login" });
            if (string.IsNullOrWhiteSpace(model.Password))
                return Unauthorized(new { error = "No password" });
            try
            {
                var user = userService.GetUserByLoginAndPassword(model.Login, model.Password, out IUserService.LoginStatus loginStatus);
                switch (loginStatus)
                {
                    case IUserService.LoginStatus.WrongPassword:
                        return Unauthorized(new { error = "Wrong password" });
                    case IUserService.LoginStatus.NoUser:
                        return Unauthorized(new { error = "User doesn't exists" });
                    case IUserService.LoginStatus.NoStatus:
                        return Unauthorized(new { error = "Failed to init user/get method" });
                    case IUserService.LoginStatus.AccountDeleted:
                        return BadRequest(new
                        {
                            error = "Account is deleted",
                            token = jwt.EncryptTokenID(user.Id),
                            stringResponse = user.DeletionDate.ToString("R"),
                        });
                    case IUserService.LoginStatus.Success:
                        return Ok(new { token = jwt.EncryptToken(user) });
                }
                return Unauthorized($"Out of expected results: {loginStatus}");
            }
            catch (Exception e)
            {
                return BadRequest(new { error = $"Failed to get user: {e.Message}" });
            }
        }
        [HttpPost("restoreaccount")]
        public IActionResult RestoreAccount(string? token)
        {
            if (string.IsNullOrWhiteSpace(token)) return BadRequest(new { error = "No token" });
            string id = jwt.DecryptTokenID(token);
            if (!Guid.TryParse(id, out Guid uid))
                return BadRequest(new { error = "Invalid token" });
            var user = userService.GetUser(uid);
            if (user == null) return NotFound(new { error = "User not found." });
            user.AccountState = AccountState.Active;
            user.DeletionDate = DateTime.MinValue;
            userService.UpdateUser(user.Id,user);
            return Ok(new { token = jwt.EncryptToken(user), userModelResponse = user });
        }
    }
}
