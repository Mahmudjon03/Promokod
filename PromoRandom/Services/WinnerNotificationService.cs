using System.Text.Json;
using System.Text;


namespace PromoRandom.Services
{
    public class WinnerNotificationService(HttpClient httpClient, ILogger<WinnerNotificationService> logger, IConfiguration config)
    {
        private readonly HttpClient _httpClient = httpClient;
        private readonly ILogger<WinnerNotificationService> _logger = logger;
        private readonly string _botToken = config["Telegram:BotToken"]!;

        public async Task SendWinnerMessageAsync(long chatId, string languageCode)
        {
            var message = GetLocalizedWinnerMessage(languageCode);

            var url = $"https://api.telegram.org/bot{_botToken}/sendMessage";

            var payload = new
            {
                chat_id = chatId,
                text = message
            };

            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(url, content);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("❌ Не удалось отправить сообщение в Telegram chat_id {ChatId}. Ответ: {Response}",
                    chatId, await response.Content.ReadAsStringAsync());
            }
            else
            {
                _logger.LogInformation("✅ Сообщение успешно отправлено в Telegram chat_id {ChatId}", chatId);
            }
        }

        private string GetLocalizedWinnerMessage(string lang) => lang switch
        {
            "ru" => "🎉 Поздравляем! Вы выиграли приз в нашей акции!",
            "uz" => "🎉 Tabriklaymiz! Siz bizning aksiyamizda g'olib bo'ldingiz!",
            "tj" => "🎉 Табрик! Шумо дар озмуни мо ғолиб шудед!",
            "en" => "🎉 Congratulations! You won a prize in our campaign!",
            _ => "🎉 Congratulations! You won a prize in our campaign!"
        };
    }
}