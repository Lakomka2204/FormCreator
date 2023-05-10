using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace FCApi.Models
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
        [Required]
        [JsonPropertyName("ownerId")]
        [BsonElement("ownerid")]
        public Guid OwnerId { get; set; }
        [Required]
        [JsonPropertyName("canBeSearched")]
        [BsonElement("can_be_searched")]
        public bool CanBeSearched { get; set; }
        [Required]
        [JsonPropertyName("formElements")]
        [BsonElement("form_elements")]
        public List<BaseFormElementModel> FormElements { get; set; }
        public static void RemovePrivateProperties(FormModel? form)
        {
            if (form == null) return;

            foreach (var item in form.FormElements)
            {
                foreach (var pi in item.GetType().GetProperties())
                {
                    if (pi.GetMethod.IsAssembly)
                        pi.SetValue(form, null);
                }
            }
        }
    }
}
