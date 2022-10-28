using ContainerStore.Common.DTO;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TraderBot.Notifier;
using Strategies;
using Strategies.DTO;
using MongoDbSettings;
using ZstdSharp.Unsafe;

namespace ContainerStore.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ContainersController : ControllerBase
{
    private readonly Notifier _logger;
    private readonly StrategyService _strategyService;

    public ContainersController(Notifier logger, StrategyService strategyService)
    {
        _logger = logger;
        _strategyService = strategyService;
    }

    [HttpGet]
    public async Task<List<MainStrategy>> Get() => await _strategyService.GetAsync();

    [HttpGet("{id:length(24)}")]
    public async Task<ActionResult<MainStrategy>> Get(string id)
    {
        var strategy = await _strategyService.GetAsync(id);
        if (strategy is null)
        {
            return NotFound();
        }
        return strategy;
    }

    [HttpPost]
    public async Task<IActionResult> Post(MainStrategy newStrategy)
    {
        await _strategyService.CreateAsync(newStrategy);
        return CreatedAtAction(nameof(Get), new { id = newStrategy.Id }, newStrategy);
    }

    [HttpPut("{id:length(24)}")]
    public async Task<IActionResult> Update(string id, MainStrategyDTO updates)
    {
        var strategy = await _strategyService.GetAsync(id);
        if (strategy is null)
        {
            return NotFound();
        }
        if (updates.MainSettings != null)
        {
            strategy.MainSettings = updates.MainSettings;
        }
        if (updates.StraddleSettings != null)
        {
            strategy.StraddleSettings = updates.StraddleSettings;
        }
        if (updates.ClosureSettings != null)
        {
            strategy.ClosureSettings = updates.ClosureSettings;
        }

        await _strategyService.UpdateAsync(id, strategy);
        return NoContent();
    }

    [HttpDelete("{id:length(24)}")]
    public async Task<IActionResult> Delete(string id)
    {
        var book = await _strategyService.GetAsync(id);
        if (book is null)
        {
            return NotFound();
        }
        await _strategyService.RemoveAsync(id);
        return NoContent();
    }
}
