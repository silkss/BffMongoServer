using Connectors;
using Microsoft.AspNetCore.Mvc;
using Notifier;
using System.Text;
using Traders.Base;
using WebApi.SignalParsers;

namespace WebApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class SignalController : ControllerBase
{
    private readonly IConnector _connector;
    private readonly IBffLogger _logger;
    private readonly Trader _trader;

    public SignalController(IConnector connector, IBffLogger logger, Trader trader)
    {
        _connector = connector;
        _logger = logger;
        _trader = trader;
    }

    [HttpGet]
    public IActionResult Get(string symbol, int direction, double price, string account)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"SIGNAL: {symbol}-{account}-{direction}");

        var strategy = _trader.GetStrategy(symbol, account);
        if (strategy is null)
        {
            sb.AppendLine("No container in trade.");
            _logger.LogInformation(sb.ToString(), toTelegram: true);
            return Ok();
        }

        sb.AppendLine(LongStraddleSignalParser.ParseSignal(direction, price, strategy, _connector, _logger));
        sb.AppendLine($"Account: {account}. Price: {price}.");
        return Ok();
    }
}
