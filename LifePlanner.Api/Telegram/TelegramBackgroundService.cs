using LifePlanner.Api.Telegram.Commands;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace LifePlanner.Api.Telegram;

public class TelegramBackgroundService : BackgroundService
{
    private TelegramBotClient _client;
    private IServiceProvider _serviceProvider;
    private IConfiguration _configuration;

    private List<TelegramBotCommand> _commands = new List<TelegramBotCommand>
    {
        new()
        {
            Name = "create",
            Description = "Neue aktivität anlegen",
            CreateNewState = (id, client, manager) => new CreateState(id, client, manager)
        }
    };

    private Dictionary<ChatId, ICommandState> _states = new();

    public TelegramBackgroundService(IConfiguration configuration, IServiceProvider serviceProvider)
    {
        _configuration = configuration;
        _serviceProvider = serviceProvider;
    }

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        _client = new TelegramBotClient(_configuration["Telegram:Token"]);
        Console.WriteLine("Started");
        List<BotCommand> botCommands = new List<BotCommand>();
        foreach (var command in _commands)
        {
            botCommands.Add(new BotCommand
            {
                Command = command.Name,
                Description = command.Description
            });
        }
        await _client.SetMyCommandsAsync(botCommands, BotCommandScope.Default(), "de", cancellationToken: cancellationToken);
        var receiverOptions = new ReceiverOptions
        {
            AllowedUpdates = Array.Empty<UpdateType>() // receive all update types
        };
        _client.StartReceiving(updateHandler: HandleUpdateAsync, pollingErrorHandler: HandlePollingErrorAsync, receiverOptions, CancellationToken.None);
        await base.StartAsync(cancellationToken);
    }
    
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Console.WriteLine("Execute called");
        return Task.CompletedTask;
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        Console.WriteLine("Stopped");
        return base.StopAsync(cancellationToken);
    }
    
    async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        var scope = _serviceProvider.CreateScope();
        var activityManager = scope.ServiceProvider.GetService<IActivityManager>();
        if (update.Message is not { Text: { } messageText } message)
            return;
        
        var chatId = message.Chat.Id;

        Console.WriteLine($"Received a '{messageText}' message in chat {chatId}.");

        if (message.Text.StartsWith("/"))
        {
            var commandName = message.Text.Substring(1, message.Text.Length-1);
            var selectedCommand = _commands.FirstOrDefault(x => x.Name == commandName);
            if (selectedCommand is null)
            {
                await TelegramInteractions.SendMessage(chatId, "Ungültiges command", _client);
                return;
            }

            var state = selectedCommand.CreateNewState.Invoke(chatId, _client, activityManager);
            _states[chatId] = state;
        }

        if (_states.ContainsKey(chatId))
        {
            var isFinished = await _states[chatId].Execute(message.Text);
            if (isFinished)
            {
                _states.Remove(chatId);
            }
        }
    }

    Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        var errorMessage = exception switch
        {
            ApiRequestException apiRequestException
                => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
            _ => exception.ToString()
        };

        Console.WriteLine(errorMessage);
        return Task.CompletedTask;
    }

}