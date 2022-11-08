using LifePlanner.Api.Store;
using LifePlanner.Api.Telegram.Commands;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace LifePlanner.Api.Telegram;

public class TelegramService : ITelegramService
{
    private TelegramBotClient _client = null!;
    private readonly IConfiguration _configuration;
    private readonly IServiceProvider _serviceProvider;
    private readonly Dictionary<ChatId, ICommandHandler> _states = new();
    private bool _isInitialized = false;
    private readonly List<TelegramBotCommand> _commands = new()
    {
        new()
        {
            Name = "create",
            Description = "Neue aktivität anlegen",
            CreateNewState = (id, activityStore, userStore, service) => new CreateActivityHandler(id, activityStore, userStore, service)
        },
        new()
        {
            Name = "newuser",
            Description = "Neuen User anlegen",
            CreateNewState = (id, activityStore, userStore, service) => new CreateUserHandler(id, userStore, service)
        }
    };
    
    public TelegramService(IConfiguration configuration, IServiceProvider serviceProvider)
    {
        _configuration = configuration;
        _serviceProvider = serviceProvider;
    }

    public async Task Init()
    {
        if (_isInitialized)
        {
            return;
        }
        _client = new TelegramBotClient(_configuration["Telegram:Token"]);
        var botCommands = new List<BotCommand>();
        foreach (var command in _commands)
        {
            botCommands.Add(new BotCommand
            {
                Command = command.Name,
                Description = command.Description
            });
        }
        await _client.SetMyCommandsAsync(botCommands, BotCommandScope.Default(), "de", cancellationToken: CancellationToken.None);
        var receiverOptions = new ReceiverOptions
        {
            AllowedUpdates = Array.Empty<UpdateType>() // receive all update types
        };
        _client.StartReceiving(updateHandler: HandleUpdateAsync, pollingErrorHandler: HandlePollingErrorAsync, receiverOptions, CancellationToken.None);
        _isInitialized = true;
    }

    private Task HandlePollingErrorAsync(ITelegramBotClient client, Exception exception, CancellationToken cancellationToken)
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

    private async Task HandleUpdateAsync(ITelegramBotClient client, Update update, CancellationToken cancellationToken)
    {
        var scope = _serviceProvider.CreateScope();
        var activityStore = scope.ServiceProvider.GetService<IActivityStore>()!;
        var userStore = scope.ServiceProvider.GetService<IUserStore>()!;
        if (update.Message is not { Text: { } messageText } message)
            return;
        
        var chatId = message.Chat.Id;

        var user = await userStore.GetByIdAsync(chatId);

        if (user is null 
            && message.Text != "/newuser" 
            && _states.GetValueOrDefault(chatId)?.GetType() != typeof(CreateUserHandler)
        )
        {
            await SendMessage(chatId, "Bitte legen sie einen User über das /newuser command an");
            return;
        }
        
        Console.WriteLine($"Received a '{messageText}' message in chat {chatId}.");

        if (message.Text.StartsWith("/"))
        {
            var commandName = message.Text.Substring(1, message.Text.Length-1);
            var selectedCommand = _commands.FirstOrDefault(x => x.Name == commandName);
            if (selectedCommand is null)
            {
                await SendMessage(chatId, "Ungültiges command");
                return;
            }

            var state = selectedCommand.CreateNewState.Invoke(chatId, activityStore, userStore, this);
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

    public async Task SendMessage(long chatId, string message)
    {
        await _client.SendTextMessageAsync(
            chatId: chatId,
            text: message,
            cancellationToken: CancellationToken.None);
    }
}