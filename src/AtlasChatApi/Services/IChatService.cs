namespace AtlasChatApi.Services;

/// <summary>
/// Sohbet yanıtı üreten servis soyutlaması.
/// Farklı sağlayıcılar (kural tabanlı, Ollama, OpenAI vb.) bu arayüzü uygulayarak
/// kolayca değiştirilebilir kılınır.
/// </summary>
public interface IChatService
{
    /// <summary>
    /// Verilen kullanıcı mesajına bir yanıt üretir.
    /// </summary>
    /// <param name="message">Kullanıcının mesajı.</param>
    /// <param name="cancellationToken">İptal jetonu.</param>
    /// <returns>Asistanın yanıtı.</returns>
    Task<string> GetResponseAsync(string message, CancellationToken cancellationToken = default);
}
