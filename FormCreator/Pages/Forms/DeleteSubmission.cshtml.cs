using FormCreator.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Principal;
using System.Text;
using System.Text.Json;

namespace FormCreator.Pages.Forms
{
    public class DeleteSubmissionModel : PageModel
    {
        private readonly IHttpClientFactory httpClientFactory;

        public DeleteSubmissionModel(IHttpClientFactory httpClientFactory)
        {
            this.httpClientFactory = httpClientFactory;
        }
        public enum DeletionType { Single, User }
        public async Task<IActionResult> OnGetAsync(string? type, string? id, string? fid)
        {
            if (type == null ||
                id == null ||
                !Enum.TryParse(type, true, out DeletionType dType) ||
                !Guid.TryParse(id, out Guid subId))
                return Redirect(Request.Headers.Referer[0]);

            using var client = httpClientFactory.CreateClient("FCApiClient");
            string token = HttpContext.Request.Cookies["jwt"];
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            StringBuilder sb = new("/api/v1/forms/submissions");
            HttpResponseMessage response = null;
            ServerResponse res = null;
            switch (dType)
            {
                case DeletionType.Single:
                    sb.Append("/delete?id=")
                        .Append(subId);
                    response = await client.DeleteAsync(sb.ToString());
                    res = JsonSerializer.Deserialize<ServerResponse>(await response.Content.ReadAsStringAsync());
                    if (response.IsSuccessStatusCode)
                        TempData["UserSuccess"] = $"Submission {res.stringResponse} has{(res.boolResponse ?? true ? "" : " not")} been deleted";
                    break;
                case DeletionType.User:
                    if (fid == null || !Guid.TryParse(fid, out Guid formId))
                        return Redirect(Request.Headers.Referer[0]);
                    sb.Append("/deleteuser?uid=")
                        .Append(subId)
                        .Append("&fid=")
                        .Append(formId);
                    response = await client.DeleteAsync(sb.ToString());
                    res = JsonSerializer.Deserialize<ServerResponse>(await response.Content.ReadAsStringAsync());
                    if (response.IsSuccessStatusCode)
                        TempData["UserSuccess"] = $"Deleted {res.stringResponse} submissions";
                    break;
            }
            if (!response.IsSuccessStatusCode)
                TempData["UserError"] = res.error;
            return Redirect(Request.Headers.Referer[0]);
        }
    }
}
