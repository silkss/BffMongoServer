using Connectors;
using Common.Types.Instruments;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;

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
		var instument = _connector.RequestInstrument(localName, exchange); // this is for test. REMOVE later!
		if (instument is null)
		{
			return NotFound("Не удалось получить инструмент. Проверь логи!");
		}
		return Ok(instument);
	}

	[HttpPost]
	public IActionResult Post(Instrument instrument)
	{
		_connector.RequestMarketData(instrument);
		return Ok();
	}
}
