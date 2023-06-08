using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace FormCreator.Models
{
    public class ChangePasswordModel
    {
        [JsonPropertyName("oldPassword")]
        public string OldPassword { get; set; }
        [JsonPropertyName("newPassword")]
        public string NewPassword { get; set; }
    }
}
