using System.Text.Json;
using System.Text;
using PromoRandom.Models;


namespace PromoRandom.Services;

public class WinnerNotificationService(HttpClient httpClient, ILogger<WinnerNotificationService> logger, IConfiguration config)
{
    private readonly HttpClient _httpClient = httpClient;
    private readonly ILogger<WinnerNotificationService> _logger = logger;
    private readonly string _botToken = config["Telegram:BotToken"]!;

    public async Task<bool> SendBroadcastMessageAsync(long chatId, string message, string? photoUrl = null)
    {
        try
        {
            var url = $"https://api.telegram.org/bot{_botToken}/";
            var method = "sendMessage";
            object payload;

            if (!string.IsNullOrEmpty(photoUrl))
            {
                method = "sendPhoto";
                payload = new
                {
                    chat_id = chatId,
                    photo = photoUrl,
                    caption = message,
                    parse_mode = "HTML"
                };
            }
            else
            {
                payload = new
                {
                    chat_id = chatId,
                    text = message,
                    parse_mode = "HTML"
                };
            }

            var response = await _httpClient.PostAsJsonAsync(url + method, payload);

            if (!response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("❌ Не удалось отправить сообщение в Telegram chat_id {ChatId}. Ответ: {Response}",
                    chatId, responseContent);
                return false;
            }

            _logger.LogInformation("✅ Сообщение успешно отправлено в Telegram chat_id {ChatId}", chatId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при отправке сообщения");
            return false;
        }
    }

    public async Task SendWinnerMessageAsync(long chatId, string languageCode, string prize)
    {
        var message = GetLocalizedWinnerMessage(languageCode, prize);

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

    private static string GetLocalizedWinnerMessage(string lang, string prize) => lang switch
    {
        "ru" =>
            $"🎉 Поздравляем! Вы стали победителем нашей акции и выиграли приз!\n" +
            $"Ваш приз: {prize}\n\n" +
            $"📍 Забрать его можно по адресу: город Душанбе, рядом с проходом Саховат. Ориентир – столовая \"Ҳочиён\", магазин \"Имкон\".\n" +
            $"📞 Чтобы получить подарок, позвоните по номеру: +992004048322.",

        "uz" =>
            $"🎉 Tabriklaymiz! Siz bizning aksiyamiz g‘olibiga aylandingiz!\n" +
            $"Yutug'ingiz: {prize}\n\n" +
            $"📍 Sovg'ani quyidagi manzildan olishingiz mumkin: Dushanbe shahri, Saxovat yo‘lagi yonida. Mo‘ljal – \"Ҳочиён\" oshxonasi, \"Imkon\" do‘koni.\n" +
            $"📞 Sovg'angizni olish uchun ushbu raqamga qo‘ng‘iroq qiling: +992004048322.",

        "tj" =>
            $"🎉 Табрик! Шумо дар озмуни мо ғолиб гардидед!\n" +
            $"Мукофоти шумо: {prize}\n\n" +
            $"📍 Шумо метавонед туҳфаи худро аз ин суроға гиред: шаҳри Душанбе, наздикии гузаргоҳи Саховат. Ориентир – ошхонаи \"Ҳочиён\", мағозаи \"Имкон\".\n" +
            $"📞 Барои гирифтани туҳфа, ба ин рақам занг занед: +992004048322.",

        "en" =>
            $"🎉 Congratulations! You are the winner of our campaign!\n" +
            $"Your prize: {prize}\n\n" +
            $"📍 You can collect it at: Dushanbe city, near the Sakhovat passage. Landmark – \"Ҳочиён\" cafeteria, \"Imkon\" store.\n" +
            $"📞 To receive your gift, please call: +992004048322.",

        _ =>
            $"🎉 Congratulations! You are the winner of our campaign!\n" +
            $"Your prize: {prize}\n\n" +
            $"📍 You can collect it at: Dushanbe city, near the Sakhovat passage. Landmark – \"Ҳочиён\" cafeteria, \"Imkon\" store.\n" +
            $"📞 To receive your gift, please call: +992004048322."
    };

    public async Task SendAdminNotificationAsync(long chatId, string message)
    {
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
            _logger.LogWarning("❌ Не удалось отправить сообщение админу chat_id {ChatId}. Ответ: {Response}",
                chatId, await response.Content.ReadAsStringAsync());
        }
        else
        {
            _logger.LogInformation("✅ Сообщение успешно отправлено админу chat_id {ChatId}", chatId);
        }
    }
}