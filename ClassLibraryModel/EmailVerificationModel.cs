using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace FCApi.Models
{
    public class EmailVerificationModel
    {
        [BsonId]
        public Guid Id { get; set; }
        [Required]
        [BsonElement("userId")]
        [JsonPropertyName("userId")]
        public Guid UserId { get; set; }
        [Required]
        [BsonElement("code")]
        [JsonPropertyName("code")]
        public string Code { get; set; }
        [Required]
        [EmailAddress]
        [BsonElement("email")]
        [JsonPropertyName("email")]
        public string Email { get; set; }
        [Required]
        [BsonElement("send_time")]
        [JsonPropertyName("sendTime")]
        [BsonDateTimeOptions(DateOnly = false,Kind = DateTimeKind.Utc, Representation = MongoDB.Bson.BsonType.DateTime)]
        public DateTime SendTime { get; set; }
        [Required]
        [BsonElement("reason")]
        [JsonPropertyName("reason")]
        public string Reason { get; set; }
    }
    public class EmailVerificationRequestModel
    {
        [Required]
        public Guid UserId { get; set; }
        [Required]
        public string Code { get; set; }
    }
}
