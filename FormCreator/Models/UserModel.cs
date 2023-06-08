using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace FormCreator.Models
{
    public class UserModel
    {
        [BsonId]
        [BsonGuidRepresentation(MongoDB.Bson.GuidRepresentation.CSharpLegacy)]
        [BsonElement("id")]
        [JsonPropertyName("id")]
        [Required]
        public Guid Id { get; set; }
        [BsonElement("username")]
        [JsonPropertyName("username")]
        public string Username { get; set; } = string.Empty;
        [EmailAddress(ErrorMessage = "Invalid Email")]
        [JsonPropertyName("email")]
        [BsonElement("email")]
        [Required]
        public string Email { get; set; } = string.Empty;
        [JsonPropertyName("emailVerified")]
        [BsonElement("email_verified")]
        [Required]
        public bool EmailVerified { get; set; }
        [JsonPropertyName("password")]
        [BsonElement("password")]
        [Required]
        public string Password { get; set; } = string.Empty;
        [JsonPropertyName("formsAvailable")]
        [BsonElement("forms_available")]
        [Required]
        public int FormsAvailable { get; set; }
        [JsonPropertyName("createdAt")]
        [BsonElement("created_at")]
        [BsonDateTimeOptions(DateOnly = false, Kind = DateTimeKind.Utc, Representation = MongoDB.Bson.BsonType.DateTime)]
        [Required]
        public DateTime CreatedAt { get; set; }
        [Required]
        [JsonPropertyName("anonymousView")]
        [BsonElement("anon_view")]
        public bool AnonymousView { get; set; }
        [Required]
        [JsonPropertyName("accountState")]
        [BsonElement("account_state")]
        public AccountState AccountState { get; set; }
        [Required]
        [JsonPropertyName("deletionDate")]
        [BsonElement("deletion_date")]
        [BsonDateTimeOptions(DateOnly = false, Kind = DateTimeKind.Utc, Representation = MongoDB.Bson.BsonType.DateTime)]
        public DateTime DeletionDate { get; set; }
        [Required]
        [JsonPropertyName("lastPasswordChangeTime")]
        [BsonElement("last_pass_time")]
        [BsonDateTimeOptions(DateOnly = false, Kind = DateTimeKind.Utc, Representation = MongoDB.Bson.BsonType.DateTime)]
        public DateTime LastPasswordChangeTime { get; set; }
    }
    public enum AccountState { Active, Banned, PendingDeletion, Deleted }
}
