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
    [BsonDiscriminator(RootClass = true)]
    [BsonNoId]
    [BsonKnownTypes(
    typeof(DateFormElementModel),
    typeof(ShortTextFormElementModel),
    typeof(LongTextFormElementModel),
    typeof(TimeFormElementModel),
    typeof(SingleOptionFormElementModel),
    typeof(MultipleOptionsFormElementModel))]
    [JsonConverter(typeof(FormElementConverter))]
    public class BaseFormElementModel
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
        public int Index { get; set; }
    }
    [BsonDiscriminator(nameof(DateFormElementModel))]
    public class DateFormElementModel : BaseFormElementModel
    {
        [Required]
        [BsonDateTimeOptions(DateOnly = true, Kind = DateTimeKind.Utc, Representation = BsonType.DateTime)]
        [BsonElement("date")]
        [JsonPropertyName("date")]
        [Display(Name = "Date", Prompt = "Choose date")]
        public DateTime Date { get; set; }
        public DateFormElementModel()
        {
            QuestionType = QuestionType.Date;
        }
    }
    [BsonDiscriminator(nameof(ShortTextFormElementModel))]
    public class ShortTextFormElementModel : BaseFormElementModel
    {
        [Required]
        [JsonPropertyName("answer")]
        [BsonElement("answer")]
        [Display(Name = "Your answer", Prompt = "Enter correct answer")]
        [MaxLength(256)]
        public string Answer { get; set; } = string.Empty;
        public ShortTextFormElementModel()
        {
            QuestionType = QuestionType.ShortText;
        }
    }
    [BsonDiscriminator(nameof(LongTextFormElementModel))]
    public class LongTextFormElementModel : BaseFormElementModel
    {
        [Required]
        [JsonPropertyName("answer")]
        [BsonElement("answer")]
        [Display(Name = "Your answer", Prompt = "Enter your answer")]
        [DataType(DataType.MultilineText)]
        public string Answer { get; set; } = string.Empty;
        public LongTextFormElementModel()
        {
            QuestionType = QuestionType.LongText;
        }
    }
    [BsonDiscriminator(nameof(TimeFormElementModel))]
    public class TimeFormElementModel : BaseFormElementModel
    {
        [Required]
        [BsonTimeSpanOptions(BsonType.String)]
        [BsonElement("time")]
        [JsonPropertyName("time")]
        [Display(Name = "Time", Prompt = "Choose time")]
        public TimeSpan Time { get; set; }
        public TimeFormElementModel()
        {
            QuestionType = QuestionType.Time;
        }
    }
    [BsonDiscriminator(nameof(SingleOptionFormElementModel))]
    public class SingleOptionFormElementModel : BaseFormElementModel
    {
        [Required]
        [JsonPropertyName("options")]
        [BsonElement("options")]
        [Display(Name = "Options", Prompt = "Choose single option")]

        public List<string> Options { get; set; } = new List<string>();
        [Required]
        [JsonPropertyName("correctAnswer")]
        [BsonElement("correct_answer")]
        public int CorrectAnswer { get; set; }
        public SingleOptionFormElementModel()
        {
            QuestionType = QuestionType.SingleOption;
        }
    }
    [BsonDiscriminator(nameof(MultipleOptionsFormElementModel))]
    public class MultipleOptionsFormElementModel : BaseFormElementModel
    {
        [Required]
        [JsonPropertyName("options")]
        [BsonElement("options")]
        [Display(Name = "Options", Prompt = "Choose multiple options")]
        public List<string> Options { get; set; } = new List<string>();
        [Required]
        [JsonPropertyName("correctAnswers")]
        [BsonElement("correct_answers")]
        [Display(Name = "Correct answers", Prompt = "Choose answers")]
        public List<bool> CorrectAnswers { get; set; } = new List<bool>();
        public MultipleOptionsFormElementModel()
        {
            QuestionType = QuestionType.MultipleOptions;
        }
    }
    public class FormElementConverter : JsonConverter<BaseFormElementModel>
    {
        public override BaseFormElementModel Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException();
            }

            using var jsonDoc = JsonDocument.ParseValue(ref reader);
            var jsonObject = jsonDoc.RootElement;
            var type = (QuestionType)jsonObject.GetProperty("questionType").GetInt32();
            var elementType = GetElementType(type);
            if (type == QuestionType.None) return null;
            return JsonSerializer.Deserialize(jsonDoc.RootElement.GetRawText(), elementType, options) as BaseFormElementModel;
        }

        public override void Write(Utf8JsonWriter writer, BaseFormElementModel value, JsonSerializerOptions options)
        {
            JsonSerializer.Serialize(writer, value, value.GetType(), options);
        }
        private static Type GetElementType(QuestionType type)
        {
            return type switch
            {
                QuestionType.None => typeof(BaseFormElementModel),
                QuestionType.ShortText => typeof(ShortTextFormElementModel),
                QuestionType.LongText => typeof(LongTextFormElementModel),
                QuestionType.SingleOption => typeof(SingleOptionFormElementModel),
                QuestionType.MultipleOptions => typeof(MultipleOptionsFormElementModel),
                QuestionType.Date => typeof(DateFormElementModel),
                QuestionType.Time => typeof(TimeFormElementModel),
                _ => throw new ArgumentException($"Invalid element type: {type}")
            };
        }
        private static Type GetElementType(string typeString)
        {
            return typeString switch
            {
                nameof(ShortTextFormElementModel) => typeof(ShortTextFormElementModel),
                nameof(LongTextFormElementModel) => typeof(LongTextFormElementModel),
                nameof(SingleOptionFormElementModel) => typeof(SingleOptionFormElementModel),
                nameof(MultipleOptionsFormElementModel) => typeof(MultipleOptionsFormElementModel),
                nameof(DateFormElementModel) => typeof(DateFormElementModel),
                nameof(TimeFormElementModel) => typeof(TimeFormElementModel),
                _ => throw new ArgumentException($"Invalid element type: {typeString}")
            };
        }
    }

}
