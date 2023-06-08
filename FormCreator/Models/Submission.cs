using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace FormCreator.Models
{
    public class Submission
    {
        [BsonId]
        [BsonGuidRepresentation(MongoDB.Bson.GuidRepresentation.CSharpLegacy)]
        [BsonElement("id")]
        [JsonPropertyName("id")]
        [Required]
        public Guid Id { get; set; }
        [BsonGuidRepresentation(MongoDB.Bson.GuidRepresentation.CSharpLegacy)]
        [BsonElement("user_id")]
        [JsonPropertyName("userId")]
        [Required]
        public Guid UserId { get; set; }
        [BsonGuidRepresentation(MongoDB.Bson.GuidRepresentation.CSharpLegacy)]
        [BsonElement("form_id")]
        [JsonPropertyName("formId")]
        [Required]
        public Guid FormId { get; set; }
        [BsonDateTimeOptions(DateOnly = false, Kind = DateTimeKind.Utc, Representation = MongoDB.Bson.BsonType.DateTime)]
        [BsonElement("submission_date")]
        [JsonPropertyName("submissionDate")]
        public DateTime SubmissionDate { get; set; }
        [Required]
        [BsonElement("submission")]
        [JsonPropertyName("submission")]
        public List<GeneralFormSubmissionModel> Submissions { get; set; } = new List<GeneralFormSubmissionModel>(0);
    }
    [JsonConverter(typeof(GeneralFormSubmissionConverter))]
    public class GeneralFormSubmissionModel
    {
        [Required]
        [BsonElement("question_type")]
        [JsonPropertyName("questionType")]
        public QuestionType QuestionType { get; set; } = QuestionType.None;
        [Required]
        [JsonPropertyName("index")]
        [BsonElement("index")]
        public int Index { get; set; }
        [JsonPropertyName("answer")]
        [BsonElement("answer")]
        public object? Answer { get; set; }
    }
    public class GeneralFormSubmissionConverter : JsonConverter<GeneralFormSubmissionModel>
    {
        public override GeneralFormSubmissionModel Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException();
            }

            using var jsonDoc = JsonDocument.ParseValue(ref reader);
            var jsonObject = jsonDoc.RootElement;

            var questionType = (QuestionType)jsonObject.GetProperty("questionType").GetInt32();
            object? answer = questionType switch
            {
                QuestionType.None or QuestionType.ShortText or QuestionType.LongText => jsonObject.GetProperty("answer").GetString(),
                QuestionType.SingleOption => jsonObject.GetProperty("answer").GetInt32(),
                QuestionType.MultipleOptions => JsonSerializer.Deserialize<List<int>>(jsonObject.GetProperty("answer").GetRawText(), options),
                QuestionType.Date => DateTime.Parse(jsonObject.GetProperty("answer").GetString()).ToString("yyyy-MM-dd"),
                QuestionType.Time => TimeSpan.Parse(jsonObject.GetProperty("answer").GetString()).ToString("hh\\:mm\\:ss"),
                _ => throw new JsonException($"Unsupported question type: {questionType}"),
            };
            var bottomg = new GeneralFormSubmissionModel();
            bottomg.Index = jsonObject.GetProperty("index").GetInt32();
            bottomg.QuestionType = questionType;
            bottomg.Answer = answer;
            return bottomg;
        }

        public override void Write(Utf8JsonWriter writer, GeneralFormSubmissionModel value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WriteNumber("questionType", (int)value.QuestionType);
            writer.WriteNumber("index", value.Index);
            if (value.Answer == null)
                writer.WriteNull("answer");
            else
                switch (value.QuestionType)
                {
                    case QuestionType.None:
                        writer.WriteNull("answer");
                        break;
                    case QuestionType.ShortText:
                    case QuestionType.LongText:
                        writer.WriteString("answer", (string)value.Answer);
                        break;
                    case QuestionType.SingleOption:
                        writer.WriteNumber("answer", (int)value.Answer);
                        break;
                    case QuestionType.MultipleOptions:
                        writer.WriteStartArray("answer");
                        List<int> answers = (List<int>)value.Answer;
                        foreach (var a in answers)
                            writer.WriteNumberValue(a);
                        writer.WriteEndArray();
                        break;
                    case QuestionType.Date:
                        writer.WriteString("answer", DateTime.Parse(value.Answer.ToString()).ToString("yyyy-MM-dd"));
                        break;
                    case QuestionType.Time:
                        writer.WriteString("answer", TimeSpan.Parse(value.Answer.ToString()).ToString("hh\\:mm\\:ss"));
                        break;
                    default:
                        throw new JsonException($"Unsupported question type: {value.QuestionType}");
                }

            writer.WriteEndObject();
        }
    }

}
