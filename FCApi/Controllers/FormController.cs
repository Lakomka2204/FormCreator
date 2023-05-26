using ClassLibraryModel;
using FCApi.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Xml.Linq;
using static FCApi.Controllers.AuthController;

namespace FCApi.Controllers
{
    [Route("api/v{version:apiVersion}/forms")]
    [ApiController]
    [ApiVersion("1")]
    public class FormController : ControllerBase
    {
        private readonly IFormService formService;
        private readonly ITokenService tokenService;

        public FormController(IFormService formService, ITokenService tokenService)
        {
            this.formService = formService;
            this.tokenService = tokenService;
        }
        [HttpGet("form")]
        [MapToApiVersion("1")]
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
                FormModelV2.RemovePrivateProperties(form);
            return Ok(new { formModelResponse = form });
        }
        [HttpGet("user")]
        public IActionResult GetUserForms(string? id)
        {
            if (id == null) return BadRequest(new { error = "No user id." });
            if (!Guid.TryParse(id, out Guid uid))
                return BadRequest(new { error = "Invalid user id." });
            tokenService.ValidateRequestToken(HttpContext.Request.Headers.Authorization, out UserModel? user);

            var forms = formService.GetFormsByUser(uid);
            for (int i = 0; i < forms.Count; i++)
            {
                if (user == null || forms[i].OwnerId != user.Id)
                {
                    if (forms[i].CanBeSearched)
                        FormModelV2.RemovePrivateProperties(forms[i]);
                    else
                        forms.RemoveAt(i);
                }
            }
            return Ok(new { formsModelResponse = forms });
        }
        [HttpPost("create")]
        public IActionResult CreateForm([FromBody][Required] FormModelV2 model)
        {
            TokenService.ValidationState vs;
            if ((vs = tokenService.ValidateRequestToken(Request.Headers.Authorization, out UserModel? user)) != TokenService.ValidationState.Valid)
                return Unauthorized(new { error = TokenService.GetStatus(vs) });

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
            if (!Guid.TryParse(id, out Guid fid))
                return BadRequest(new { error = "Invalid form ID." });

            var form = formService.GetForm(fid);
            if (form == null) return NotFound(new { error = "Form not found." });

            if (form.OwnerId != user.Id) return StatusCode(StatusCodes.Status403Forbidden, new { error = "No permissions" });
            formService.DeleteForm(fid);
            return Ok(new { boolResponse = true, stringResponse = form.Name });
        }
        [HttpDelete("deleteall")]
        public IActionResult DeleteAll()
        {
            TokenService.ValidationState vs;
            if ((vs = tokenService.ValidateRequestToken(Request.Headers.Authorization, out UserModel? user)) != TokenService.ValidationState.Valid)
                return Unauthorized(new { error = TokenService.GetStatus(vs) });
            if (user == null) return NotFound(new { error = "User not found." });
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
        public IActionResult EditForm([Required][FromBody] FormModelV2 newModel)
        {
            TokenService.ValidationState vs;
            if ((vs = tokenService.ValidateRequestToken(Request.Headers.Authorization, out UserModel? user)) != TokenService.ValidationState.Valid)
                return Unauthorized(new { error = TokenService.GetStatus(vs) });

            if (user == null) return Unauthorized(new { error = "User not found." });
            var form = formService.GetForm(newModel.Id);
            if (form == null) return NotFound(new { error = "Form not found." });
            if (form.OwnerId != user.Id) return Unauthorized(new { error = "No permissions" });
            newModel.OwnerId = user.Id;
            if (form == newModel) return BadRequest(new { error = "Form hasn't changed." });
            formService.EditForm(newModel);
            return Ok(new { boolResponse = true });
        }

    }
}
