using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace FormCreator.Models
{
    public class FormAlterModel
    {
        [JsonPropertyName("form")]
        public FormModel Form { get; set; }
        [JsonPropertyName("token")]
        public string Token { get; set; }
    }
}
