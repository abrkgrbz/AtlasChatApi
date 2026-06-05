using AtlasChatApi.Models;
using AtlasChatApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace AtlasChatApi.Controllers;

/// <summary>
/// Sohbet uç noktası. POST /api/chat
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ChatController : ControllerBase
{
    private readonly IChatService _chatService;
    private readonly ILogger<ChatController> _logger;

    public ChatController(IChatService chatService, ILogger<ChatController> logger)
    {
        _chatService = chatService;
        _logger = logger;
    }

    /// <summary>
    /// Kullanıcı mesajını alır ve bir yanıt döner.
    /// </summary>
    /// <remarks>
    /// Örnek istek:
    ///
    ///     POST /api/chat
    ///     { "message": "Merhaba" }
    ///
    /// Örnek yanıt:
    ///
    ///     { "response": "Merhaba, size nasıl yardımcı olabilirim?" }
    /// </remarks>
    [HttpPost]
    [ProducesResponseType(typeof(ChatResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ChatResponse>> Post(
        [FromBody] ChatRequest request,
        CancellationToken cancellationToken)
    {
        // [ApiController] doğrulama hatalarında otomatik 400 döner;
        // burada açıkça bırakılması niyetin okunabilirliğini artırır.
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var reply = await _chatService.GetResponseAsync(request.Message, cancellationToken);

        // Not: Kullanıcı girdisini (request.Message) doğrudan log'a yazmıyoruz.
        // Bu, log injection / log forging (CWE-117) riskini önler.
        _logger.LogInformation("Chat isteği başarıyla işlendi.");

        return Ok(new ChatResponse(reply));
    }
}
