using AtlasChatApi.Services;

var builder = WebApplication.CreateBuilder(args);

// MVC controllers (POST /api/chat ChatController içinde tanımlıdır).
builder.Services.AddControllers();

// Swagger / OpenAPI: API'yi tarayıcıdan elle test edebilmek için.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Sohbet sağlayıcısı konfigürasyondan seçilir (appsettings.json -> Chat:Provider).
//   "Ollama"  -> lokal/ücretsiz LLM üzerinden yanıt üretir.
//   diğer/boş -> harici bağımlılığı olmayan kural tabanlı yanıt üreticisi (varsayılan).
var provider = builder.Configuration["Chat:Provider"];
if (string.Equals(provider, "Ollama", StringComparison.OrdinalIgnoreCase))
{
    builder.Services.AddHttpClient<IChatService, OllamaChatService>();
}
else
{
    builder.Services.AddSingleton<IChatService, RuleBasedChatService>();
}

var app = builder.Build();

// Swagger'ı yalnızca geliştirme ortamında aç.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();

app.Run();
