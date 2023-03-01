namespace BffLogger;

using System.Threading;
using Telegram.Bot;

public class TelegramLogger
{
    private CancellationTokenSource _cts = new();
    private readonly string _token = "5492257084:AAFwlIkEAvuHObH2VSExos5UpP5ZSbG7yu8";
    private readonly TelegramBotClient _client;

    public TelegramLogger() => _client = new(_token);
    public void SendMessage(string msg) =>
        _client.SendTextMessageAsync(-1001688422300, msg);
}
