using Connectors;
using Microsoft.AspNetCore.Mvc;
using Notifier;
using OptionTraderWebGui.SignalParsers;
using System.Text;
using Traders;
using Traders.Base;

namespace OptionTraderWebGui.Controllers;

[Route("api/[controller]")]
[ApiController]
public class SignalController : ControllerBase
{
    private readonly IConnector _connector;
    private readonly IBffLogger _logger;
    private readonly ContainerTrader _trader;

    public SignalController(IConnector connector, IBffLogger logger, ContainerTrader trader)
    {
        _connector = connector;
        _logger = logger;
        _trader = trader;
    }

    /// <summary>
    /// Принимает сигнал от внешнего источника (чаще всего это Мультичарт) и, в зависимости от сигнала,
    /// производит всякие манипуляции.
    /// </summary>
    /// <param name="symbol">Инструмент, точней его полное имя.</param>
    /// <param name="direction">1 - лонг. 0 - флэт. -1 - шорт.</param>
    /// <param name="price">Тут все просто. Цена на которой произошел вход - выход.(для выхода 0)</param>
    /// <param name="account">Тут еще все проще. Торгуемый аккаунт.</param>
    /// <returns></returns>
    [HttpGet]
    public IActionResult Get(string symbol, int direction, double price, string account)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"SIGNAL: {symbol}::{account}::{price}::{direction}");

        var container = _trader.GetContainer(symbol, account);
        if (container is null)
        {
            sb.AppendLine("No container in trade.");
            _logger.LogInformation(sb.ToString(), toTelegram: true);
            return Ok();
        }

        sb.AppendLine(SpreadSignalParser.ParseSignal(direction, price, container, _connector, _logger));
        sb.AppendLine($"Account: {account}. Price: {price}.");
        _logger.LogInformation(sb.ToString(), toTelegram: true);
        return Ok();
    }
}
