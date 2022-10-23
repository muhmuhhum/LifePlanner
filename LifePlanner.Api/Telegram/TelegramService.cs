using LifePlanner.Api.Stores;
using LifePlanner.Api.Telegram.Commands;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace LifePlanner.Api.Telegram;

public class TelegramService : ITelegramService
{
    private readonly ILogger<TelegramService> _logger;
    private readonly IConfiguration _configuration;
    private TelegramBotClient? _client;
    private readonly IServiceProvider _serviceProvider;
    
    private readonly List<TelegramBotCommand> _commands;
    private Dictionary<ChatId, ICommandState> _states = new();

    public TelegramService(ILogger<TelegramService> logger, IConfiguration configuration, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _configuration = configuration;
        _serviceProvider = serviceProvider;
        _commands = new List<TelegramBotCommand>
        {
            new(
                name:"create",
                description:"Neue aktivität anlegen", 
                commandStateType:typeof(CreateCommandState)
            ),
            new(
                name: "register",
                description:"User erstellen",
                commandStateType: typeof(RegisterCommandState)
            )
        };
        Init();
    }

    public void Init()
    {
        _client = new TelegramBotClient(_configuration["Telegram:Token"]);
        var receiverOptions = new ReceiverOptions
        {
            AllowedUpdates = Array.Empty<UpdateType>() // receive all update types
        };
        var botCommands = new List<BotCommand>();
        foreach (var command in _commands)
        {
            botCommands.Add(new BotCommand
            {
                Command = command.Name,
                Description = command.Description
            });
        }
        _client.StartReceiving(updateHandler: HandleUpdateAsync, pollingErrorHandler: HandlePollingErrorAsync, receiverOptions, CancellationToken.None);
    }
    
    public async Task SendMessage(long chatId, string message)
    {
        await _client.SendTextMessageAsync(
            chatId: chatId,
            text: message,
            cancellationToken: CancellationToken.None);
    }

    private async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        var scope = _serviceProvider.CreateScope();
        var userStore = scope.ServiceProvider.GetService<IUserStore>()!;
        if (update.Message is not { Text: { } messageText } message)
            return;
        
        var chatId = message.Chat.Id;

        Console.WriteLine($"Received a '{messageText}' message in chat {chatId}.");

        var user = await userStore.GetUserByChatId(chatId);

        if (user is null && 
            (!_states.ContainsKey(chatId) || _states[chatId].GetType() != typeof(RegisterCommandState)))
        {
            _states[chatId] = scope.ServiceProvider.GetService<RegisterCommandState>()!;
        }
        else
        {
            if (message.Text.StartsWith("/"))
            {
                var commandName = message.Text.Substring(1, message.Text.Length-1);
                var selectedCommand = _commands.FirstOrDefault(x => x.Name == commandName);
                if (selectedCommand is null)
                {
                    await SendMessage(chatId, "Ungültiges command");
                    return;
                }
            
                var command = (ICommandState)scope.ServiceProvider.GetService(selectedCommand.CommandStateType)!;
                _states[chatId] = command;
            }
        }
        
        if (_states.ContainsKey(chatId))
        {
            var isFinished = await _states[chatId].Execute(message.Text, chatId);
            if (isFinished)
            {
                _states.Remove(chatId);
            }
        }
    }

    static Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
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