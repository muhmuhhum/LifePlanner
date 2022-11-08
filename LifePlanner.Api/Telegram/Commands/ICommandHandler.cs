namespace LifePlanner.Api.Telegram.Commands;

public interface ICommandHandler
{
    public Task<bool> Execute(string message);
}