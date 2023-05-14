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
            writer.WriteStartObject();
            writer.WritePropertyName("questionType");
            writer.WriteNumberValue((int)value.QuestionType);
            writer.WritePropertyName("question");
            writer.WriteStringValue(value.Question);
            // Write other properties here...

            // Write type-specific properties based on the object's type
            switch (value)
            {
                case ShortTextFormElementModel shortText:
                    writer.WritePropertyName("answer");
                    writer.WriteStringValue(shortText.Answer);
                    break;

                case LongTextFormElementModel longText:
                    writer.WritePropertyName("answer");
                    writer.WriteStringValue(longText.Answer);
                    break;

                case SingleOptionFormElementModel singleOption:
                    writer.WritePropertyName("options");
                    JsonSerializer.Serialize(writer, singleOption.Options, options);
                    break;

                case MultipleOptionsFormElementModel multipleOptions:
                    writer.WritePropertyName("options");
                    JsonSerializer.Serialize(writer, multipleOptions.Options, options);
                    break;

                case DateFormElementModel date:
                    writer.WritePropertyName("date");
                    writer.WriteStringValue(date.Date.ToString("yyyy-MM-dd"));
                    break;

                case TimeFormElementModel time:
                    writer.WritePropertyName("time");
                    writer.WriteStringValue(time.Time.ToString("HH:mm:ss"));
                    break;

                default:
                    throw new ArgumentException($"Unsupported element type: {value.GetType().Name}");
            }

            writer.WriteEndObject();
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
