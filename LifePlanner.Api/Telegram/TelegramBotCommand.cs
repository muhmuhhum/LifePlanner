using LifePlanner.Api.Store;
using LifePlanner.Api.Telegram.Commands;

namespace LifePlanner.Api.Telegram;

public class TelegramBotCommand
{
    public string Name { get; set; }
    public string Description { get; set; }

    public Create CreateNewState { get; set; }

    public delegate ICommandHandler Create(long chatId, IActivityStore manager, IUserStore userStore, ITelegramService telegramService);

}