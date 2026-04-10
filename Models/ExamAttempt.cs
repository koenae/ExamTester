using System.Text.Json.Serialization;

namespace ExamTester.Models;

public class ExamAttempt
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [JsonPropertyName("examTitle")]
    public string ExamTitle { get; set; } = string.Empty;

    [JsonPropertyName("examCode")]
    public string ExamCode { get; set; } = string.Empty;

    [JsonPropertyName("vendor")]
    public string Vendor { get; set; } = string.Empty;

    [JsonPropertyName("startTime")]
    public DateTime StartTime { get; set; }

    [JsonPropertyName("endTime")]
    public DateTime EndTime { get; set; }

    [JsonPropertyName("timeSpentSeconds")]
    public int TimeSpentSeconds { get; set; }

    [JsonPropertyName("scorePercentage")]
    public double ScorePercentage { get; set; }

    [JsonPropertyName("correctAnswers")]
    public int CorrectAnswers { get; set; }

    [JsonPropertyName("totalQuestions")]
    public int TotalQuestions { get; set; }

    [JsonPropertyName("isPassed")]
    public bool IsPassed { get; set; }

    [JsonPropertyName("passingScore")]
    public int PassingScore { get; set; }

    [JsonPropertyName("mode")]
    public string Mode { get; set; } = "Exam";

    [JsonPropertyName("domainScores")]
    public Dictionary<string, double> DomainScores { get; set; } = new();

    [JsonPropertyName("examJson")]
    public string? ExamJson { get; set; }

    [JsonPropertyName("answersJson")]
    public string? AnswersJson { get; set; }

    public string TimeSpentFormatted
    {
        get
        {
            var span = TimeSpan.FromSeconds(TimeSpentSeconds);
            return span.Hours > 0
                ? $"{span.Hours:D2}:{span.Minutes:D2}:{span.Seconds:D2}"
                : $"{span.Minutes:D2}:{span.Seconds:D2}";
        }
    }
}
