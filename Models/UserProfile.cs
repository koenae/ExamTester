using System.Text.Json.Serialization;

namespace ExamTester.Models;

public class UserProfile
{
    [JsonPropertyName("displayName")]
    public string DisplayName { get; set; } = "Exam Candidate";

    [JsonPropertyName("llmConfig")]
    public LlmConfig LlmConfig { get; set; } = new();

    [JsonPropertyName("settings")]
    public AppSettings Settings { get; set; } = new();

    [JsonPropertyName("examHistory")]
    public List<ExamAttempt> ExamHistory { get; set; } = new();

    [JsonPropertyName("customExams")]
    public List<string> CustomExamFiles { get; set; } = new();

    [JsonPropertyName("favoriteExams")]
    public HashSet<string> FavoriteExams { get; set; } = new();

    [JsonPropertyName("studyStreak")]
    public int StudyStreak { get; set; }

    [JsonPropertyName("lastActiveDate")]
    public DateTime LastActiveDate { get; set; } = DateTime.Now;

    [JsonPropertyName("totalStudyTimeMinutes")]
    public int TotalStudyTimeMinutes { get; set; }
}

public class AppSettings
{
    [JsonPropertyName("darkMode")]
    public bool DarkMode { get; set; }

    [JsonPropertyName("showHints")]
    public bool ShowHints { get; set; } = true;

    [JsonPropertyName("shuffleQuestions")]
    public bool ShuffleQuestions { get; set; } = true;

    [JsonPropertyName("shuffleOptions")]
    public bool ShuffleOptions { get; set; }

    [JsonPropertyName("defaultQuestionCount")]
    public int DefaultQuestionCount { get; set; } = 50;

    [JsonPropertyName("autoSaveProgress")]
    public bool AutoSaveProgress { get; set; } = true;

    [JsonPropertyName("showDomainTags")]
    public bool ShowDomainTags { get; set; } = true;

    [JsonPropertyName("confirmBeforeSubmit")]
    public bool ConfirmBeforeSubmit { get; set; } = true;

    [JsonPropertyName("enableKeyboardShortcuts")]
    public bool EnableKeyboardShortcuts { get; set; } = true;

    [JsonPropertyName("questionFontSize")]
    public string QuestionFontSize { get; set; } = "medium";
}
