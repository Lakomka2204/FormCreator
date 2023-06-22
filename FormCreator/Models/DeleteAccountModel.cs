using System.ComponentModel;
using System.Text.Json.Serialization;

namespace FormCreator.Models
{
    public class DeleteAccountClassModel
    {
        [JsonPropertyName("password")]
        [PasswordPropertyText(true)]
        public string Password { get; set; }
        [JsonPropertyName("emailId")]
        public Guid EmailId { get; set; }
        [JsonPropertyName("code")]
        public string Code { get; set; }
    }
}
