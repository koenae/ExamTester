using System.Text.Json.Serialization;

namespace ExamTester.Models;

public class Exam
{
    [JsonPropertyName("examTitle")]
    public string ExamTitle { get; set; } = string.Empty;

    [JsonPropertyName("timeLimit")]
    public int TimeLimit { get; set; } = 60;

    [JsonPropertyName("questions")]
    public List<Question> Questions { get; set; } = new();

    [JsonPropertyName("passingScore")]
    public int PassingScore { get; set; } = 70;
}
