using System.Text.Json.Serialization;

namespace ExamTester.Models;

public class Exam
{
    [JsonPropertyName("examTitle")]
    public string ExamTitle { get; set; } = string.Empty;

    [JsonPropertyName("examCode")]
    public string ExamCode { get; set; } = string.Empty;

    [JsonPropertyName("vendor")]
    public string Vendor { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("timeLimit")]
    public int TimeLimit { get; set; } = 60;

    [JsonPropertyName("questions")]
    public List<Question> Questions { get; set; } = new();

    [JsonPropertyName("passingScore")]
    public int PassingScore { get; set; } = 70;

    [JsonPropertyName("domains")]
    public List<string> Domains { get; set; } = new();

    [JsonPropertyName("difficulty")]
    public string Difficulty { get; set; } = "Intermediate";

    [JsonPropertyName("version")]
    public string Version { get; set; } = "1.0";

    [JsonPropertyName("generatedByAi")]
    public bool GeneratedByAi { get; set; }
}
