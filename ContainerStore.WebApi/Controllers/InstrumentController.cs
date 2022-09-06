using ContainerStore.Connectors;
using ContainerStore.Data.Models;
using Microsoft.AspNetCore.Mvc;

namespace ContainerStore.WebApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class InstrumentController : ControllerBase
{
	private readonly IConnector _connector;

	public InstrumentController(IConnector connector)
	{
		_connector = connector;
	}

	[HttpGet] 
	public ActionResult<Instrument> Get(string localName, string? exchange)
	{
		if (exchange is null)
		{
			exchange = "GLOBEX";
		}
		var instument = _connector.RequestInstrument(localName, exchange);
		if (instument is null)
		{
			return NotFound("Не удалось получить инструмент. Проверь логи!");
		}
		return Ok(instument);
	}
}
