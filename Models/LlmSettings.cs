namespace ExamTester.Models;

public class LlmSettings
{
    public string ApiKey { get; set; } = string.Empty;
    public string ModelId { get; set; } = "gpt-4o-mini";
    public LlmProvider Provider { get; set; } = LlmProvider.OpenAI;
}

public enum LlmProvider
{
    OpenAI,
    AzureOpenAI
}
