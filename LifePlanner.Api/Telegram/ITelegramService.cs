namespace LifePlanner.Api.Telegram;

public interface ITelegramService
{
    public Task Init();
    public Task SendMessage(long chatId, string message);
}