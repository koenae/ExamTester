using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using ExamTester.Models;

namespace ExamTester.Services;

public class LlmService
{
    private readonly HttpClient _httpClient;
    private readonly PersistenceService _persistence;
    private LlmConfig? _config;

    public bool IsConfigured => _config?.IsConfigured ?? false;
    public string? CurrentModel => _config?.ModelName;
    public string? CurrentProvider => _config?.Provider;

    public event Action? OnConfigChanged;

    public LlmService(PersistenceService persistence)
    {
        _httpClient = new HttpClient();
        _httpClient.Timeout = TimeSpan.FromMinutes(5);
        _persistence = persistence;
    }

    public async Task InitializeAsync()
    {
        _config = await _persistence.LoadLlmConfigAsync();
    }

    public async Task UpdateConfigAsync(LlmConfig config)
    {
        _config = config;
        await _persistence.SaveLlmConfigAsync(config);
        OnConfigChanged?.Invoke();
    }

    public LlmConfig GetConfig()
    {
        return _config ?? new LlmConfig();
    }

    public async Task<(bool Success, string Message)> TestConnectionAsync()
    {
        if (_config == null || !_config.IsConfigured)
            return (false, "LLM is not configured. Please set up your API credentials first.");

        try
        {
            var response = await SendChatRequestAsync("Respond with exactly: OK", "You are a test assistant. Respond with exactly 'OK' and nothing else.", maxTokens: 10);
            if (!string.IsNullOrEmpty(response))
                return (true, $"Connection successful! Model: {_config.ModelName}");
            return (false, "Received empty response from API.");
        }
        catch (HttpRequestException ex)
        {
            return (false, $"Connection failed: {ex.Message}");
        }
        catch (Exception ex)
        {
            return (false, $"Error: {ex.Message}");
        }
    }

    public async Task<string> SendChatRequestAsync(string userMessage, string systemPrompt, double? temperature = null, int? maxTokens = null)
    {
        if (_config == null || !_config.IsConfigured)
            throw new InvalidOperationException("LLM is not configured.");

        if (_config.Provider == "Anthropic" && LlmConfig.Providers.TryGetValue("Anthropic", out var preset) && preset.UsesAnthropicFormat)
        {
            return await SendAnthropicRequestAsync(userMessage, systemPrompt, temperature, maxTokens);
        }

        if (_config.Provider == "Google Gemini" && LlmConfig.Providers.TryGetValue("Google Gemini", out var geminiPreset) && geminiPreset.UsesGeminiFormat)
        {
            return await SendGeminiRequestAsync(userMessage, systemPrompt, temperature, maxTokens);
        }

        return await SendOpenAiCompatibleRequestAsync(userMessage, systemPrompt, temperature, maxTokens);
    }

    private async Task<string> SendOpenAiCompatibleRequestAsync(string userMessage, string systemPrompt, double? temperature, int? maxTokens)
    {
        var endpoint = _config!.ApiEndpoint.TrimEnd('/');
        if (!endpoint.EndsWith("/chat/completions"))
            endpoint += "/chat/completions";

        var request = new
        {
            model = _config.ModelName,
            messages = new[]
            {
                new { role = "system", content = systemPrompt },
                new { role = "user", content = userMessage }
            },
            temperature = temperature ?? _config.Temperature,
            max_tokens = maxTokens ?? _config.MaxTokens
        };

        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var httpRequest = new HttpRequestMessage(HttpMethod.Post, endpoint);
        httpRequest.Content = content;

        if (!string.IsNullOrWhiteSpace(_config.ApiKey))
        {
            httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _config.ApiKey);
        }

        var response = await _httpClient.SendAsync(httpRequest);
        var responseBody = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException($"API returned {response.StatusCode}: {responseBody}");
        }

        using var doc = JsonDocument.Parse(responseBody);
        var choices = doc.RootElement.GetProperty("choices");
        if (choices.GetArrayLength() > 0)
        {
            return choices[0].GetProperty("message").GetProperty("content").GetString() ?? string.Empty;
        }

        return string.Empty;
    }

    private async Task<string> SendAnthropicRequestAsync(string userMessage, string systemPrompt, double? temperature, int? maxTokens)
    {
        var endpoint = _config!.ApiEndpoint.TrimEnd('/');
        if (!endpoint.EndsWith("/messages"))
            endpoint += "/messages";

        var request = new
        {
            model = _config.ModelName,
            max_tokens = maxTokens ?? _config.MaxTokens,
            system = systemPrompt,
            messages = new[]
            {
                new { role = "user", content = userMessage }
            }
        };

        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var httpRequest = new HttpRequestMessage(HttpMethod.Post, endpoint);
        httpRequest.Content = content;
        httpRequest.Headers.Add("x-api-key", _config.ApiKey);
        httpRequest.Headers.Add("anthropic-version", "2023-06-01");

        var response = await _httpClient.SendAsync(httpRequest);
        var responseBody = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException($"API returned {response.StatusCode}: {responseBody}");
        }

        using var doc = JsonDocument.Parse(responseBody);
        var contentArray = doc.RootElement.GetProperty("content");
        if (contentArray.GetArrayLength() > 0)
        {
            return contentArray[0].GetProperty("text").GetString() ?? string.Empty;
        }

        return string.Empty;
    }

    private async Task<string> SendGeminiRequestAsync(string userMessage, string systemPrompt, double? temperature, int? maxTokens)
    {
        var endpoint = _config!.ApiEndpoint.TrimEnd('/');
        endpoint += $"/models/{_config.ModelName}:generateContent?key={_config.ApiKey}";

        var request = new
        {
            system_instruction = new
            {
                parts = new[] { new { text = systemPrompt } }
            },
            contents = new[]
            {
                new
                {
                    parts = new[] { new { text = userMessage } }
                }
            },
            generationConfig = new
            {
                temperature = temperature ?? _config.Temperature,
                maxOutputTokens = maxTokens ?? _config.MaxTokens
            }
        };

        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync(endpoint, content);
        var responseBody = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException($"API returned {response.StatusCode}: {responseBody}");
        }

        using var doc = JsonDocument.Parse(responseBody);
        var candidates = doc.RootElement.GetProperty("candidates");
        if (candidates.GetArrayLength() > 0)
        {
            var parts = candidates[0].GetProperty("content").GetProperty("parts");
            if (parts.GetArrayLength() > 0)
            {
                return parts[0].GetProperty("text").GetString() ?? string.Empty;
            }
        }

        return string.Empty;
    }
}
