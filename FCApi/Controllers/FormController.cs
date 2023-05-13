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
    [Route("api/forms")]
    [ApiController]
    public class FormController : ControllerBase
    {
        private readonly IUserService userService;
        private readonly IJWT jwt;
        private readonly IFormService formService;
        public FormController(IUserService userService, IJWT jwt, IFormService formService)
        {
            this.userService = userService;
            this.jwt = jwt;
            this.formService = formService;
        }
        [HttpGet("form")]
        public IActionResult GetById(string? token, string? fid)
        {
            if (string.IsNullOrWhiteSpace(token)) return BadRequest(new { error = "No token" });
            string id = jwt.DecryptTokenID(token);
            if (!Guid.TryParse(id, out Guid uid))
                return BadRequest(new { error = "Invalid token" });
            if (fid == null) return BadRequest(new { error = "No fid." });
            if (!Guid.TryParse(fid, out Guid ffid))
                return BadRequest(new { error = "Invalid fid." });
            var form = formService.GetForm(ffid);
            if (form == null) return NotFound(new { error = "Form not found." });
            var user = userService.GetUser(uid);
            bool isOwner = form?.OwnerId == user.Id;
            if (!isOwner)
                FormModel.RemovePrivateProperties(form);
            return Ok(new { formModelResponse = form });
        }
        [HttpGet("")]
        public IActionResult GetUserForms(string? uid)
        {
            if (uid == null) return BadRequest(new { error = "No uid." });
            if (!Guid.TryParse(uid, out Guid uuid))
                return BadRequest(new { error = "Invalid uid." });

            var forms = formService.GetFormsByUser(uuid);
            var user = userService.GetUser(uuid);
            foreach (var form in forms)
            {
                if (form?.OwnerId != user.Id)
                    FormModel.RemovePrivateProperties(form);
            }
            return Ok(new { formsModelResponse = forms });
        }
        [HttpGet("my")]
        public IActionResult GetMyForms(string? token)
        {
            if (string.IsNullOrWhiteSpace(token)) return BadRequest(new { error = "No token" });
            string id = jwt.DecryptTokenID(token);
            if (!Guid.TryParse(id, out Guid uid))
                return BadRequest(new { error = "Invalid token" });
            var forms = formService.GetFormsByUser(uid);
            return Ok(new { formsModelResponse = forms });
        }
        [HttpPost("create")]
        public IActionResult CreateForm([FromBody][Required] FormAlterModel model)
        {
            if (string.IsNullOrWhiteSpace(model.Token)) return BadRequest(new { error = "No token" });
            string id = jwt.DecryptTokenID(model.Token);
            if (!Guid.TryParse(id, out Guid uid))
                return BadRequest(new { error = "Invalid token" });
            var user = userService.GetUser(uid);
            if (user == null) return NotFound(new { error = "User not found." });

            if (model == null && model.Form.FormElements.Any(x => x.QuestionType == QuestionType.None))
                return NotFound(new { error = "Question type must not be 0" });
            var uforms = formService.GetFormsByUser(user.Id).Count;
            if (uforms >= user.FormsAvailable)
                return Unauthorized(new { error = $"Ran out of available forms." });
            model.Form.Id = Guid.NewGuid();
            model.Form.OwnerId = user.Id;
            var createdModel = formService.CreateForm(model.Form);
            if (createdModel == null) return NotFound("No form questions");
            return Ok(new { stringResponse = model?.Form?.Id });
        }
        [HttpDelete("delete")]
        public IActionResult DeleteForm(string? token, string? formId)
        {
            if (string.IsNullOrWhiteSpace(token)) return BadRequest(new { error = "No token" });
            string id = jwt.DecryptTokenID(token);
            if (!Guid.TryParse(id, out Guid uid))
                return BadRequest(new { error = "Invalid token" });
            if (string.IsNullOrWhiteSpace(token)) return BadRequest(new { error = "No formId" });
            if (!Guid.TryParse(formId, out Guid fid))
                return BadRequest(new { error = "Invalid form ID." });
            var user = userService.GetUser(uid);
            if (user == null) return NotFound(new { error = "User not found." });
            var form = formService.GetForm(fid);
            if (form == null) return NotFound(new { error = "Form not found." });

            if (form.OwnerId != user.Id) return Unauthorized(new { error = "No permissions" });
            formService.DeleteForm(fid);
            return Ok(new { boolResponse = true, stringResponse = form.Name });
        }
        [HttpDelete("deleteall")]
        public IActionResult DeleteAll(string? token)
        {
            if (string.IsNullOrWhiteSpace(token)) return BadRequest(new { error = "No token" });
            string id = jwt.DecryptTokenID(token);
            if (!Guid.TryParse(id, out Guid uid))
                return BadRequest(new { error = "Invalid token" });

            var user = userService.GetUser(uid);
            if (user == null) return NotFound(new { error = "User not found." });
            var forms = formService.GetFormsByUser(uid);
            int formsDeleted = 0;
            foreach(var form in forms)
            {
                if (formService.DeleteForm(form.Id))
                    formsDeleted++;
            }    
            return Ok(new { stringResponse = $"Deleted {formsDeleted} forms."});
        }
        [HttpGet("search")]
        public IActionResult SearchForm(string? q)
        {
            if (string.IsNullOrWhiteSpace(q)) return BadRequest("No query input");
            return Ok(formService.SearchForm(q));
        }
        [HttpPut("edit")]
        public IActionResult EditForm([Required][FromBody] FormAlterModel newModel)
        {
            if (string.IsNullOrWhiteSpace(newModel.Token)) return BadRequest(new { error = "No token" });
            string id = jwt.DecryptTokenID(newModel.Token);
            if (!Guid.TryParse(id, out Guid uid))
                return BadRequest(new { error = "Invalid token" });
            var user = userService.GetUser(uid);
            if (user == null) return Unauthorized(new { error = "User not found." });
            var form = formService.GetForm(newModel.Form.Id);
            if (form == null) return NotFound($"Form with id ({newModel.Form.Id}) not found.");
            if (form.OwnerId != user.Id) return Unauthorized("No permissions");
            newModel.Form.OwnerId = user.Id; // на всякий случай, а его можно менять
            if (form == newModel.Form) return BadRequest("Form hasn't changed (BK)");
            formService.EditForm(newModel.Form);
            return Ok(new { boolResponse = true });
        }

    }
}
