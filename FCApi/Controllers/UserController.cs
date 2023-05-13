using ClassLibraryModel;
using FCApi.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace FCApi.Controllers
{
    [Route("api/user")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService userService;
        private readonly IJWT jwt;
        private readonly IPasswordService passwordService;
        private readonly IEmailService emailService;

        public UserController(IUserService userService, IJWT jwt, IPasswordService passwordService, IEmailService emailService)
        {
            this.userService = userService;
            this.jwt = jwt;
            this.passwordService = passwordService;
            this.emailService = emailService;
        }
        [HttpGet("{id}")]
        public IActionResult GetUserById(string? id)
        {
            if (id == null) return BadRequest(new { error = "No id." });
            if (!Guid.TryParse(id, out Guid uid))
                return BadRequest(new { error = "Invalid id." });
            var user = userService.GetUser(uid);
            if (user == null) return NotFound(new { error = $"User not found." });
            if (!user.AnonymousView) return Unauthorized(new { error = "No permission to view user." });
            var protectedUser = new
            {
                id = user.Id,
                username = user.Username,
                createdAt = user.CreatedAt,
                formsAvailable = user.FormsAvailable,
                emailVerified = user.EmailVerified,
            };
            return Ok(new { userModelResponse = protectedUser });
        }
        [HttpPost("changepass")]
        public IActionResult ChangePassword([FromBody] ChangePasswordModel model)
        {
            if (string.IsNullOrWhiteSpace(model.Token)) return BadRequest(new { error = "No token." });
            string id = jwt.DecryptTokenID(model.Token);
            if (!Guid.TryParse(id, out Guid uid))
                return BadRequest(new { error = id });
            if (model.OldPassword == model.NewPassword)
                return BadRequest(new { error = "The passwords are the same." });
            var user = userService.GetUser(uid);
            PasswordHasher<UserModel> passwordHasher = new();
            if (passwordHasher.VerifyHashedPassword(user, user.Password, model.OldPassword) != PasswordVerificationResult.Success)
                return Unauthorized(new { error = "Wrong password." });
            user.Password = passwordHasher.HashPassword(user, model.NewPassword);
            user.LastPasswordChangeTime = DateTime.UtcNow;
            userService.UpdateUser(user.Id, user);
            return Ok(new { stringResponse = "Changed password.", token = jwt.EncryptToken(user) });
        }
        [HttpGet("changevisibility")]
        public IActionResult ChangeVisibility(string? token)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(token)) return BadRequest(new { error = "No token" });
                string id = jwt.DecryptTokenID(token);
                if (!Guid.TryParse(id, out Guid uid))
                    return BadRequest(new { error = "Invalid token" });
                var user = userService.GetUser(uid);
                user.AnonymousView = !user.AnonymousView;
                userService.UpdateUser(uid, user);
                return Ok(new { boolResponse = user.AnonymousView });
            }
            catch
            {
                return BadRequest(new { error = "Failed to parse token." });
            }
        }
        [HttpPut("changeemail0")]
        public IActionResult ChangeEmailOld([FromBody] ChangeEmailClassModel model)
        {
            if (model.Code != "bazinga") return Unauthorized(new { error = "System hacking... 94%" });

            if (string.IsNullOrWhiteSpace(model.Token)) return BadRequest(new { error = "No token" });
            string id = jwt.DecryptTokenID(model.Token);
            if (!Guid.TryParse(id, out Guid uid))
                return BadRequest(new { error = "Invalid token" });

            var user = userService.GetUser(uid);
            if (!passwordService.VerifyPassword(user, user.Password, model.Password))
                return Unauthorized(new { error = "Invalid password, probably has been changed asynchronously." });
            if (!emailService.CanSendEmail(uid))
                return StatusCode(429,new {error= "Email cooldown, please try again in 5 minutes." });
            var emailId = emailService.SendVerificationCodeEmail(uid, user.Email, "Email change");

            return Ok(new { stringResponse = emailId.ToString() });
        }
        [HttpPut("changeemail1")]
        public IActionResult ChangeEmail([FromBody] ChangeEmailClassModel model)
        {
            if (string.IsNullOrWhiteSpace(model.Token)) return BadRequest(new { error = "No token" });
            string id = jwt.DecryptTokenID(model.Token);
            if (!Guid.TryParse(id, out Guid uid))
                return BadRequest(new { error = "Invalid token" });
            bool codeMatch = emailService.CheckCodeById(model.EmailId, model.Code);
            if (!codeMatch) return Unauthorized(new { error = "Invalid code." });

            var user = userService.GetUser(uid);
            //if (!emailService.CanSendEmail(uid)) return Unauthorized(new { error = "Email cooldown, please try again in 5 minutes." });
            var emailValidator = new EmailAddressAttribute();
            if (!emailValidator.IsValid(model.NewEmail)) return BadRequest(new { error = "Email invalid." });
            user.Email = model.NewEmail;
            user.EmailVerified = false;
            emailService.SendVerificationCodeEmail(user.Id, model.NewEmail, "New email verification");
            userService.UpdateUser(uid, user);

            return Ok(new { token = jwt.EncryptToken(user) });
        }
        [HttpPost("changeemailforce")]
        public IActionResult ChangeEmailForce([FromBody] ChangeEmailClassModel model)
        {
            if (string.IsNullOrWhiteSpace(model.Token)) return BadRequest(new { error = "No token" });
            string id = jwt.DecryptTokenID(model.Token);
            if (!Guid.TryParse(id, out Guid uid))
                return BadRequest(new { error = "Invalid token" });
            var user = userService.GetUser(uid);
            if (user.EmailVerified)
                return StatusCode(StatusCodes.Status405MethodNotAllowed,new { error = "Not allowed." });

            var emailValidator = new EmailAddressAttribute();
            if (!emailValidator.IsValid(model.NewEmail)) return BadRequest(new { error = "Email invalid." });
            user.Email = model.NewEmail;
            emailService.SendVerificationCodeEmail(user.Id, model.NewEmail, "New email verification");
            userService.UpdateUser(uid, user);
            return Ok(new { token = jwt.EncryptToken(user) });
        }
        [HttpPut("deleteaccount")]
        public IActionResult DeleteAccount([FromBody] DeleteAccountClassModel model)
        {
            string? token = model.Token;
            if (string.IsNullOrWhiteSpace(token)) return Unauthorized(new { error = "No token" });
            string id = jwt.DecryptTokenID(token);
            if (!Guid.TryParse(id, out Guid uid))
                return BadRequest(new { error = "Invalid token" });
            var user = userService.GetUser(uid);
            if (user == null) return NotFound(new { error = "No user." });
            if (!passwordService.VerifyPassword(user, user.Password, model.Password))
                return BadRequest(new { error = "Wrong password." });
            var isDeleted = userService.SetDeletionDate(uid, out DateTime deletionDate);
            if (isDeleted)
                return Ok(new { boolResponse = isDeleted, stringResponse = deletionDate.ToString("R") });
            else return BadRequest(new { error = "Account is already planned for deletion", stringResponse = user.DeletionDate.ToString() });
        }
        [HttpGet("")]
        public IActionResult GetUserByToken(string? token)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(token)) return BadRequest(new { error = "No token" });
                string id = jwt.DecryptTokenID(token);
                if (!Guid.TryParse(id, out Guid uid))
                    return BadRequest(new { error = "No token." });
                var user = userService.GetUser(uid);
                var userNoPass = new
                {
                    user.Id,
                    user.Username,
                    user.Email,
                    user.EmailVerified,
                    user.FormsAvailable,
                    user.CreatedAt,
                    user.AnonymousView,
                    user.AccountState,
                    user.LastPasswordChangeTime,
                };
                return Ok(new { userModelResponse = userNoPass });
            }
            catch
            {
                return BadRequest(new { error = $"Failed to parse token." });
            }
        }
    }
}
