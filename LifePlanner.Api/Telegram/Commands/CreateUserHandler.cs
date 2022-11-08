using LifePlanner.Api.Domain;
using LifePlanner.Api.Store;

namespace LifePlanner.Api.Telegram.Commands;

public class CreateUserHandler : ICommandHandler
{
    private readonly long _chatId;
    private readonly IUserStore _userStore;
    private readonly ITelegramService _telegramService;
    private CreateUserState? _state = null;
    private User _user;

    public CreateUserHandler(long chatId, IUserStore userStore, ITelegramService telegramService)
    {
        _chatId = chatId;
        _userStore = userStore;
        _telegramService = telegramService;
    }

    public async Task<bool> Execute(string message)
    {
        if (await _userStore.GetByIdAsync(_chatId) is not null)
        {
            await _telegramService.SendMessage(_chatId, "Es existiert bereits ein User zu ihrer ChatId");
            return true;
        }
        switch (_state)
        {
            case null:
                await _telegramService.SendMessage(_chatId, "Vornamen eingeben");
                _state = CreateUserState.SetFirstname;
                return false;
            case CreateUserState.SetFirstname:
                _user = new User
                {
                    Firstname = message
                };
                await _telegramService.SendMessage(_chatId, "Nachnamen eingeben");
                _state = CreateUserState.SetLastname;
                return false;
            case CreateUserState.SetLastname:
                _user.Id = _chatId;
                _user.Lastname = message;
                await _userStore.CreateAsync(_user);
                return true;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}

internal enum CreateUserState
{
    SetFirstname,
    SetLastname
}