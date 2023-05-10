using System.Text.Json.Serialization;

namespace ClassLibraryModel
{
    public class DeleteAccountClassModel
    {
        [JsonPropertyName("token")]
        public string Token { get; set; }
        [JsonPropertyName("password")]
        public string Password { get; set; }
    }
}
