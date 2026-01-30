using System.Text.Json;
using ExamTester.Models;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace ExamTester.Services;

public class QuestionGeneratorService
{
    private LlmSettings _settings = new();

    public LlmSettings Settings => _settings;

    public event Action? OnSettingsChanged;
    public event Action<string>? OnProgressUpdate;

    public void UpdateSettings(LlmSettings settings)
    {
        _settings = settings;
        OnSettingsChanged?.Invoke();
    }

    public bool IsConfigured => !string.IsNullOrWhiteSpace(_settings.ApiKey);

    public async Task<Exam?> GenerateExamAsync(string examNumber, int questionCount = 10, CancellationToken cancellationToken = default)
    {
        if (!IsConfigured)
        {
            throw new InvalidOperationException("API key is not configured");
        }

        OnProgressUpdate?.Invoke("Initializing AI model...");

        var builder = Kernel.CreateBuilder();

        if (_settings.Provider == LlmProvider.OpenAI)
        {
            builder.AddOpenAIChatCompletion(
                modelId: _settings.ModelId,
                apiKey: _settings.ApiKey
            );
        }
        else
        {
            throw new NotSupportedException($"Provider {_settings.Provider} is not yet supported");
        }

        var kernel = builder.Build();
        var chatService = kernel.GetRequiredService<IChatCompletionService>();

        OnProgressUpdate?.Invoke($"Generating {questionCount} questions for exam {examNumber}...");

        var prompt = CreatePrompt(examNumber, questionCount);

        var chatHistory = new ChatHistory();
        chatHistory.AddSystemMessage("You are an expert exam question generator. You create realistic, challenging multiple-choice questions for certification exams. Always respond with valid JSON only, no additional text.");
        chatHistory.AddUserMessage(prompt);

        try
        {
            var response = await chatService.GetChatMessageContentAsync(
                chatHistory,
                cancellationToken: cancellationToken
            );

            OnProgressUpdate?.Invoke("Parsing generated questions...");

            var jsonContent = response.Content ?? string.Empty;

            // Extract JSON if wrapped in markdown code blocks
            jsonContent = ExtractJsonFromResponse(jsonContent);

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var exam = JsonSerializer.Deserialize<Exam>(jsonContent, options);

            if (exam != null)
            {
                // Ensure questions have sequential IDs
                for (int i = 0; i < exam.Questions.Count; i++)
                {
                    exam.Questions[i].Id = i + 1;
                }

                OnProgressUpdate?.Invoke("Exam generated successfully!");
            }

            return exam;
        }
        catch (JsonException ex)
        {
            OnProgressUpdate?.Invoke($"Failed to parse response: {ex.Message}");
            throw new InvalidOperationException("Failed to parse the generated exam. The AI response was not in the expected format.", ex);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            OnProgressUpdate?.Invoke($"Error: {ex.Message}");
            throw;
        }
    }

    private static string CreatePrompt(string examNumber, int questionCount)
    {
        return $@"Generate a practice exam for certification exam ""{examNumber}"".

Create exactly {questionCount} multiple-choice questions that would be typical for this certification exam.

Requirements:
- Each question should have exactly 4 options (A, B, C, D)
- Questions should vary in difficulty
- Include realistic scenarios and technical details
- Provide clear explanations for the correct answers

Respond with ONLY a valid JSON object in this exact format (no markdown, no additional text):
{{
  ""examTitle"": ""[Exam Number] Practice Exam"",
  ""timeLimit"": 60,
  ""passingScore"": 70,
  ""questions"": [
    {{
      ""id"": 1,
      ""text"": ""Question text here?"",
      ""options"": [""Option A"", ""Option B"", ""Option C"", ""Option D""],
      ""correctAnswer"": 0,
      ""explanation"": ""Explanation of why the correct answer is correct.""
    }}
  ]
}}

Important:
- correctAnswer is a zero-based index (0 for first option, 1 for second, etc.)
- All questions must be relevant to the {examNumber} certification
- Response must be valid JSON only";
    }

    private static string ExtractJsonFromResponse(string response)
    {
        // Try to extract JSON from markdown code blocks
        var jsonStartMarkers = new[] { "```json", "```" };
        var jsonEndMarker = "```";

        foreach (var startMarker in jsonStartMarkers)
        {
            var startIndex = response.IndexOf(startMarker, StringComparison.OrdinalIgnoreCase);
            if (startIndex >= 0)
            {
                startIndex += startMarker.Length;
                var endIndex = response.IndexOf(jsonEndMarker, startIndex, StringComparison.OrdinalIgnoreCase);
                if (endIndex > startIndex)
                {
                    return response.Substring(startIndex, endIndex - startIndex).Trim();
                }
            }
        }

        // Try to find JSON object directly
        var jsonObjectStart = response.IndexOf('{');
        var jsonObjectEnd = response.LastIndexOf('}');

        if (jsonObjectStart >= 0 && jsonObjectEnd > jsonObjectStart)
        {
            return response.Substring(jsonObjectStart, jsonObjectEnd - jsonObjectStart + 1);
        }

        return response.Trim();
    }
}
