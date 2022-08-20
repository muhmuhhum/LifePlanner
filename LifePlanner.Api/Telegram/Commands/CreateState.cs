using System.Text.Json;
using LifePlanner.Api.Domain;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace LifePlanner.Api.Telegram.Commands;

public class CreateState : ICommandState
{
    public long ChatId { get; set; }
    public CreateStateEnum? State { get; set; }
    private Activity _activity;
    private readonly IActivityManager _activityManager;
    

    private readonly TelegramBotClient _client;
    
    public CreateState(long chatId, TelegramBotClient client, IActivityManager activityManager)
    {
        ChatId = chatId;
        _client = client;
        _activityManager = activityManager;
    }

    public async Task<bool> Execute(string message)
    {
        switch (State)
        {
            case null:
                await TelegramInteractions.SendMessage(ChatId, "Wie soll die Aktivität heißen", _client);
                State = CreateStateEnum.SetName;
                break;
            case CreateStateEnum.SetName:
                _activity = new Activity
                {
                    Name = message
                };
                await TelegramInteractions.SendMessage(ChatId, "Interval eingeben", _client);
                State = CreateStateEnum.SetInterval;
                break;
            case CreateStateEnum.SetInterval:
                ExecutionInterval output;
                if (!ExecutionInterval.TryParse(message, true,  out output))
                {
                    await TelegramInteractions.SendMessage(ChatId, "Ungültiges interval", _client);
                    return false;
                }

                _activity.Interval = output;
                await TelegramInteractions.SendMessage(ChatId, "Startdatum eingeben", _client);
                State = CreateStateEnum.SetStartingDate;
                break;
            case CreateStateEnum.SetStartingDate:
                DateTime startDateOutput;
                if (!DateTime.TryParse(message, out startDateOutput))
                {
                    await TelegramInteractions.SendMessage(ChatId, "Ungültiges interval", _client);
                    return false;
                }

                if (startDateOutput < DateTime.Now)
                {
                    await TelegramInteractions.SendMessage(ChatId, "Datum nach heute eingeben", _client);
                    return false;
                }

                var universalTime = startDateOutput.ToUniversalTime();
                _activity.StartDate = universalTime;
                await TelegramInteractions.SendMessage(ChatId, JsonSerializer.Serialize(_activity), _client);
                await _activityManager.CreateAsync(_activity);
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