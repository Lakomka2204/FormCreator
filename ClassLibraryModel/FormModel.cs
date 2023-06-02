using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ClassLibraryModel
{
    public class FormModel
    {
        [Required]
        [BsonId]
        [JsonPropertyName("id")]
        public Guid Id { get; set; }
        [Required]
        [JsonPropertyName("name")]
        [BsonElement("name")]
        public string Name { get; set; }
        [JsonPropertyName("description")]
        [BsonElement("desc")]
        public string? Description { get; set; }
        [Required]
        [JsonPropertyName("ownerId")]
        [BsonElement("ownerid")]
        public Guid OwnerId { get; set; }
        [Required]
        [JsonPropertyName("canBeSearched")]
        [BsonElement("can_be_searched")]
        public bool CanBeSearched { get; set; }
        [JsonPropertyName("oneShotSubmission")]
        [BsonElement("oneshot_submission")]
        public bool CanBeSubmittedOnce { get; set; }
        [JsonPropertyName("formElements")]
        [BsonElement("form_elements")]
        public List<GeneralFormElementModel>? FormElements { get; set; }
        public static void RemovePrivateProperties(FormModel? form)
        {
            if (form == null) return;

            foreach (var item in form.FormElements)
            {
                //item.QuestionType = QuestionType.None;
                item.Answer = null;
            }
        }
    }
}
