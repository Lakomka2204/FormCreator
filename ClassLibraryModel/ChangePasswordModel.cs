using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ClassLibraryModel
{
    public class ChangePasswordModel
    {
        [JsonPropertyName("token")]
        public string Token { get; set; }
        [JsonPropertyName("oldPassword")]
        public string OldPassword { get; set; }
        [JsonPropertyName("newPassword")]
        public string NewPassword { get; set; }
    }
}
