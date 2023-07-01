using FormCreator.Models;
using FormCreator.Services;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace FormCreator.Controllers
{
    [Route("api/v{version:apiVersion}/forms")]
    [ApiController]
    [ApiVersion("1")]
    public class FormController : ControllerBase
    {
        private readonly IFormService formService;
        private readonly ITokenService tokenService;
        private readonly ISubmissionService submissionService;
        private readonly IUserService userService;

        public FormController(IFormService formService, ITokenService tokenService, ISubmissionService submissionService, IUserService userService)
        {
            this.formService = formService;
            this.tokenService = tokenService;
            this.submissionService = submissionService;
            this.userService = userService;
        }
        [HttpGet("form")]
        public IActionResult GetById(string? id)
        {
            tokenService.ValidateRequestToken(HttpContext.Request.Headers.Authorization, out UserModel? user);
            if (id == null) return BadRequest(new { error = "No form id." });
            if (!Guid.TryParse(id, out Guid fid))
                return BadRequest(new { error = "Invalid form id." });
            var form = formService.GetForm(fid);
            if (form == null) return NotFound(new { error = "Form not found." });
            bool isOwner = user != null && form?.OwnerId == user.Id;
            if (!isOwner)
                FormModel.RemovePrivateProperties(form);
            return Ok(new { formModelResponse = form });
        }
        [HttpGet("user")]
        public IActionResult GetUserForms(string? id)
        {
            if (id == null) return BadRequest(new { error = "No user id." });
            if (!Guid.TryParse(id, out Guid uid))
                return BadRequest(new { error = "Invalid user id." });
            tokenService.ValidateRequestToken(HttpContext.Request.Headers.Authorization, out UserModel? user);
            if (user != null && !user.EmailVerified)
                return StatusCode(StatusCodes.Status403Forbidden, new { error = "Email is not verified." });
            var forms = formService.GetFormsByUser(uid);
            for (int i = 0; i < forms.Count; i++)
            {
                if (user == null || forms[i].OwnerId != user.Id)
                {
                    if (forms[i].CanBeSearched)
                        FormModel.RemovePrivateProperties(forms[i]);
                    else
                        forms.RemoveAt(i);
                }
            }
            return Ok(new { formsModelResponse = forms });
        }
        [HttpPost("create")]
        public IActionResult CreateForm([FromBody][Required] FormModel model)
        {
            TokenService.ValidationState vs;
            if ((vs = tokenService.ValidateRequestToken(Request.Headers.Authorization, out UserModel? user)) != TokenService.ValidationState.Valid)
                return Unauthorized(new { error = TokenService.GetStatus(vs) });
            if (!user.EmailVerified)
                return StatusCode(StatusCodes.Status403Forbidden, new { error = "Email is not verified." });
            if (model == null && model.FormElements.Any(x => x.QuestionType == QuestionType.None))
                return NotFound(new { error = "Question type must not be None" });
            var uforms = formService.GetFormsByUser(user.Id).Count;
            if (uforms >= user.FormsAvailable)
                return StatusCode(StatusCodes.Status403Forbidden, new { error = $"Ran out of available forms." });
            model.Id = Guid.NewGuid();
            model.OwnerId = user.Id;
            var createdModel = formService.CreateForm(model);
            if (createdModel == null) return NotFound(new { error = "No form questions" });
            return Ok(new { stringResponse = model?.Id });
        }
        [HttpDelete("delete")]
        public IActionResult DeleteForm(string? id)
        {
            TokenService.ValidationState vs;
            if ((vs = tokenService.ValidateRequestToken(Request.Headers.Authorization, out UserModel? user)) != TokenService.ValidationState.Valid)
                return Unauthorized(new { error = TokenService.GetStatus(vs) });
            if (!user.EmailVerified)
                return StatusCode(StatusCodes.Status403Forbidden, new { error = "Email is not verified." });
            if (!Guid.TryParse(id, out Guid fid))
                return BadRequest(new { error = "Invalid form ID." });

            var form = formService.GetForm(fid);
            if (form == null) return NotFound(new { error = "Form not found." });

            if (form.OwnerId != user.Id) return StatusCode(StatusCodes.Status403Forbidden, new { error = "No permissions" });
            formService.DeleteForm(fid);
            foreach (var sub in submissionService.GetSubmissionsByForm(fid))
                submissionService.DeleteSubmission(sub.Id);
            return Ok(new { boolResponse = true, stringResponse = form.Name });
        }
        [HttpDelete("deleteall")]
        public IActionResult DeleteAll()
        {
            TokenService.ValidationState vs;
            if ((vs = tokenService.ValidateRequestToken(Request.Headers.Authorization, out UserModel? user)) != TokenService.ValidationState.Valid)
                return Unauthorized(new { error = TokenService.GetStatus(vs) });
            if (!user.EmailVerified)
                return StatusCode(StatusCodes.Status403Forbidden, new { error = "Email is not verified." });
            var forms = formService.GetFormsByUser(user.Id);
            int formsDeleted = 0;
            foreach (var form in forms)
            {
                if (formService.DeleteForm(form.Id))
                    formsDeleted++;
            }
            return Ok(new { stringResponse = $"Deleted {formsDeleted} forms." });
        }
        [HttpGet("search")]
        public IActionResult SearchForm(string? q)
        {
            if (string.IsNullOrWhiteSpace(q)) return BadRequest(new { error = "No query input" });
            return Ok(new { formsModelResponse = formService.SearchForm(q) });
        }
        [HttpPut("edit")]
        public IActionResult EditForm([Required][FromBody] FormModel newModel)
        {
            TokenService.ValidationState vs;
            if ((vs = tokenService.ValidateRequestToken(Request.Headers.Authorization, out UserModel? user)) != TokenService.ValidationState.Valid)
                return Unauthorized(new { error = TokenService.GetStatus(vs) });
            if (!user.EmailVerified)
                return StatusCode(StatusCodes.Status403Forbidden, new { error = "Email is not verified." });
            var form = formService.GetForm(newModel.Id);
            if (form == null) return NotFound(new { error = "Form not found." });
            if (form.OwnerId != user.Id) return Unauthorized(new { error = "No permissions" });
            newModel.OwnerId = user.Id;
            if (form == newModel) return BadRequest(new { error = "Form hasn't changed." });
            formService.EditForm(newModel);
            return Ok(new { boolResponse = true });
        }
        [HttpPost("submissions/submit")]
        public IActionResult SubmitForm([Required][FromBody] Submission submission)
        {
            TokenService.ValidationState vs;
            if ((vs = tokenService.ValidateRequestToken(Request.Headers.Authorization, out UserModel? user)) != TokenService.ValidationState.Valid)
                return Unauthorized(new { error = TokenService.GetStatus(vs) });
            if (!user.EmailVerified)
                return StatusCode(StatusCodes.Status403Forbidden, new { error = "Email is not verified." });
            if (user.Id != submission.UserId)
                return BadRequest(new { error = "User ID mismatch." });
            var form = formService.GetForm(submission.FormId);
            if (form == null)
                return BadRequest(new { error = "Form is not found." });
            if (form.FormElements.Count != submission.Submissions.Count)
                return BadRequest(new { error = "Submission elements mismatch." });

            for (int i = 0; i < form.FormElements.Count; i++)
            {
                var f = form.FormElements[i];
                var s = submission.Submissions[i];
                if (f.QuestionType != s.QuestionType)
                    return BadRequest(new { error = "Submission elements mismatch." });
            }
            var retSub = submissionService.Submit(submission);
            if (retSub == null)
                return BadRequest(new { error = "No submissions." });
            return Ok(new { submissionModelResponse = retSub });
        }
        [HttpDelete("submissions/deleteuser")]
        public IActionResult DeleteAllByUser(string uid, string fid)
        {
            TokenService.ValidationState vs;
            if ((vs = tokenService.ValidateRequestToken(Request.Headers.Authorization, out UserModel currentUser)) != TokenService.ValidationState.Valid)
                return Unauthorized(new { error = TokenService.GetStatus(vs) });

            if (!currentUser.EmailVerified)
                return StatusCode(StatusCodes.Status403Forbidden, new { error = "Email is not verified." });

            if (!Guid.TryParse(uid, out var userId))
                return BadRequest(new { error = "Invalid uid." });

            if (!Guid.TryParse(fid, out var formId))
                return BadRequest(new { error = "Invalid fid." });

            var form = formService.GetForm(formId);
            if (form == null)
                return NotFound(new { error = "Form not found." });

            if (form.OwnerId != currentUser.Id)
                return Unauthorized(new { error = "No permissions." });

            var user = userService.GetUser(userId);
            if (user == null)
                return NotFound(new { error = "User not found." });

            var submissions = submissionService.GetSubmissionsByForm(formId);
            if (submissions == null || submissions.Count == 0)
                return NotFound(new { error = "No submissions in the form." });

            var subByUser = submissions.Where(x => x.UserId == userId);
            if (!subByUser.Any())
                return NotFound(new { error = "No submissions from user." });
            int deleted = 0;
            foreach (var sub in subByUser)
                if (submissionService.DeleteSubmission(sub.Id))
                    deleted++;
            return Ok(new { stringResponse = deleted.ToString() });

        }
        [HttpDelete("submissions/delete")]
        public IActionResult DeleteSubmission(string id)
        {
            TokenService.ValidationState vs;
            if ((vs = tokenService.ValidateRequestToken(Request.Headers.Authorization, out UserModel user)) != TokenService.ValidationState.Valid)
                return Unauthorized(new { error = TokenService.GetStatus(vs) });

            if (!Guid.TryParse(id, out var submissionId))
                return BadRequest(new { error = "Invalid id." });

            if (!user.EmailVerified)
                return StatusCode(StatusCodes.Status403Forbidden, new { error = "Email is not verified." });

            var submission = submissionService.GetSubmission(submissionId);
            if (submission == null)
                return NotFound(new { error = "Submission not found." });

            var form = formService.GetForm(submission.FormId);
            if (form == null)
                return NotFound(new { error = "Form not found. (strange)" });

            if (form.OwnerId != user.Id)
                return Unauthorized(new { error = "No permissions." });

            bool deleted = submissionService.DeleteSubmission(submissionId);
            return Ok(new { stringResponse = submission.Id.ToString(), boolResponse = deleted });
        }
        [HttpGet("submissions/form")]
        public IActionResult GetSubmissionsByForm(string? formId)
        {
            TokenService.ValidationState vs;
            if ((vs = tokenService.ValidateRequestToken(Request.Headers.Authorization, out UserModel? user)) != TokenService.ValidationState.Valid)
                return Unauthorized(new { error = TokenService.GetStatus(vs) });
            if (!user.EmailVerified)
                return StatusCode(StatusCodes.Status403Forbidden, new { error = "Email is not verified." });
            if (!Guid.TryParse(formId, out Guid fid))
                return BadRequest(new { error = "Invalid form id." });
            var form = formService.GetForm(fid);
            if (form == null)
                return NotFound(new { error = "Not found." });
            if (form.OwnerId != user.Id)
                return Unauthorized(new { error = "No permissions." });
            var submissions = submissionService.GetSubmissionsByForm(form.Id);
            return Ok(new { submissionsModelResponse = submissions });
        }
        [HttpGet("submissions/id")]
        public IActionResult GetSubmission(string? subId)
        {
            TokenService.ValidationState vs;
            if ((vs = tokenService.ValidateRequestToken(Request.Headers.Authorization, out UserModel? user)) != TokenService.ValidationState.Valid)
                return Unauthorized(new { error = TokenService.GetStatus(vs) });
            if (!user.EmailVerified)
                return StatusCode(StatusCodes.Status403Forbidden, new { error = "Email is not verified." });
            if (!Guid.TryParse(subId, out Guid sid))
                return BadRequest(new { error = "Invalid sub id." });
            var submission = submissionService.GetSubmission(sid);
            var form = formService.GetForm(submission.FormId);
            if (form.OwnerId != user.Id)
                return Unauthorized(new { error = "No permissions." });
            if (submission == null)
                return NotFound(new { error = "Submission not found." });
            return Ok(new { submissionModelResponse = submission });
        }
        [HttpGet("submissions/user")]
        public IActionResult GetSubmissionsByUser(string? userId)
        {
            TokenService.ValidationState vs;
            if ((vs = tokenService.ValidateRequestToken(Request.Headers.Authorization, out UserModel? user)) != TokenService.ValidationState.Valid)
                return Unauthorized(new { error = TokenService.GetStatus(vs) });
            if (!user.EmailVerified)
                return StatusCode(StatusCodes.Status403Forbidden, new { error = "Email is not verified." });
            if (!Guid.TryParse(userId, out Guid uid))
                return BadRequest(new { error = "Invalid sub id." });
            var reqUser = userService.GetUser(uid);
            if (reqUser == null)
                return NotFound(new { error = "User not found." });
            if (!reqUser.AnonymousView)
                return Unauthorized(new { error = "No permissions." });
            var submissions = submissionService.GetSubmissionsByUser(uid);
            return Ok(new { submissionsFormModel = submissions });
        }
    }
}
