using System.Text.Json.Serialization;

namespace ExamTester.Models;

public class LlmConfig
{
    [JsonPropertyName("provider")]
    public string Provider { get; set; } = "OpenAI";

    [JsonPropertyName("apiEndpoint")]
    public string ApiEndpoint { get; set; } = "https://api.openai.com/v1";

    [JsonPropertyName("apiKey")]
    public string ApiKey { get; set; } = string.Empty;

    [JsonPropertyName("modelName")]
    public string ModelName { get; set; } = "gpt-4";

    [JsonPropertyName("temperature")]
    public double Temperature { get; set; } = 0.7;

    [JsonPropertyName("maxTokens")]
    public int MaxTokens { get; set; } = 4096;

    [JsonPropertyName("isConfigured")]
    public bool IsConfigured => !string.IsNullOrWhiteSpace(ApiKey) && !string.IsNullOrWhiteSpace(ApiEndpoint);

    public static readonly Dictionary<string, ProviderPreset> Providers = new()
    {
        ["OpenAI"] = new ProviderPreset
        {
            Name = "OpenAI",
            DefaultEndpoint = "https://api.openai.com/v1",
            Models = new[] { "gpt-4o", "gpt-4o-mini", "gpt-4-turbo", "gpt-4", "gpt-3.5-turbo", "o1", "o1-mini", "o3-mini" },
            DefaultModel = "gpt-4o",
            RequiresApiKey = true
        },
        ["Anthropic"] = new ProviderPreset
        {
            Name = "Anthropic",
            DefaultEndpoint = "https://api.anthropic.com/v1",
            Models = new[] { "claude-opus-4-20250514", "claude-sonnet-4-20250514", "claude-haiku-4-5-20251001", "claude-3-5-sonnet-20241022" },
            DefaultModel = "claude-sonnet-4-20250514",
            RequiresApiKey = true,
            UsesAnthropicFormat = true
        },
        ["Azure OpenAI"] = new ProviderPreset
        {
            Name = "Azure OpenAI",
            DefaultEndpoint = "https://{your-resource}.openai.azure.com/openai/deployments/{deployment-name}",
            Models = new[] { "gpt-4o", "gpt-4", "gpt-35-turbo" },
            DefaultModel = "gpt-4o",
            RequiresApiKey = true
        },
        ["Google Gemini"] = new ProviderPreset
        {
            Name = "Google Gemini",
            DefaultEndpoint = "https://generativelanguage.googleapis.com/v1beta",
            Models = new[] { "gemini-2.0-flash", "gemini-2.0-flash-lite", "gemini-1.5-pro", "gemini-1.5-flash" },
            DefaultModel = "gemini-2.0-flash",
            RequiresApiKey = true,
            UsesGeminiFormat = true
        },
        ["Ollama (Local)"] = new ProviderPreset
        {
            Name = "Ollama (Local)",
            DefaultEndpoint = "http://localhost:11434/v1",
            Models = new[] { "llama3.1", "llama3", "mistral", "mixtral", "codellama", "phi3", "gemma2", "qwen2" },
            DefaultModel = "llama3.1",
            RequiresApiKey = false
        },
        ["LM Studio (Local)"] = new ProviderPreset
        {
            Name = "LM Studio (Local)",
            DefaultEndpoint = "http://localhost:1234/v1",
            Models = new[] { "loaded-model" },
            DefaultModel = "loaded-model",
            RequiresApiKey = false
        },
        ["OpenRouter"] = new ProviderPreset
        {
            Name = "OpenRouter",
            DefaultEndpoint = "https://openrouter.ai/api/v1",
            Models = new[] { "anthropic/claude-sonnet-4", "openai/gpt-4o", "google/gemini-2.0-flash-exp", "meta-llama/llama-3.1-405b-instruct" },
            DefaultModel = "openai/gpt-4o",
            RequiresApiKey = true
        },
        ["Custom"] = new ProviderPreset
        {
            Name = "Custom",
            DefaultEndpoint = "http://localhost:8000/v1",
            Models = new[] { "custom-model" },
            DefaultModel = "custom-model",
            RequiresApiKey = false
        }
    };
}

public class ProviderPreset
{
    public string Name { get; set; } = string.Empty;
    public string DefaultEndpoint { get; set; } = string.Empty;
    public string[] Models { get; set; } = Array.Empty<string>();
    public string DefaultModel { get; set; } = string.Empty;
    public bool RequiresApiKey { get; set; } = true;
    public bool UsesAnthropicFormat { get; set; }
    public bool UsesGeminiFormat { get; set; }
}
