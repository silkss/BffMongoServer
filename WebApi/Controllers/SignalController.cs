using Microsoft.AspNetCore.Mvc;
using Notifier;
using System;

namespace WebApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class SignalController : ControllerBase
{
    private readonly IBffLogger _logger;

    public SignalController(IBffLogger logger)
    {
        _logger = logger;
    }

    [HttpGet]
    public IActionResult Get(string symbol, int direction, double price, string account)
    {
        _logger.LogInformation($"{DateTime.Now}. {symbol} {direction} {price} {account}", toTelegram: true);
        return Ok();
    }
}
