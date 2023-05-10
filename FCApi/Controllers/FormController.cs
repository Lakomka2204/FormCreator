using ClassLibraryModel;
using FCApi.Models;
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
    public class FormController : FCBaseController
    {
        private readonly IFormService formService;
        public FormController(IUserService userService, IJWT jwt, IFormService formService)
            : base(userService, jwt)
        {
            this.formService = formService;
        }
        [HttpGet("{id}")]
        public IActionResult GetById([Required] Guid id)
        {
            if (id == null) return NotFound("Id not found");
            var form = formService.GetForm(id);
            if (form == null) return NotFound("Form not found");
            bool isOwner = false;
            var auth = CheckAuth(x => true, out _, out UserModel? um);
            if (auth == AuthStatus.Pass)
                isOwner = form?.OwnerId == um.Id;
            if (!isOwner)
                FormModel.RemovePrivateProperties(form);
            return Ok(form);
        }
        [HttpGet("my")]
        public IActionResult GetMyForms()
        {
            var authStatus = CheckAuth(x => true, out string? jwtError, out UserModel? user);
            switch (authStatus)
            {
                case AuthStatus.NoJwt:
                    return Unauthorized("No jwt token");
                case AuthStatus.ParseTokenFail:
                    return Unauthorized(jwtError);
            }
            if (user == null) return Unauthorized("this null check vs2022 sucks");
            return Ok(formService.GetFormsByUser(user.Id));
        }
        [HttpPost("create")]
        public IActionResult CreateForm([FromBody] [Required] FormModel model)
        {
            var authStatus = CheckAuth(x => true, out string? jwtError, out UserModel? user);
            switch (authStatus)
            {
                case AuthStatus.NoJwt:
                    return Unauthorized("No jwt token");
                case AuthStatus.ParseTokenFail:
                    return Unauthorized(jwtError);
            }
            if (user == null) return Unauthorized("this null check vs2022 sucks");
            if (model == null && model.FormElements.Any(x => x.QuestionType == QuestionType.None))
                return NotFound("Question type must not be 0");
            var uforms = formService.GetFormsByUser(user.Id).Count;
            if (uforms >= user.FormsAvailable)
                return Unauthorized($"Ran out of available forms ({user.FormsAvailable})");

            model.OwnerId = user.Id;
            var createdModel = formService.CreateForm(model);
            if (createdModel == null) return NotFound("No form questions");
            return CreatedAtAction(nameof(GetById), new { id = model?.Id },createdModel);
        }
        [HttpDelete("delete")]
        public IActionResult DeleteForm([Required] Guid formId)
        {
            var authStatus = CheckAuth(x => true, out string? jwtError, out UserModel? user);
            switch (authStatus)
            {
                case AuthStatus.NoJwt:
                    return Unauthorized("No jwt token");
                case AuthStatus.ParseTokenFail:
                    return Unauthorized(jwtError);
            }
            if (user == null) return Unauthorized("this null check vs2022 sucks");
            var form = formService.GetForm(formId);
            if (form == null) return NotFound($"Form with id ({formId}) not found.");
            if (form.OwnerId != user.Id) return Unauthorized("No permissions");
            formService.DeleteForm(formId);
            return Ok("Form deleted.");
        }
        [HttpGet("search")]
        public IActionResult SearchForm(string? q)
        {
            if (string.IsNullOrWhiteSpace(q)) return BadRequest("No query input");
            return Ok(formService.SearchForm(q));
        }
        [HttpPut("edit")]
        public IActionResult EditForm([Required][FromBody] FormModel newModel)
        {
            var authStatus = CheckAuth(x => true, out string? jwtError, out UserModel? user);
            switch (authStatus)
            {
                case AuthStatus.NoJwt:
                    return Unauthorized("No jwt token");
                case AuthStatus.ParseTokenFail:
                    return Unauthorized(jwtError);
            }
            if (user == null) return Unauthorized("this null check vs2022 sucks");
            var form = formService.GetForm(newModel.Id);
            if (form == null) return NotFound($"Form with id ({newModel.Id}) not found.");
            if (form.OwnerId != user.Id) return Unauthorized("No permissions");
            if (form == newModel) return BadRequest("Form hasn't changed (BK)");
            formService.EditForm(newModel);
            return Ok();
        }

    }
}
