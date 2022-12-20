using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Notifier;
using Strategies.DTO;
using MongoDbSettings;
using System.Linq;
using Strategies.Strategies;

namespace WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ContainersController : ControllerBase
{
    private readonly IBffLogger _logger;
    private readonly StrategyService _strategyService;

    public ContainersController(IBffLogger logger, StrategyService strategyService)
    {
        _logger = logger;
        _strategyService = strategyService;
    }

    [HttpGet]
    public async Task<IEnumerable<MainStrategyDTO>> Get()
    {
        var strategies = await _strategyService.GetAsync();
        return strategies.Select(s => s.ToDto()); 
    }
    [HttpGet("admin/")]
    public async Task<IEnumerable<MainStrategy>> AdminGet() => await _strategyService.GetAsync();

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
    public async Task<IActionResult> Create(MainStrategyDTO newStrategyDto)
    {
        if (newStrategyDto.Instrument == null ||
            newStrategyDto.MainSettings == null ||
            newStrategyDto.ClosureSettings == null ||
            newStrategyDto.StraddleSettings == null )
        { 
            return BadRequest();
        }
        var newStrategy = new MainStrategy
        {
            Instrument = newStrategyDto.Instrument,
            MainSettings = newStrategyDto.MainSettings,
            ClosureSettings = newStrategyDto.ClosureSettings,
            StraddleSettings = newStrategyDto.StraddleSettings
        };

        await _strategyService.CreateAsync(newStrategy);
        return CreatedAtAction(nameof(Get), new { id = newStrategy.Id }, newStrategy);
    }

    [HttpPut("{id:length(24)}")]
    public async Task<IActionResult> Update(string id, MainStrategySettingsDTO updates)
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

    [HttpPut("admin/{id:length(24)}")]
    public async Task<IActionResult> Update(string id, MainStrategy source)
    {
        var target = await _strategyService.GetAsync(id);
        if (target == null) return NotFound();
        await _strategyService.UpdateAsync(id, source);
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
