using System.Threading;
using System;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Extensions.Polling;

namespace Notifier;
internal class TelegramNotifier
{
    private CancellationTokenSource _cts = new();
    private static string _token = "5492257084:AAFwlIkEAvuHObH2VSExos5UpP5ZSbG7yu8";
    private TelegramBotClient _client;
    public TelegramNotifier()
    {
        _client = new(token: _token);
        _client.StartReceiving(HandleUpdateAsync,
            HandleErrorAsync,
            new ReceiverOptions { AllowedUpdates = { } },
            _cts.Token);
    }
    public async Task<User> GetBotAsync() => await _client.GetMeAsync();
    public async void SendMessageAsync(string msg)
    {
        await _client.SendTextMessageAsync(-1001688422300, msg);
    }

    protected async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        var handler = update.Type switch
        {
            _ => UnknownUpdateHandlerAsync(botClient, update)
        };

        try
        {
            await handler;
        }
        catch (Exception exception)
        {
            await HandleErrorAsync(botClient, exception, cancellationToken);
        }
    }
    protected Task MessageRecievedAsync(Message msg)
    {
        Console.WriteLine($"Message recieved: {msg}");
        return Task.CompletedTask;
    }
    protected Task UnknownUpdateHandlerAsync(ITelegramBotClient botClient, Update update)
    {
        Console.WriteLine($"Unknown update type:\n" +
            $"Type:\t{update.Type}");
        return Task.CompletedTask;
    }

    protected static Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        var ErrorMessage = exception switch
        {
            ApiRequestException apiRequestException => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
            _ => exception.ToString()
        };

        Console.WriteLine(ErrorMessage);
        return Task.CompletedTask;
    }
}
//Id	-1001688422300	long
