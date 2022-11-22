using Connectors;
using Connectors.Info;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;

[EnableCors()]
[Route("api/[controller]")]
[ApiController]
public class ConnectorController : ControllerBase
{
	private readonly IConnector _connector;

	public ConnectorController(IConnector connector)
	{
		_connector = connector;
	}

	[HttpGet]
	public ActionResult<ConnectorInfo> Get() => Ok(_connector.GetConnectionInfo());
	
	[HttpPost]
	public IActionResult Post(ConnectorInfo info)
	{
		if (info.IsConnected)
		{
			if (_connector.GetConnectionInfo().IsConnected)
				_connector.Disconnect();
			_connector.Connect(info.Host, info.Port, info.ClientId);
		}
		else if (!info.IsConnected)
		{
			_connector.Disconnect();
		}
        return Ok(_connector.GetConnectionInfo());
    }
}
