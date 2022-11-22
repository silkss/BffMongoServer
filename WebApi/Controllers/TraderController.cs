using Traders.Base;
using Microsoft.AspNetCore.Mvc;
using MongoDbSettings;
using Strategies;
using Strategies.DTO;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApi.Controllers;

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
    public IEnumerable<MainStrategyDTO> Get()
    {
        var strategies = _trader.GetStrategies();
        return strategies.Select(s => s.ToDto()); 
    }
    [HttpGet("{id:length(24)}")]
    public MainStrategyDTO? Get(string id) => _trader.GetStrategyById(id)?.ToDto();

    [HttpGet("admin/")]
    public IEnumerable<MainStrategy> AdminGet() => _trader.GetStrategies();

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
