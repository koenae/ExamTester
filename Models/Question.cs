using System.Text.Json.Serialization;

namespace ExamTester.Models;

public enum QuestionType
{
    SingleChoice,
    MultipleChoice,
    DragAndDrop,
    FillInTheBlank,
    CaseStudy,
    Matching,
    YesNo
}

public class Question
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("text")]
    public string Text { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public QuestionType Type { get; set; } = QuestionType.SingleChoice;

    [JsonPropertyName("options")]
    public List<string> Options { get; set; } = new();

    [JsonPropertyName("correctAnswer")]
    public int CorrectAnswer { get; set; }

    [JsonPropertyName("correctAnswers")]
    public List<int>? CorrectAnswers { get; set; }

    [JsonPropertyName("correctOrder")]
    public List<int>? CorrectOrder { get; set; }

    [JsonPropertyName("correctText")]
    public string? CorrectText { get; set; }

    [JsonPropertyName("matchPairs")]
    public List<MatchPair>? MatchPairs { get; set; }

    [JsonPropertyName("subQuestions")]
    public List<Question>? SubQuestions { get; set; }

    [JsonPropertyName("explanation")]
    public string? Explanation { get; set; }

    [JsonPropertyName("domain")]
    public string? Domain { get; set; }

    [JsonPropertyName("difficulty")]
    public int Difficulty { get; set; } = 3;

    [JsonPropertyName("hint")]
    public string? Hint { get; set; }

    [JsonPropertyName("scenarioText")]
    public string? ScenarioText { get; set; }
}

public class MatchPair
{
    [JsonPropertyName("left")]
    public string Left { get; set; } = string.Empty;

    [JsonPropertyName("right")]
    public string Right { get; set; } = string.Empty;
}
