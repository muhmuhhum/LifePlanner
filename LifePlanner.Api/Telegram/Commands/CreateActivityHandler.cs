using System.Text.Json;
using LifePlanner.Api.Domain;
using LifePlanner.Api.Store;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace LifePlanner.Api.Telegram.Commands;

public class CreateActivityHandler : ICommandHandler
{
    public long ChatId { get; }
    public CreateStateEnum? State { get; set; }
    private Activity _activity;
    private readonly IActivityStore _activityStore;
    private readonly ITelegramService _telegramService;
    private readonly IUserStore _userStore;

    public CreateActivityHandler(long chatId, IActivityStore activityStore, IUserStore userStore, ITelegramService telegramService)
    {
        ChatId = chatId;
        _activityStore = activityStore;
        _userStore = userStore;
        _telegramService = telegramService;
    }

    public async Task<bool> Execute(string message)
    {
        switch (State)
        {
            case null:
                await _telegramService.SendMessage(ChatId, "Wie soll die Aktivität heißen");
                State = CreateStateEnum.SetName;
                break;
            case CreateStateEnum.SetName:
                _activity = new Activity
                {
                    Name = message
                };
                await _telegramService.SendMessage(ChatId, "Interval eingeben");
                State = CreateStateEnum.SetInterval;
                break;
            case CreateStateEnum.SetInterval:
                ExecutionInterval output;
                if (!ExecutionInterval.TryParse(message, true,  out output))
                {
                    await _telegramService.SendMessage(ChatId, "Ungültiges interval");
                    return false;
                }

                _activity.Interval = output;
                await _telegramService.SendMessage(ChatId, "Startdatum eingeben");
                State = CreateStateEnum.SetStartingDate;
                break;
            case CreateStateEnum.SetStartingDate:
                DateTime startDateOutput;
                if (!DateTime.TryParse(message, out startDateOutput))
                {
                    await _telegramService.SendMessage(ChatId, "Ungültiges interval");
                    return false;
                }

                if (startDateOutput < DateTime.Now)
                {
                    await _telegramService.SendMessage(ChatId, "Datum nach heute eingeben");
                    return false;
                }

                var universalTime = startDateOutput.ToUniversalTime();
                _activity.StartDate = universalTime;
                await _telegramService.SendMessage(ChatId, JsonSerializer.Serialize(_activity));
                await _activityStore.CreateAsync(_activity);
                return true;
            default:
                throw new ArgumentOutOfRangeException();
        }

        return false;
    }
}

public enum CreateStateEnum
{
    SetName,
    SetInterval,
    SetStartingDate
}