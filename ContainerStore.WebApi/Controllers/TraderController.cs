using ContainerStore.Data.Models;
using ContainerStore.Traders.Base;
using ContainerStore.WebApi.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ContainerStore.WebApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class TraderController : ControllerBase
{
	private readonly Trader _trader;
	private readonly ContainersService _containersService;

	public TraderController(Trader trader, ContainersService containersService)
	{
		_trader = trader;
		_containersService = containersService;
	}
    [HttpGet]
    public IEnumerable<Container> Get() => _trader.GetContainers();

    [HttpPost("{id:length(24)}")]
    public async Task<IActionResult> Start(string id)
    {
        var container = await _containersService.GetAsync(id);
        if (container is null)
        {
            return NotFound();
        }
        var result = _trader.AddToTrade(container);
        if (!result.isAdded)
        {
            return BadRequest(result.message);
        }
        return Ok(result.message);
    }

}
