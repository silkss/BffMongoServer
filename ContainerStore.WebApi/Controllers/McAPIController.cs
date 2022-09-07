using ContainerStore.Connectors;
using ContainerStore.WebApi.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace ContainerStore.WebApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class McAPIController : ControllerBase
{
	private readonly ILogger<McAPIController> _logger;
	private readonly IConnector _connector;
	private readonly ContainersService _containersService;

	public McAPIController(ILogger<McAPIController> logger, IConnector connector, ContainersService containersService)
	{
		_logger = logger;
		_connector = connector;
		_containersService = containersService;
	}

	[HttpGet]
	public IActionResult Get(string symbol, double price, string account, string type)
	{
		return Ok();
	}
}
