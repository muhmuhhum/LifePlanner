using LifePlanner.Api.Telegram.Commands;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace LifePlanner.Api.Telegram;

public class TelegramBotCommand
{
    public string Name { get; set; }
    public string Description { get; set; }

    public Create CreateNewState { get; set; }

    public delegate ICommandState Create(long chatId, TelegramBotClient client, IActivityManager manager);

}