# Atlas Chat API

Atlas Yazılım için hazırladığım kısa değerlendirme çalışması. .NET 8 ile yazılmış basit bir
Web API var (`POST /api/chat`), bir de istenen 5 sorunun cevaplarını aşağıya yazdım.

Amacım dev bir proje yapmak değildi; küçük ama düzgün, kurcak olan kişinin başına iş açmadan
çalışan bir şey bırakmaktı. Yanıt üretme kısmını bir arayüzün (`IChatService`) arkasına aldım,
böylece "sabit cevap" yerine yapay zekaya geçmek tek satır ayar meselesi oluyor.

## Çalıştırma

.NET 8 SDK kuruluysa proje klasöründe:

```bash
dotnet run --project src/AtlasChatApi
```

Uygulama `http://localhost:5080` adresinde açılıyor. Test için en kolayı tarayıcıdan
`http://localhost:5080/swagger` adresine gidip denemek.

## Endpoint

Tek bir endpoint var:

```
POST /api/chat
```

İstek:

```json
{ "message": "Merhaba" }
```

Cevap:

```json
{ "response": "Merhaba, size nasıl yardımcı olabilirim?" }
```

`message` boş gelirse ya da 1000 karakteri geçerse 400 dönüyor.

Komut satırından denemek isteyen olursa:

```bash
curl -X POST http://localhost:5080/api/chat \
  -H "Content-Type: application/json" \
  -d '{"message":"Merhaba"}'
```

## Cevap nasıl üretiliyor?

Şu an varsayılan olarak basit bir kural tabanlı servis çalışıyor (`RuleBasedChatService`):
gelen mesajda "merhaba", "teşekkür", "fiyat" gibi birkaç anahtar kelimeye bakıp uygun bir
cevap dönüyor, tanımadığı bir şey gelirse genel bir cevap veriyor. Hiçbir kurulum gerektirmediği
için projeyi klonlayan herkeste direkt çalışıyor.

İsteyen gerçek bir yapay zeka modeline bağlayabilsin diye `OllamaChatService` da yazdım. Lokal
ve ücretsiz çalışan [Ollama](https://ollama.com) üzerinden cevap üretiyor. Açmak için önce bir
model indirip:

```bash
ollama pull llama3.2
```

sonra `appsettings.json` içinde sağlayıcıyı değiştirmek yeterli:

```json
"Chat": {
  "Provider": "Ollama"
}
```

Ollama o an ayakta değilse uygulama patlamıyor, sessizce kural tabanlı cevaba düşüyor.

## Proje yapısı

```
src/AtlasChatApi/
  Program.cs                  -> servis kayıtları, hangi sağlayıcının kullanılacağı
  Controllers/ChatController  -> POST /api/chat
  Models/                     -> ChatRequest, ChatResponse
  Services/                   -> IChatService + iki uygulaması (kural tabanlı, Ollama)
tests/AtlasChatApi.Tests/     -> birkaç xUnit testi
```

Test çalıştırmak için: `dotnet test`

---

# Dokümantasyon Soruları

## 1. Asterisk Entegrasyonu

Buradaki mantık şu: Asterisk telefon tarafını ve sesi yönetir, bu API ise sadece "ne cevap
vereyim" kısmını halleder. İkisini birbirine bağlayan bir ara katman lazım.

Çağrı geldiğinde Asterisk dialplan'de (`extensions.conf`) `Answer()` ile çağrıyı açar. Ben
modern tarafı, yani **ARI**'yi (Asterisk REST Interface) tercih ederim; dialplan'i `Stasis`
uygulamasına yönlendirip kontrolü kendi .NET köprü servisime devrederim. Eski usul **AGI** de
iş görür ama ARI daha esnek.

API'yi Asterisk'in kendisi çağırmaz; arada yazdığım köprü servisi çağırır. Akış kabaca şöyle
işler: kullanıcının sesi kaydedilir → STT ile metne çevrilir → bu metin `HttpClient` ile
`POST /api/chat`'e gönderilir → dönen cevap TTS ile sese çevrilir → ARI üzerinden çağrıya
oynatılır. Sonra tekrar dinlemeye geçilir.

Sesi oynatmak için ARI tarafında `play` komutu (`POST /channels/{id}/play`), klasik dialplan
kullanıyorsam `Playback` yeterli. Telefon hattı 8kHz olduğu için TTS çıktısını o formata
çevirmek gerekir.

## 2. STT (Speech To Text)

Lokal kalmak istersem **Whisper**, özellikle daha hızlı olan `faster-whisper` sürümünü
kullanırdım. Türkçesi gayet iyi, çok dilli eğitildiği için aksanlı/günlük konuşmada da fena
sonuç vermiyor. En önemlisi kendi sunucuda çalıştığı için çağrı kayıtları dışarı çıkmıyor;
çağrı merkezi tarafında bu ciddi bir konu. Maliyet de sabit, dakika başı ücret yok.

Eğer gerçek zamanlı (kullanıcı konuşurken anlık çeviri, araya girince durdurma gibi) bir şey
gerekiyorsa ve operasyon yüküyle uğraşmak istemiyorsam Azure ya da Google'ın Speech-to-Text
servisleri hazır geliyor, orada streaming ve noktalama gibi şeyler kutudan çıkıyor. Yani seçim
biraz "gizlilik + maliyet mi, yoksa kolaylık + anlık mı" dengesine bakar.

## 3. TTS (Text To Speech)

Burada da lokal tarafta **Piper**'ı tercih ederim. CPU'da bile gerçek zamandan hızlı sentezliyor,
Türkçe sesleri var ve ücretsiz. IVR'da gecikme önemli olduğu için bu hız işe yarıyor.

Sesin mümkün olduğunca doğal, "robot gibi olmayan" olması öncelikse o zaman Azure Neural TTS
(ya da Google) daha iyi. Tonlama, SSML ile vurgu ayarlama gibi konularda bulut servisleri açık
ara önde. Karşılığında internet bağımlılığı ve karakter başı ücret var.

Pratikte sık tekrar eden anonsları ("hoş geldiniz", "lütfen bekleyin" gibi) baştan üretip
kaydetmek de iyi oluyor, her seferinde yeniden sentezlemeye gerek kalmıyor.

## 4. Yapay Zeka

Ücretsiz ve lokal çalışsın istediğim için **Ollama** üzerinden açık modelleri kullanırdım;
Llama 3.x ya da Türkçede iyi olan Qwen gibi. Sebebi basit: token başı ücret yok, veri dışarı
gitmiyor (KVKK açısından rahat) ve .NET'ten sadece bir HTTP isteğiyle konuşuluyor. Zaten bu
projede `OllamaChatService` olarak bağladım, denenebilir durumda.

Burada dikkat edilmesi gereken asıl konu donanım. Lokal model seçimi sunucunun ve özellikle
ekran kartının gücüne göre değişir; bunu baştan öngörmek lazım. Mesela iyi bir GPU'su (yeterli
VRAM) olan bir makinede Qwen 12B gibi büyük bir modeli rahatça çalıştırabilirsin ve kalite
belirgin şekilde artar. Ama aynı modeli düşük donanımlı bir PC'de çalıştıramazsın; ya çok yavaş
çalışır ya da hiç açılmaz. O yüzden zayıf donanımda 3B–7B aralığında daha küçük bir modele
inmek, hatta nicelenmiş (quantized) sürümleri tercih etmek gerekir. Yani "hangi model" sorusunun
cevabı tek başına değil, elimizdeki donanımla birlikte verilmeli.

Donanım gerçekten yetmiyorsa veya en yüksek kaliteyi şart koşan bir senaryo varsa aynı
`IChatService` arkasına bir bulut LLM eklemek tek sınıflık iş, mimari buna uygun. Yani lokal
başlayıp gerekirse buluta geçmek mümkün.

## 5. Test Süreci

Sıfırdan kuracak biri için:

**Projeyi çalıştırmak:** .NET 8 SDK'nın kurulu olduğundan emin ol (`dotnet --version`), sonra
`dotnet run --project src/AtlasChatApi`. Açılan adres terminalde yazıyor (`http://localhost:5080`).

**API'yi test etmek:** En rahatı Swagger; `http://localhost:5080/swagger` aç, `POST /api/chat`'i
seç, "Try it out" deyip `{ "message": "Merhaba" }` gönder. Alternatif olarak yukarıdaki curl
komutu ya da Postman da olur. Servis testleri için `dotnet test`.

**Asterisk ile test etmek:** Lokalde en pratiği Asterisk'i Docker'da ayağa kaldırmak. Sonra
`pjsip.conf`'ta bir SIP kullanıcısı tanımlayıp Zoiper / Linphone gibi bir softphone ile kayıt
oluyorsun, test numarasını arıyorsun. Tam STT/TTS kurmadan önce hızlı bir kontrol için dialplan'den
sabit bir metni API'ye gönderip dönen cevabı `Playback` ile oynatmak, entegrasyonun çalıştığını
görmek için yeterli.

**Kullandığım / kullanılabilecek araçlar:** geliştirme için VS Code veya Visual Studio; API
denemek için Swagger, curl, Postman; testler için xUnit; telefon tarafı için Asterisk + bir
softphone; STT/TTS tarafında Whisper ve Piper (ya da Azure/Google); yapay zeka için Ollama.

---

Not: Asterisk, STT ve TTS'in gerçek kodunu yazmadım çünkü soru bunları anlatmamı istiyordu, kod
beklemiyordu. API tarafını bunların rahatça bağlanabileceği şekilde sade tuttum. Yapay zeka kısmını
ise hem anlattım hem de çalışan kodla (`OllamaChatService`) gösterdim.
