using System.Text.Json.Serialization;

namespace ClassLibraryModel
{
    public class DeleteAccountClassModel
    {
        [JsonPropertyName("password")]
        public string Password { get; set; }
    }
}
