using Telegram.Bot;

namespace LifePlanner.Api.Telegram;

public static class TelegramInteractions
{
    public static async Task SendMessage(long chatId, string message, TelegramBotClient client)
    {
        await client.SendTextMessageAsync(
            chatId: chatId,
            text: message,
            cancellationToken: CancellationToken.None);
    }
}