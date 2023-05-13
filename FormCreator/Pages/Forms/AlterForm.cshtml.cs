using ClassLibraryModel;
using FormCreator.Pages.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FormCreator.Pages.Forms
{
    [Authorize]
    public class AlterFormModel : UserAPIModel
    {
        private readonly IHttpClientFactory httpClientFactory;

        public AlterFormModel(IHttpClientFactory httpClientFactory, IJWT jwt)
            : base(jwt)
        {
            this.httpClientFactory = httpClientFactory;
        }
        [BindProperty]
        public FormModel Form { get; set; }
        public IActionResult OnGetAdd(string? type, string? id)
        {

            if (string.IsNullOrEmpty(type))
            {
                // Handle the case when the parameter is null or empty
                return BadRequest("Invalid parameter");
            }
            var jsonForm = TempData["Form"] as string;
            var form = JsonSerializer.Deserialize<FormModel>(jsonForm);
            switch (type)
            {
                case nameof(ShortTextFormElementModel):
                    form?.FormElements?.Add(new ShortTextFormElementModel()
                    {
                        Question = "What do you want to ask?",
                        Answer = "Write the correct answer here",
                        Index = form.FormElements.Count,
                        QuestionType = QuestionType.ShortText,
                    });
                    break;
                case nameof(LongTextFormElementModel):
                    form?.FormElements?.Add(new LongTextFormElementModel()
                    {
                        Question = "Ask people about something that long that they couldn't fit in one paragraph",
                        Answer = "I don't think there should be a correct answer, but if you want, you're good to go",
                        Index = form.FormElements.Count,
                        QuestionType = QuestionType.LongText,
                    });
                    break;
                case nameof(TimeFormElementModel):
                    form?.FormElements?.Add(new TimeFormElementModel()
                    {
                        Question = "What's the time?",
                        Time = DateTime.UtcNow.TimeOfDay,
                        Index = form.FormElements.Count,
                        QuestionType = QuestionType.Time,
                    });
                    break;
                case nameof(DateFormElementModel):
                    form?.FormElements?.Add(new DateFormElementModel()
                    {
                        Question = "What's the funny date?",
                        Date = new DateTime(2001, 9, 11),
                        Index = form.FormElements.Count,
                        QuestionType = QuestionType.Date,
                    });
                    break;
                case nameof(SingleOptionFormElementModel):
                    form?.FormElements?.Add(new SingleOptionFormElementModel()
                    {
                        Question = "What's your favourite color?",
                        Options = new List<string>()
                        {
                            "Red",
                            "Green",
                            "Blue"
                        },
                        CorrectAnswer = 1,
                        Index = form.FormElements.Count,
                        QuestionType = QuestionType.SingleOption,
                    });
                    break;
                case nameof(MultipleOptionsFormElementModel):
                    form?.FormElements?.Add(new MultipleOptionsFormElementModel()
                    {
                        Question = "Do you agree to the following?",
                        Options = new List<string>()
                        {
                            "Enter your option",
                        },
                        Index = form.FormElements.Count,
                        CorrectAnswers = new List<bool>() { true, true, false },
                        QuestionType = QuestionType.MultipleOptions,
                    });
                    break;
            }
            Form = form;
            TempData["Form"] = JsonSerializer.Serialize(form);
            return RedirectToPage("AlterForm", "", new { id, reset=false });
        }
        public IActionResult OnGetAddMultiple(int index, string id)
        {
            var jsonForm = TempData["Form"] as string;
            var form = JsonSerializer.Deserialize<FormModel>(jsonForm);
            if (index < 0 || index >= jsonForm.Length)
                return RedirectToPage("AlterForm", "", new { id, reset=false });
            if (form.FormElements[index] is not MultipleOptionsFormElementModel)
                return RedirectToPage("AlterForm", "", new { id, reset=false });
            var multiple = form.FormElements[index] as MultipleOptionsFormElementModel;
            multiple.Options.Add("New item");
            multiple.CorrectAnswers.Add(false);
            form.FormElements[index] = multiple;
            Form = form;
            TempData["Form"] = JsonSerializer.Serialize(form);
            return RedirectToPage("AlterForm", "", new { id, reset=false });
        }
        public IActionResult OnGetAddSingle(int index, string id)
        {

            var jsonForm = TempData["Form"] as string;
            var form = JsonSerializer.Deserialize<FormModel>(jsonForm);
            if (index < 0 || index >= jsonForm.Length)
            {
                TempData["UserError"] = "Out of range.";
                return RedirectToPage("AlterForm", "", new { id, reset=false });
            }
            if (form.FormElements[index] is not SingleOptionFormElementModel)
            {
                TempData["UserError"] = "Type mismatch.";
                return RedirectToPage("AlterForm", "", new { id, reset=false });
            }
            var multiple = form.FormElements[index] as SingleOptionFormElementModel;
            multiple.Options.Add("New item");
            form.FormElements[index] = multiple;
            Form = form;
            TempData["Form"] = JsonSerializer.Serialize(form);
            return RedirectToPage("AlterForm", "", new { id, reset=false });
        }
        public IActionResult OnGetRemoveSingle(string id, int oindex, int iindex)
        {
            var jsonForm = TempData["Form"] as string;
            var form = JsonSerializer.Deserialize<FormModel>(jsonForm);
            if (oindex < 0 || oindex >= jsonForm.Length)
            {
                TempData["UserError"] = "Out of outer range.";
                return RedirectToPage("AlterForm", "", new { id, reset=false });
            }
            if (form.FormElements[oindex] is not SingleOptionFormElementModel)
            {
                TempData["UserError"] = "Type mismatch.";
                return RedirectToPage("AlterForm", "", new { id, reset=false });
            }
            var multiple = form.FormElements[oindex] as SingleOptionFormElementModel;
            if (iindex < 0 || iindex >= multiple.Options.Count)
            {
                TempData["UserError"] = "Out of inner range.";
                return RedirectToPage("AlterForm", "", new { id, reset=false });
            }
            if (multiple.Options.Count <= 2)
            {
                TempData["UserError"] = "There must be at least two options.";
                return RedirectToPage("AlterForm", "", new { id, reset=false });
            }
            multiple.Options.RemoveAt(iindex);
            form.FormElements[oindex] = multiple;
            Form = form;
            TempData["Form"] = JsonSerializer.Serialize(form);
            return RedirectToPage("AlterForm", "", new { id, reset=false });
        }
        public IActionResult OnGetRemoveMultiple(string id, int oindex, int iindex)
        {
            var jsonForm = TempData["Form"] as string;
            var form = JsonSerializer.Deserialize<FormModel>(jsonForm);
            if (oindex < 0 || oindex >= jsonForm.Length)
            {
                TempData["UserError"] = "Out of outer range.";
                return RedirectToPage("AlterForm", "", new { id, reset=false });
            }
            if (form.FormElements[oindex] is not MultipleOptionsFormElementModel)
            {
                TempData["UserError"] = "Type mismatch.";
                return RedirectToPage("AlterForm", "", new { id, reset=false });
            }
            var multiple = form.FormElements[oindex] as MultipleOptionsFormElementModel;
            if (iindex < 0 || iindex >= multiple.Options.Count)
            {
                TempData["UserError"] = "Out of inner range.";
                return RedirectToPage("AlterForm", "", new { id, reset=false });
            }
            if (multiple.Options.Count <= 2)
            {
                TempData["UserError"] = "There must be at least two options.";
                return RedirectToPage("AlterForm", "", new { id, reset=false });
            }
            multiple.Options.RemoveAt(iindex);
            form.FormElements[oindex] = multiple;
            Form = form;
            TempData["Form"] = JsonSerializer.Serialize(form);
            return RedirectToPage("AlterForm", "", new { id, reset=false });
        }
        public async Task<IActionResult> OnPostAsync()
        {

            if (!ModelState.IsValid)
            {
                string errorkeys = "";
                foreach (string key in ModelState.Keys)
                    if (ModelState[key].ValidationState != Microsoft.AspNetCore.Mvc.ModelBinding.ModelValidationState.Valid)
                        errorkeys += string.Join('\n', ModelState[key].Errors.Select(x => x.ErrorMessage));
                TempData["UserError"] = errorkeys;
                return Page();
            }
            using var client = httpClientFactory.CreateClient("FCApiClient");
            string endpoint = "api/forms/edit";
            if (Form.FormElements == null)
                Form.FormElements = new List<BaseFormElementModel>(0);
            if (Form.Description == null)
                Form.Description = string.Empty;
            FormAlterModel fm = new FormAlterModel
            {
                Form = Form,
                Token = Request.Cookies["jwt"]
            };
            var json = JsonSerializer.Serialize(fm);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await client.PutAsync(endpoint, content);
            var resString = await response.Content.ReadAsStringAsync();
            var res = JsonSerializer.Deserialize<ServerResponse>(resString);
            if (!response.IsSuccessStatusCode)
            {
                TempData["UserError"] = res.error;
                return Page();
            }
            TempData["UserSuccess"] = res.boolResponse ?? false ? $"Form was updated." : "Form remained the same";
            return Page();
        }
        public async Task<IActionResult> OnGetAsync(string? id, bool? reset)
        {
            if (TempData["Form"] != null && (!reset ?? false))
            {
                Form = JsonSerializer.Deserialize<FormModel>(TempData["Form"].ToString());
                TempData["Form"] = JsonSerializer.Serialize(Form);
                return Page();
            }
            if (!Guid.TryParse(id, out _))
            {
                TempData["UserError"] = "Invalid ID.";
                return Page();
            }
            QueryBuilder qb = new QueryBuilder()
            {
                {"token",Request.Cookies["jwt"] },
                {"fid",id }
            };
            string endpoint = "api/forms/form" + qb.ToQueryString();
            using var client = httpClientFactory.CreateClient("FCApiClient");
            var response = await client.GetAsync(endpoint);
            var resString = await response.Content.ReadAsStringAsync();
            var res = JsonSerializer.Deserialize<ServerResponse>(resString);
            if (!response.IsSuccessStatusCode)
            {
                TempData["UserError"] = res.error;
                return Page();
            }
            if (res?.formModelResponse?.OwnerId != GetCookieUser()?.Id)
            {
                TempData["UserError"] = "No permission.";
                return Page();
            }
            Form = res.formModelResponse;
            TempData["Form"] = JsonSerializer.Serialize(Form);
            return Page();
        }
    }
}
