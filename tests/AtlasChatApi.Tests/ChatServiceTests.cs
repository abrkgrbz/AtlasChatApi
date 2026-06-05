using AtlasChatApi.Services;
using Xunit;

namespace AtlasChatApi.Tests;

public class RuleBasedChatServiceTests
{
    private readonly IChatService _service = new RuleBasedChatService();

    [Theory]
    [InlineData("Merhaba")]
    [InlineData("merhaba")]
    [InlineData("Selam")]
    [InlineData("Günaydın")]
    public async Task SelamlamaMesajlari_SelamYanitiDoner(string message)
    {
        var response = await _service.GetResponseAsync(message);

        Assert.Equal("Merhaba, size nasıl yardımcı olabilirim?", response);
    }

    [Fact]
    public async Task TesekkurMesaji_NazikYanitDoner()
    {
        var response = await _service.GetResponseAsync("Teşekkürler");

        Assert.Contains("Rica ederim", response);
    }

    [Fact]
    public async Task BilinmeyenMesaj_BosOlmayanYanitDoner()
    {
        var response = await _service.GetResponseAsync("xyz123");

        Assert.False(string.IsNullOrWhiteSpace(response));
    }

    [Fact]
    public async Task BosMesaj_PatlamadanYanitDoner()
    {
        var response = await _service.GetResponseAsync(string.Empty);

        Assert.False(string.IsNullOrWhiteSpace(response));
    }
}
