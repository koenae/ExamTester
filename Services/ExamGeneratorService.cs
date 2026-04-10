using System.Text.Json;
using ExamTester.Models;

namespace ExamTester.Services;

public class ExamGeneratorService
{
    private readonly LlmService _llm;
    private readonly ExamCatalogService _catalog;
    private readonly JsonSerializerOptions _jsonOptions;

    public ExamGeneratorService(LlmService llm, ExamCatalogService catalog)
    {
        _llm = llm;
        _catalog = catalog;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    public async Task<Exam> GenerateExamAsync(ExamGenerationRequest request, Action<string>? onProgress = null)
    {
        var catalogEntry = _catalog.GetExamById(request.ExamId);
        if (catalogEntry == null)
            throw new ArgumentException($"Exam '{request.ExamId}' not found in catalog.");

        var questionCount = request.QuestionCount > 0 ? request.QuestionCount : catalogEntry.QuestionCount;
        var batchSize = Math.Min(10, questionCount);
        var batches = (int)Math.Ceiling((double)questionCount / batchSize);
        var allQuestions = new List<Question>();

        for (int batch = 0; batch < batches; batch++)
        {
            var remaining = questionCount - allQuestions.Count;
            var currentBatch = Math.Min(batchSize, remaining);

            onProgress?.Invoke($"Generating questions {allQuestions.Count + 1}-{allQuestions.Count + currentBatch} of {questionCount}...");

            var prompt = BuildGenerationPrompt(catalogEntry, request, currentBatch, allQuestions.Count + 1);
            var systemPrompt = BuildSystemPrompt(catalogEntry);

            var response = await _llm.SendChatRequestAsync(prompt, systemPrompt, temperature: 0.8, maxTokens: 8192);

            var questions = ParseQuestionsFromResponse(response, allQuestions.Count + 1);
            allQuestions.AddRange(questions);
        }

        onProgress?.Invoke("Finalizing exam...");

        return new Exam
        {
            ExamTitle = $"{catalogEntry.Name} ({catalogEntry.Code}) - Practice Exam",
            ExamCode = catalogEntry.Code,
            Vendor = catalogEntry.Vendor,
            Description = catalogEntry.Description,
            TimeLimit = request.TimeLimit > 0 ? request.TimeLimit : catalogEntry.TimeLimit,
            PassingScore = catalogEntry.PassingScore,
            Questions = allQuestions.Take(questionCount).ToList(),
            Domains = catalogEntry.Domains.Select(d => d.Name).ToList(),
            Difficulty = request.Difficulty ?? catalogEntry.Difficulty,
            GeneratedByAi = true,
            Version = DateTime.Now.ToString("yyyy.MM.dd")
        };
    }

    public async Task<string> GenerateExplanationAsync(Question question, int? userAnswer = null)
    {
        var systemPrompt = "You are an expert instructor. Provide a clear, detailed explanation for the exam question. Explain why the correct answer is right and why each wrong answer is wrong. Be educational and thorough.";

        var userPrompt = $"""
            Question: {question.Text}

            Options:
            {string.Join("\n", question.Options.Select((o, i) => $"{(char)('A' + i)}. {o}"))}

            Correct Answer: {(char)('A' + question.CorrectAnswer)}
            {(userAnswer.HasValue ? $"User's Answer: {(char)('A' + userAnswer.Value)}" : "")}

            Please explain:
            1. Why the correct answer is right
            2. Why each other option is wrong
            3. Key concepts to remember
            """;

        return await _llm.SendChatRequestAsync(userPrompt, systemPrompt, maxTokens: 2048);
    }

    public async Task<List<Question>> GenerateAdaptiveQuestionsAsync(string examCode, Dictionary<string, DomainScore> domainScores, int count = 5)
    {
        var catalogEntry = _catalog.GetExamById(examCode);
        if (catalogEntry == null) return new List<Question>();

        var weakDomains = domainScores
            .Where(d => d.Value.Percentage < 70)
            .OrderBy(d => d.Value.Percentage)
            .Select(d => d.Key)
            .ToList();

        if (weakDomains.Count == 0)
        {
            weakDomains = domainScores.Keys.ToList();
        }

        var systemPrompt = BuildSystemPrompt(catalogEntry);
        var prompt = $"""
            Generate {count} practice questions focused on these weak areas:
            {string.Join(", ", weakDomains)}

            These questions should target concepts the student is struggling with.
            Make the questions challenging but educational.

            {GetQuestionFormatInstructions(count, 1)}
            """;

        var response = await _llm.SendChatRequestAsync(prompt, systemPrompt, temperature: 0.8, maxTokens: 4096);
        return ParseQuestionsFromResponse(response, 1);
    }

    private string BuildSystemPrompt(ExamCatalogEntry entry)
    {
        return $"""
            You are an expert certification exam question writer for {entry.Vendor} {entry.Name} ({entry.Code}).

            You create high-quality, realistic practice exam questions that match the style, difficulty, and scope of the actual certification exam.

            Exam domains and weights:
            {string.Join("\n", entry.Domains.Select(d => $"- {d.Name} ({d.Weight}%): {string.Join(", ", d.Topics)}"))}

            Key objectives:
            {string.Join("\n", entry.Objectives.Select(o => $"- {o}"))}

            Rules:
            1. Questions must be technically accurate and up-to-date
            2. Each question must have exactly 4 options (A-D)
            3. Only one correct answer per question
            4. Distractors should be plausible but clearly wrong to someone who knows the material
            5. Avoid trick questions or overly ambiguous wording
            6. Include scenario-based questions where appropriate
            7. Cover a balanced mix of the exam domains
            8. Vary difficulty levels within the specified range
            9. ALWAYS respond with valid JSON only - no markdown, no code blocks, no extra text
            """;
    }

    private string BuildGenerationPrompt(ExamCatalogEntry entry, ExamGenerationRequest request, int count, int startId)
    {
        var difficultyInstruction = request.Difficulty switch
        {
            "Beginner" => "Focus on fundamental concepts and straightforward recall questions.",
            "Advanced" => "Include complex scenarios, multi-step problem-solving, and deep technical knowledge.",
            "Expert" => "Focus on advanced architecture decisions, edge cases, and nuanced technical details.",
            _ => "Mix of straightforward recall, scenario-based, and analytical questions."
        };

        var questionTypes = request.IncludeQuestionTypes ?? new List<string> { "SingleChoice" };
        var typeInstructions = GetQuestionTypeInstructions(questionTypes);

        return $"""
            Generate exactly {count} practice exam questions for {entry.Name} ({entry.Code}).

            Difficulty: {request.Difficulty ?? entry.Difficulty}
            {difficultyInstruction}

            {(request.FocusDomains?.Count > 0 ? $"Focus on these domains: {string.Join(", ", request.FocusDomains)}" : "Cover all exam domains proportionally.")}

            {typeInstructions}

            {GetQuestionFormatInstructions(count, startId)}
            """;
    }

    private string GetQuestionTypeInstructions(List<string> types)
    {
        var instructions = new List<string>();
        foreach (var type in types)
        {
            switch (type)
            {
                case "MultipleChoice":
                    instructions.Add("Include some 'select all that apply' questions with multiple correct answers. For these, set \"type\": \"MultipleChoice\" and use \"correctAnswers\": [0, 2] (array of indices).");
                    break;
                case "DragAndDrop":
                    instructions.Add("Include ordering questions where items must be arranged in the correct sequence. Set \"type\": \"DragAndDrop\" and use \"correctOrder\": [2, 0, 3, 1] (correct arrangement of option indices).");
                    break;
                case "FillInTheBlank":
                    instructions.Add("Include fill-in-the-blank questions. Set \"type\": \"FillInTheBlank\" with \"correctText\": \"expected answer\" and leave options empty.");
                    break;
                case "YesNo":
                    instructions.Add("Include True/False questions. Set \"type\": \"YesNo\" with options [\"True\", \"False\"] and correctAnswer as 0 or 1.");
                    break;
            }
        }
        return instructions.Count > 0 ? string.Join("\n", instructions) : "";
    }

    private string GetQuestionFormatInstructions(int count, int startId)
    {
        return $$"""
            Respond with ONLY a valid JSON array. No markdown, no code blocks, no explanation.
            Format:
            [
              {
                "id": {{startId}},
                "text": "Question text here?",
                "type": "SingleChoice",
                "options": ["Option A", "Option B", "Option C", "Option D"],
                "correctAnswer": 0,
                "explanation": "Explanation of why this answer is correct and others are wrong.",
                "domain": "Domain Name",
                "difficulty": 3,
                "hint": "A helpful hint for study mode"
              }
            ]

            Generate exactly {{count}} questions starting from id {{startId}}.
            """;
    }

    private List<Question> ParseQuestionsFromResponse(string response, int startId)
    {
        var cleaned = response.Trim();

        var jsonStart = cleaned.IndexOf('[');
        var jsonEnd = cleaned.LastIndexOf(']');
        if (jsonStart >= 0 && jsonEnd > jsonStart)
        {
            cleaned = cleaned.Substring(jsonStart, jsonEnd - jsonStart + 1);
        }

        try
        {
            var questions = JsonSerializer.Deserialize<List<Question>>(cleaned, _jsonOptions);
            if (questions != null)
            {
                for (int i = 0; i < questions.Count; i++)
                {
                    questions[i].Id = startId + i;
                }
                return questions;
            }
        }
        catch (JsonException)
        {
            // Try to salvage individual questions
        }

        return new List<Question>
        {
            new Question
            {
                Id = startId,
                Text = "Failed to parse generated question. Please try again.",
                Options = new List<string> { "Retry generation", "Check LLM configuration", "Try a different model", "Report issue" },
                CorrectAnswer = 0,
                Explanation = "The AI response could not be parsed as valid questions. Try regenerating or check your LLM settings.",
                Domain = "General"
            }
        };
    }
}

public class ExamGenerationRequest
{
    public string ExamId { get; set; } = string.Empty;
    public int QuestionCount { get; set; }
    public int TimeLimit { get; set; }
    public string? Difficulty { get; set; }
    public List<string>? FocusDomains { get; set; }
    public List<string>? IncludeQuestionTypes { get; set; }
    public bool ShuffleQuestions { get; set; } = true;
}
