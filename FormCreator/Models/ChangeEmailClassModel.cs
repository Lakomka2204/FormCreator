using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace FormCreator.Models
{
    public class ChangeEmailClassModel
    {
        [JsonPropertyName("code")]
        public string Code { get; set; }
        [JsonPropertyName("password")]
        public string Password { get; set; }
        [EmailAddress]
        [JsonPropertyName("newEmail")]
        public string NewEmail { get; set; }
        [JsonPropertyName("emailId")]
        public Guid EmailId { get; set; }
    }
}
