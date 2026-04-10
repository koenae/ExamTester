using System.Text.Json.Serialization;

namespace ExamTester.Models;

public class ExamCatalogEntry
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("code")]
    public string Code { get; set; } = string.Empty;

    [JsonPropertyName("vendor")]
    public string Vendor { get; set; } = string.Empty;

    [JsonPropertyName("category")]
    public string Category { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("questionCount")]
    public int QuestionCount { get; set; } = 50;

    [JsonPropertyName("timeLimit")]
    public int TimeLimit { get; set; } = 60;

    [JsonPropertyName("passingScore")]
    public int PassingScore { get; set; } = 70;

    [JsonPropertyName("domains")]
    public List<ExamDomain> Domains { get; set; } = new();

    [JsonPropertyName("objectives")]
    public List<string> Objectives { get; set; } = new();

    [JsonPropertyName("difficulty")]
    public string Difficulty { get; set; } = "Intermediate";

    [JsonPropertyName("tags")]
    public List<string> Tags { get; set; } = new();

    [JsonPropertyName("retiredDate")]
    public string? RetiredDate { get; set; }

    [JsonPropertyName("lastUpdated")]
    public string LastUpdated { get; set; } = "2024";
}

public class ExamDomain
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("weight")]
    public int Weight { get; set; }

    [JsonPropertyName("topics")]
    public List<string> Topics { get; set; } = new();
}
