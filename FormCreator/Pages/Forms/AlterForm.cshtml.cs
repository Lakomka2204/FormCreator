using ClassLibraryModel;
using FormCreator.Pages.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.ComponentModel.DataAnnotations;
using System.Net.Http.Headers;
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
        public FormModelV2 Form { get; set; }
        public IActionResult OnGetAdd(string? type, string id)
        {

            if (string.IsNullOrEmpty(type))
            {
                // Handle the case when the parameter is null or empty
                return BadRequest("Invalid parameter");
            }
            var jsonForm = TempData["Form"] as string;
            var form = JsonSerializer.Deserialize<FormModelV2>(jsonForm);
            QuestionType qType = Enum.Parse<QuestionType>(type);
            switch (qType)
            {
                case QuestionType.ShortText:
                    form?.FormElements?.Add(
                        new GeneralFormElementModel()
                        {
                            Question = "What do you want to ask?",
                            Answer = "Write the correct answer here",
                            Index = form.FormElements.Count,
                            QuestionType = QuestionType.ShortText,
                        });
                    break;
                case QuestionType.LongText:
                    form?.FormElements?.Add(
                        new GeneralFormElementModel()
                        {
                            Question = "Ask people about something that long that they couldn't fit in one paragraph",
                            Answer = "I don't think there should be a correct answer, but if you want, you're good to go",
                            Index = form.FormElements.Count,
                            QuestionType = QuestionType.LongText,
                        });
                    break;
                case QuestionType.Time:
                    form?.FormElements?.Add(
                        new GeneralFormElementModel()
                        {
                            Question = "What's the time?",
                            Answer = DateTime.UtcNow.TimeOfDay.ToString("hh\\:mm\\:ss"),
                            Index = form.FormElements.Count,
                            QuestionType = QuestionType.Time,
                        });
                    break;
                case QuestionType.Date:
                    form?.FormElements?.Add(
                        new GeneralFormElementModel()
                        {
                            Question = "What's the funny date?",
                            Answer = new DateTime(2001, 9, 11),
                            Index = form.FormElements.Count,
                            QuestionType = QuestionType.Date,
                        });
                    break;
                case QuestionType.SingleOption:
                    form?.FormElements?.Add(
                        new GeneralFormElementModel()
                        {
                            Question = "What's your favourite color?",
                            Options = new List<string>()
                            {
                                "red", "green","blue"
                            },
                            MultiChoice = false,
                            Answer = 1,
                            Index = form.FormElements.Count,
                            QuestionType = QuestionType.SingleOption,
                        });
                    break;
                case QuestionType.MultipleOptions:
                    form?.FormElements?.Add(
                        new GeneralFormElementModel()
                        {
                            Question = "Do you agree to the following?",
                            Options = new List<string>()
                            {
                                "Enter your options",
                                "You can have lots of them",
                            },
                            Answer = new List<int>(0),
                            MultiChoice = true,
                            Index = form.FormElements.Count,
                            QuestionType = QuestionType.MultipleOptions,
                        });
                    break;
            }
            Form = form;
            TempData["Form"] = JsonSerializer.Serialize(form);
            return RedirectToPage("AlterForm", "", new { id, r = false });
        }
        public IActionResult OnGetRemove(string id, int index)
        {
            var jsonForm = TempData["Form"] as string;
            var form = JsonSerializer.Deserialize<FormModelV2>(jsonForm);
            if (index < 0 || index >= jsonForm.Length)
                return RedirectToPage("AlterForm", "", new { id, r = false });
            form?.FormElements?.RemoveAt(index);
            RecalculateIndexes(form);
            Form = form;
            TempData["Form"] = JsonSerializer.Serialize(form);
            return RedirectToPage("AlterForm", "", new { id, r = false });
        }
        public void RecalculateIndexes(FormModelV2 form)
        {
            foreach (var fe in form?.FormElements)
                fe.Index = form.FormElements.IndexOf(fe);
        }
        public IActionResult OnGetMoveUp(string id, int index)
        {
            var jsonForm = TempData["Form"] as string;
            var form = JsonSerializer.Deserialize<FormModelV2>(jsonForm);
            if (index <= 0 || index > jsonForm.Length)
                return RedirectToPage("AlterForm", "", new { id, r = false });
            var element = form.FormElements[index];
            var insertingElement = form.FormElements[index];
            var elementToReplace = form.FormElements[index - 1];
            form.FormElements[index - 1] = insertingElement;
            form.FormElements[index] = elementToReplace;
            RecalculateIndexes(form);
            Form = form;
            TempData["Form"] = JsonSerializer.Serialize(form);
            return RedirectToPage("AlterForm", "", new { id, r = false });
        }
        public IActionResult OnGetMoveDown(string id, int index)
        {
            var jsonForm = TempData["Form"] as string;
            var form = JsonSerializer.Deserialize<FormModelV2>(jsonForm);
            if (index < 0 || index >= jsonForm.Length)
                return RedirectToPage("AlterForm", "", new { id, r = false });
            var insertingElement = form.FormElements[index];
            var elementToReplace = form.FormElements[index + 1];
            form.FormElements[index + 1] = insertingElement;
            form.FormElements[index] = elementToReplace;
            //form.FormElements.Insert(index + 1, insertingElement);
            //form.FormElements.RemoveAt(index);
            RecalculateIndexes(form);
            Form = form;
            TempData["Form"] = JsonSerializer.Serialize(form);
            return RedirectToPage("AlterForm", "", new { id, r = false });
        }
        public IActionResult OnGetAddMultiple(string id, int index)
        {
            var jsonForm = TempData["Form"] as string;
            var form = JsonSerializer.Deserialize<FormModelV2>(jsonForm);
            if (index < 0 || index >= jsonForm.Length)
                return RedirectToPage("AlterForm", "", new { id, r = false });
            if (form.FormElements[index].QuestionType != QuestionType.MultipleOptions)
            {
                TempData["UserError"] = "Type mismatch.";
                return RedirectToPage("AlterForm", "", new { id, r = false });
            }
            var multiple = form.FormElements[index];
            multiple.Options.Add("New item");
            form.FormElements[index] = multiple;
            Form = form;
            TempData["Form"] = JsonSerializer.Serialize(form);
            return RedirectToPage("AlterForm", "", new { id, r = false });
        }
        public IActionResult OnGetAddSingle(int index, string id)
        {

            var jsonForm = TempData["Form"] as string;
            var form = JsonSerializer.Deserialize<FormModelV2>(jsonForm);
            if (index < 0 || index >= jsonForm.Length)
            {
                TempData["UserError"] = "Out of range.";
                return RedirectToPage("AlterForm", "", new { id, r = false });
            }
            if (form.FormElements[index].QuestionType != QuestionType.SingleOption)
            {
                TempData["UserError"] = "Type mismatch.";
                return RedirectToPage("AlterForm", "", new { id, r = false });
            }
            var multiple = form.FormElements[index];
            multiple.Options.Add("New item");
            form.FormElements[index] = multiple;
            Form = form;
            TempData["Form"] = JsonSerializer.Serialize(form);
            return RedirectToPage("AlterForm", "", new { id, r = false });
        }
        public IActionResult OnGetRemoveSingle(string id, int oindex, int iindex)
        {
            var jsonForm = TempData["Form"] as string;
            var form = JsonSerializer.Deserialize<FormModelV2>(jsonForm);
            if (oindex < 0 || oindex >= jsonForm.Length)
            {
                TempData["UserError"] = "Out of outer range.";
                return RedirectToPage("AlterForm", "", new { id, r = false });
            }
            if (form.FormElements[oindex].QuestionType != QuestionType.SingleOption)
            {
                TempData["UserError"] = "Type mismatch.";
                return RedirectToPage("AlterForm", "", new { id, r = false });
            }
            var multiple = form.FormElements[oindex];
            if (iindex < 0 || iindex >= multiple.Options.Count)
            {
                TempData["UserError"] = "Out of inner range.";
                return RedirectToPage("AlterForm", "", new { id, r = false });
            }
            if (multiple.Options.Count <= 2)
            {
                TempData["UserError"] = "There must be at least two options.";
                return RedirectToPage("AlterForm", "", new { id, r = false });
            }
            multiple.Options.RemoveAt(iindex);
            form.FormElements[oindex] = multiple;
            Form = form;
            TempData["Form"] = JsonSerializer.Serialize(form);
            return RedirectToPage("AlterForm", "", new { id, r = false });
        }
        public IActionResult OnGetRemoveMultiple(string id, int oindex, int iindex)
        {
            var jsonForm = TempData["Form"] as string;
            var form = JsonSerializer.Deserialize<FormModelV2>(jsonForm);
            if (oindex < 0 || oindex >= jsonForm.Length)
            {
                TempData["UserError"] = "Out of outer range.";
                return RedirectToPage("AlterForm", "", new { id, r = false });
            }
            if (form.FormElements[oindex].QuestionType != QuestionType.MultipleOptions)
            {
                TempData["UserError"] = "Type mismatch.";
                return RedirectToPage("AlterForm", "", new { id, r = false });
            }
            var multiple = form.FormElements[oindex];
            if (iindex < 0 || iindex >= multiple.Options.Count)
            {
                TempData["UserError"] = "Out of inner range.";
                return RedirectToPage("AlterForm", "", new { id, r = false });
            }
            if (multiple.Options.Count <= 2)
            {
                TempData["UserError"] = "There must be at least two options.";
                return RedirectToPage("AlterForm", "", new { id, r = false });
            }
            multiple.Options.RemoveAt(iindex);
            form.FormElements[oindex] = multiple;
            Form = form;
            TempData["Form"] = JsonSerializer.Serialize(form);
            return RedirectToPage("AlterForm", "", new { id, r = false });
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
            string endpoint = "api/v1/forms/edit";
            string token = Request.Cookies["jwt"];
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            if (Form.FormElements == null)
                Form.FormElements = new List<GeneralFormElementModel>(0);
            if (Form.Description == null)
                Form.Description = string.Empty;
            for (int i = 0; i < Form.FormElements.Count; i++)
            {
                if (Form.FormElements[i].Answer != null) continue;
                var actualValue = Request.Form.FirstOrDefault(x => x.Key.Contains($"[{i}].Answer"));
                switch (Form.FormElements[i].QuestionType)
                {
                    case QuestionType.None:
                        throw new Exception("crazy fucker how did you do questiontype 0, go back monke to your cave");
                    case QuestionType.ShortText:
                    case QuestionType.LongText:
                        Form.FormElements[i].Answer = actualValue.Value[0];
                        break;
                    case QuestionType.Time:
                        Form.FormElements[i].Answer = TimeSpan.Parse(actualValue.Value[0] ?? "00:00:00").ToString("hh\\:mm\\:ss");
                        break;
                    case QuestionType.Date:
                        Form.FormElements[i].Answer = DateTime.Parse(actualValue.Value[0] ?? DateTime.MinValue.ToString()).ToString("yyyy-MM-dd");
                        break;
                    case QuestionType.SingleOption:
                        Form.FormElements[i].Answer = int.Parse(actualValue.Value[0] ?? "0");
                        break;
                    case QuestionType.MultipleOptions:
                        Form.FormElements[i].Answer = actualValue.Value.Select(int.Parse).ToList();
                        break;
                }
            }

            var json = JsonSerializer.Serialize(Form);
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
            TempData.Remove("Form");
            return RedirectToPage("AlterForm", "", new { id = Form.Id, r = true });
        }
        public async Task<IActionResult> OnGetAsync(string? id, bool? r)
        {
            if (TempData["Form"] != null && (!r ?? false))
            {
                Form = JsonSerializer.Deserialize<FormModelV2>(TempData["Form"].ToString());
                TempData["Form"] = JsonSerializer.Serialize(Form);
                return Page();
            }
            if (!Guid.TryParse(id, out _))
            {
                TempData["UserError"] = "Invalid ID.";
                return Page();
            }

            string endpoint = $"api/v1/forms/form?id={id}";
            using var client = httpClientFactory.CreateClient("FCApiClient");
            string token = Request.Cookies["jwt"];
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
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
            Form.FormElements = Form?.FormElements?.Where(x => x.QuestionType != QuestionType.None).ToList();
            TempData["Form"] = JsonSerializer.Serialize(Form);
            return Page();
        }
    }
}
