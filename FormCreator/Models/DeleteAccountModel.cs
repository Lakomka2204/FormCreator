using System.Text.Json.Serialization;

namespace FormCreator.Models
{
    public class DeleteAccountClassModel
    {
        [JsonPropertyName("password")]
        public string Password { get; set; }
    }
}
