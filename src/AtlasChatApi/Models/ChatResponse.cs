namespace AtlasChatApi.Models;

/// <summary>
/// POST /api/chat isteğine dönülen yanıt gövdesi.
/// </summary>
public class ChatResponse
{
    /// <summary>
    /// Asistanın ürettiği yanıt metni.
    /// </summary>
    public string Response { get; set; } = string.Empty;

    public ChatResponse(string response) => Response = response;
}
