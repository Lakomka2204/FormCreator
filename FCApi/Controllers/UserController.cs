using ClassLibraryModel;
using FCApi.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace FCApi.Controllers
{
    [Route("api/v{version:apiVersion}/user")]
    [ApiController]
    [ApiVersion("1")]
    public class UserController : ControllerBase
    {
        private readonly IUserService userService;
        private readonly IPasswordService passwordService;
        private readonly IEmailService emailService;
        private readonly ITokenService tokenService;

        public UserController(IUserService userService, IPasswordService passwordService, IEmailService emailService, ITokenService tokenService)
        {
            this.userService = userService;
            this.passwordService = passwordService;
            this.emailService = emailService;
            this.tokenService = tokenService;
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
            TokenService.ValidationState vs;
            if ((vs = tokenService.ValidateRequestToken(Request.Headers.Authorization, out UserModel? user)) != TokenService.ValidationState.Valid)
                return Unauthorized(new { error = TokenService.GetStatus(vs) });
            if (model.OldPassword == model.NewPassword)
                return BadRequest(new { error = "The passwords are the same." });
            if (!passwordService.VerifyPassword(user, user.Password, model.OldPassword))
                return Unauthorized(new { error = "Wrong password." });
            user.Password = passwordService.HashPassword(user, model.NewPassword);
            user.LastPasswordChangeTime = DateTime.UtcNow;
            userService.UpdateUser(user.Id, user);
            Response.Headers.Authorization = tokenService.CreateAuthorizationToken(user);
            return Ok();
        }
        [HttpGet("changevisibility")]
        public IActionResult ChangeVisibility()
        {
            try
            {
                TokenService.ValidationState vs;
                if ((vs = tokenService.ValidateRequestToken(Request.Headers.Authorization, out UserModel? user)) != TokenService.ValidationState.Valid)
                    return Unauthorized(new { error = TokenService.GetStatus(vs) });
                user.AnonymousView = !user.AnonymousView;
                userService.UpdateUser(user.Id, user);
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

            if (tokenService.ValidateRequestToken(Request.Headers.Authorization, out UserModel? user) != TokenService.ValidationState.Valid)
                return Unauthorized(new { error = "No token." });
            if (!passwordService.VerifyPassword(user, user.Password, model.Password))
                return Unauthorized(new { error = "Invalid password, probably has been changed asynchronously." });
            if (!emailService.CanSendEmail(user.Id))
                return StatusCode(429,new {error= "Email rate limit." });
            var emailId = emailService.SendVerificationCodeEmail(user.Id, user.Email, "Email change");

            return Ok(new { stringResponse = emailId.ToString() });
        }
        [HttpPut("changeemail1")]
        public IActionResult ChangeEmail([FromBody] ChangeEmailClassModel model)
        {
            TokenService.ValidationState vs;
            if ((vs = tokenService.ValidateRequestToken(Request.Headers.Authorization, out UserModel? user)) != TokenService.ValidationState.Valid)
                return Unauthorized(new { error = TokenService.GetStatus(vs) });
            bool codeMatch = emailService.CheckCodeById(model.EmailId, model.Code);
            if (!codeMatch)
                return Unauthorized(new { error = "Invalid code." });
            //if (!emailService.CanSendEmail(user.Id))
            //    return Unauthorized(new { error = "Email rate limit." });
            var emailValidator = new EmailAddressAttribute();
            if (!emailValidator.IsValid(model.NewEmail))
                return BadRequest(new { error = "Email invalid." });
            user.Email = model.NewEmail;
            user.EmailVerified = false;
            emailService.SendVerificationCodeEmail(user.Id, model.NewEmail, "New email verification");
            userService.UpdateUser(user.Id, user);
            Response.Headers.Authorization = tokenService.CreateAuthorizationToken(user);
            return Ok();
        }
        [HttpPost("changeemailforce")]
        public IActionResult ChangeEmailForce([FromBody] ChangeEmailClassModel model)
        {
            TokenService.ValidationState vs;
            if ((vs = tokenService.ValidateRequestToken(Request.Headers.Authorization, out UserModel? user)) != TokenService.ValidationState.Valid)
                return Unauthorized(new { error = TokenService.GetStatus(vs) });
            if (user.EmailVerified)
                return StatusCode(StatusCodes.Status405MethodNotAllowed,new { error = "Not allowed." });

            var emailValidator = new EmailAddressAttribute();
            if (!emailValidator.IsValid(model.NewEmail)) return BadRequest(new { error = "Email invalid." });
            user.Email = model.NewEmail;
            emailService.SendVerificationCodeEmail(user.Id, model.NewEmail, "New email verification");
            userService.UpdateUser(user.Id, user);
            Response.Headers.Authorization = tokenService.CreateAuthorizationToken(user);
            return Ok();
        }
        [HttpPut("deleteaccount")]
        public IActionResult DeleteAccount([FromBody] DeleteAccountClassModel model)
        {
            TokenService.ValidationState vs;
            if ((vs = tokenService.ValidateRequestToken(Request.Headers.Authorization, out UserModel? user)) != TokenService.ValidationState.Valid)
                return Unauthorized(new { error = TokenService.GetStatus(vs) });

            if (!passwordService.VerifyPassword(user, user.Password, model.Password))
                return BadRequest(new { error = "Wrong password." });
            var isDeleted = userService.SetDeletionDate(user.Id, out DateTime deletionDate);
            if (isDeleted)
                return Ok(new { boolResponse = isDeleted, stringResponse = deletionDate.ToString("R") });
            else return BadRequest(new { error = "Account is already planned for deletion", stringResponse = user.DeletionDate.ToString() });
        }
        [HttpGet("self")]
        public IActionResult GetUserByToken()
        {
            try
            {
                TokenService.ValidationState vs;
                if ((vs = tokenService.ValidateRequestToken(Request.Headers.Authorization, out UserModel? user)) != TokenService.ValidationState.Valid)
                    return Unauthorized(new { error = TokenService.GetStatus(vs) });
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
                return BadRequest(new { error = "Failed to parse token." });
            }
        }
    }
}
