using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace AtlasChatApi.Services;

/// <summary>
/// Lokal ve ücretsiz çalışan Ollama (https://ollama.com) üzerinden yanıt üretir.
/// appsettings.json içinde Chat:Provider = "Ollama" olduğunda devreye girer.
/// Ollama'ya erişilemezse uygulamanın akışını bozmamak için kural tabanlı yanıta düşer.
/// </summary>
public class OllamaChatService : IChatService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<OllamaChatService> _logger;
    private readonly IChatService _fallback = new RuleBasedChatService();

    private readonly string _model;
    private readonly string _systemPrompt;

    public OllamaChatService(HttpClient httpClient, IConfiguration configuration, ILogger<OllamaChatService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;

        var baseUrl = configuration["Chat:Ollama:BaseUrl"] ?? "http://localhost:11434";
        _httpClient.BaseAddress = new Uri(baseUrl);
        _httpClient.Timeout = TimeSpan.FromSeconds(60);

        _model = configuration["Chat:Ollama:Model"] ?? "llama3.2";
        _systemPrompt = configuration["Chat:Ollama:SystemPrompt"]
            ?? "Sen bir Türkçe çağrı merkezi asistanısın. Kullanıcıya yardımcı ol, dilbilgisi açısından doğru, kısa ve net yanıtlar ver.";
    }

    public async Task<string> GetResponseAsync(string message, CancellationToken cancellationToken = default)
    {
        try
        {
            var payload = new OllamaChatRequest
            {
                Model = _model,
                Stream = false,
                Messages = new[]
                {
                    new OllamaMessage("system", _systemPrompt),
                    new OllamaMessage("user", message)
                }
            };

            using var httpResponse = await _httpClient.PostAsJsonAsync("/api/chat", payload, cancellationToken);
            httpResponse.EnsureSuccessStatusCode();

            var result = await httpResponse.Content
                .ReadFromJsonAsync<OllamaChatResponse>(cancellationToken: cancellationToken);

            var content = result?.Message?.Content?.Trim();
            return string.IsNullOrWhiteSpace(content)
                ? await _fallback.GetResponseAsync(message, cancellationToken)
                : content;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Ollama çağrısı başarısız oldu; kural tabanlı yanıta geçiliyor.");
            return await _fallback.GetResponseAsync(message, cancellationToken);
        }
    }

    // --- Ollama /api/chat sözleşmesi (yalnızca kullanılan alanlar) ---

    private sealed class OllamaChatRequest
    {
        [JsonPropertyName("model")] public string Model { get; set; } = string.Empty;
        [JsonPropertyName("stream")] public bool Stream { get; set; }
        [JsonPropertyName("messages")] public OllamaMessage[] Messages { get; set; } = Array.Empty<OllamaMessage>();
    }

    private sealed record OllamaMessage(
        [property: JsonPropertyName("role")] string Role,
        [property: JsonPropertyName("content")] string Content);

    private sealed class OllamaChatResponse
    {
        [JsonPropertyName("message")] public OllamaMessage? Message { get; set; }
    }
}
