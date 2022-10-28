using ContainerStore.Traders.Base;
using Microsoft.AspNetCore.Mvc;
using MongoDbSettings;
using Strategies;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ContainerStore.WebApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class TraderController : ControllerBase
{
	private readonly Trader _trader;
    private readonly StrategyService _strategyService;

	public TraderController(Trader trader, StrategyService strategyService)
	{
		_trader = trader;
        _strategyService = strategyService;
	}
    [HttpGet]
    public IEnumerable<MainStrategy> Get() => _trader.GetStrategies();

    [HttpPost("{id:length(24)}")]
    public async Task<IActionResult> Start(string id)
    {
        var strategy = await _strategyService.GetAsync(id);
        if (strategy is null)
        {
            return NotFound();
        }
        var result = _trader.AddToTrade(strategy);
        if (!result)
        {
            return BadRequest();
        }
        return Ok();
    }

    [HttpDelete("{id:length(24)}")]
    public async Task<IActionResult> Stop(string id) => await _trader.StopContainerAsync(id) ? Ok() : NotFound();
}
