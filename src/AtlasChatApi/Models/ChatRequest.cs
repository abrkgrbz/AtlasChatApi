using System.ComponentModel.DataAnnotations;

namespace AtlasChatApi.Models;

/// <summary>
/// POST /api/chat isteğinin gövdesi.
/// </summary>
public class ChatRequest
{
    /// <summary>
    /// Kullanıcının gönderdiği mesaj. Zorunludur ve en fazla 1000 karakter olabilir.
    /// </summary>
    [Required(ErrorMessage = "message alanı zorunludur.")]
    [MaxLength(1000, ErrorMessage = "message en fazla 1000 karakter olabilir.")]
    public string Message { get; set; } = string.Empty;
}
