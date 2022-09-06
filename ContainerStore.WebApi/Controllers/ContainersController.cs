using ContainerStore.Data.Models;
using ContainerStore.WebApi.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ContainerStore.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ContainersController : ControllerBase
{
    private readonly ILogger<ContainersController> _logger;
    private readonly ContainersService _containersService;

    public ContainersController(ILogger<ContainersController> logger, ContainersService containersService)
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
    public async Task<IActionResult> Update(string id, Container updatedContainer)
    {
        var container = await _containersService.GetAsync(id);
        if (container is null)
        {
            return NotFound();
        }
        updatedContainer.Id = container.Id;
        await _containersService.UpdateAsync(id, updatedContainer);
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
