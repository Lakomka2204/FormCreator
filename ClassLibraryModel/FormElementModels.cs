using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ClassLibraryModel
{
    public enum QuestionType
    {
        None,
        ShortText,
        LongText,
        SingleOption,
        MultipleOptions,
        Date,
        Time
    }

    [JsonConverter(typeof(GeneralFormElementConverter))]
    public class GeneralFormElementModel
    {
        [Required]
        [Display(Name = "Question", Prompt = "Enter your question")]
        [BsonElement("question")]
        [JsonPropertyName("question")]
        public string Question { get; set; } = string.Empty;
        [Required]
        [BsonElement("question_type")]
        [JsonPropertyName("questionType")]
        public QuestionType QuestionType { get; set; } = QuestionType.None;
        [Required]
        [JsonPropertyName("index")]
        [BsonElement("index")]
        public int Index { get; set; }
        [JsonPropertyName("options")]
        [BsonElement("options")]
        public List<string>? Options { get; set; }
        [JsonPropertyName("multiChoice")]
        [BsonElement("multi")]
        public bool MultiChoice { get; set; }
        [JsonPropertyName("answer")]
        [BsonElement("answer")]
        public object? Answer { get; set; }
    }
    public class GeneralFormElementConverter : JsonConverter<GeneralFormElementModel>
    {
        public override GeneralFormElementModel Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException();
            }

            using var jsonDoc = JsonDocument.ParseValue(ref reader);
            var jsonObject = jsonDoc.RootElement;

            var questionType = (QuestionType)jsonObject.GetProperty("questionType").GetInt32();

            object? answer;
            switch (questionType)
            {
                case QuestionType.None:
                case QuestionType.ShortText:
                case QuestionType.LongText:
                    answer = jsonObject.GetProperty("answer").GetString();
                    break;
                case QuestionType.SingleOption:
                    answer = jsonObject.GetProperty("answer").GetInt32();
                    break;
                case QuestionType.MultipleOptions:
                    answer = JsonSerializer.Deserialize<List<int>>(jsonObject.GetProperty("answer").GetRawText(), options);
                    break;
                case QuestionType.Date:
                    answer = DateTime.Parse(jsonObject.GetProperty("answer").GetString()).ToString("yyyy-MM-dd");
                    break;
                case QuestionType.Time:
                    answer = TimeSpan.Parse(jsonObject.GetProperty("answer").GetString()).ToString("hh\\:mm\\:ss");
                    break;
                default:
                    throw new JsonException($"Unsupported question type: {questionType}");
            }
            var topg = new GeneralFormElementModel();
            topg.Question = jsonObject.GetProperty("question").GetString()!;
            topg.MultiChoice = jsonObject.GetProperty("multiChoice").GetBoolean();
            topg.Index = jsonObject.GetProperty("index").GetInt32();
            topg.QuestionType = questionType;
            topg.Options = JsonSerializer.Deserialize<List<string>>(jsonObject.GetProperty("options").GetRawText(), options);
            topg.Answer = answer;
            return topg;
        }

        public override void Write(Utf8JsonWriter writer, GeneralFormElementModel value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WriteString("question", value.Question);
            writer.WriteNumber("questionType", (int)value.QuestionType);
            writer.WriteNumber("index", value.Index);
            writer.WritePropertyName("options");
            writer.WriteStartArray();
            if (value.Options != null)
                foreach (var o in value?.Options)
                    writer.WriteStringValue(o.ToString());
            writer.WriteEndArray();
            writer.WriteBoolean("multiChoice", value.MultiChoice);
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
