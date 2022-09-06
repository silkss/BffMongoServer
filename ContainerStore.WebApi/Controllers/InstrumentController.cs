using ContainerStore.Data.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace ContainerStore.WebApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class InstrumentController : ControllerBase
{
	private readonly ILogger<InstrumentController> _logger;

	public InstrumentController(ILogger<InstrumentController> logger)
	{
		_logger = logger;
	}

	//[HttpGet] 
	//public ActionResult<Instrument> Get(string localName, string? exchange)
	//{

	//}

}
