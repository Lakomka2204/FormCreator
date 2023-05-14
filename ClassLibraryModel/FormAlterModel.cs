using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ClassLibraryModel
{
    public class FormAlterModel
    {
        [JsonPropertyName("form")]
        public FormModelV2 Form { get; set; }
        [JsonPropertyName("token")]
        public string Token { get; set; }
    }
}
