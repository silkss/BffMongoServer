using ContainerStore.Common.DTO;
using ContainerStore.Data.Models;
using ContainerStore.WebApi.Services;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TraderBot.Notifier;

namespace ContainerStore.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ContainersController : ControllerBase
{
    private readonly Notifier _logger;
    private readonly ContainersService _containersService;

    public ContainersController(Notifier logger, ContainersService containersService)
    {
        _logger = logger;
        _containersService = containersService;
    }

    [HttpGet]
    public async Task<List<Container>> Get() => await _containersService.GetAsync();

    [HttpGet("{id:length(24)}")]
    public async Task<ActionResult<Container>> Get(string id)
    {
        var book = await _containersService.GetAsync(id);
        if (book is null)
        {
            return NotFound();
        }
        return book;
    }

    [HttpPost]
    public async Task<IActionResult> Post(Container newContainer)
    {
        await _containersService.CreateAsync(newContainer);
        return CreatedAtAction(nameof(Get), new { id = newContainer.Id }, newContainer);
    }

    [HttpPut("{id:length(24)}")]
    public async Task<IActionResult> Update(string id, ContainerDTO updates)
    {
        var container = await _containersService.GetAsync(id);
        if (container is null)
        {
            return NotFound();
        }

        container.Account = updates.Account;
        container.StraddleExpirationDays = updates.StraddleExpirationDays;
        container.ClosurePriceGapProcent = updates.ClosurePriceGapProcent;
        container.ClosureStrikeStep = updates.ClosureStrikeStep;
        container.StraddleLiveDays = updates.StraddleLiveDays;
        container.OrderPriceShift = updates.OrderPriceShift;
        container.StraddleTargetPnl = updates.StraddleTargetPnl;

        await _containersService.UpdateAsync(id, container);
        return NoContent();
    }

    [HttpDelete("{id:length(24)}")]
    public async Task<IActionResult> Delete(string id)
    {
        var book = await _containersService.GetAsync(id);
        if (book is null)
        {
            return NotFound();
        }
        await _containersService.RemoveAsync(id);
        return NoContent();
    }
}
