namespace AtlasChatApi.Services;

/// <summary>
/// Harici bağımlılığı olmayan, anahtar kelime tabanlı basit yanıt üreticisi.
/// Uygulamanın hiçbir kurulum gerektirmeden "kutudan çıktığı gibi" çalışmasını sağlar.
/// </summary>
public class RuleBasedChatService : IChatService
{
    public Task<string> GetResponseAsync(string message, CancellationToken cancellationToken = default)
    {
        var normalized = (message ?? string.Empty).Trim();

        string reply =
            Contains(normalized, "merhaba", "selam", "günaydın", "iyi günler")
                ? "Merhaba, size nasıl yardımcı olabilirim?"
            : Contains(normalized, "nasılsın", "naber")
                ? "Teşekkür ederim, iyiyim. Size nasıl yardımcı olabilirim?"
            : Contains(normalized, "teşekkür", "sağ ol", "sağol", "eyvallah")
                ? "Rica ederim! Başka bir konuda yardımcı olabilir miyim?"
            : Contains(normalized, "görüşürüz", "hoşça kal", "iyi günler dilerim")
                ? "İyi günler dilerim, görüşmek üzere!"
            : Contains(normalized, "fiyat", "ücret", "maliyet")
                ? "Fiyat bilgisi için sizi ilgili birime yönlendirebilirim. Hangi ürünle ilgileniyorsunuz?"
            : "Sorunuzu aldım. Şu an demo modunda sabit yanıt veriyorum; gerçek bir yapay zeka "
              + "modeli bağlamak için README'deki Ollama adımlarını izleyebilirsiniz.";

        return Task.FromResult(reply);
    }

    private static bool Contains(string source, params string[] keywords)
        => keywords.Any(keyword => source.Contains(keyword, StringComparison.OrdinalIgnoreCase));
}
