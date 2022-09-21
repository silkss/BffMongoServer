using ContainerStore.Connectors;
using ContainerStore.Data.ServiceModel;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace ContainerStore.WebApi.Controllers;

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
	public ActionResult<ConnectorModel> Get() => Ok(_connector.GetConnectionInfo());
	
	[HttpPost]
	public IActionResult Post(ConnectorModel model)
	{
		if (model.IsConnected)
		{
			if (_connector.GetConnectionInfo().IsConnected)
				_connector.Disconnect();
			_connector.Connect(model.Host, model.Port, model.ClientId);
		}
		else if (!model.IsConnected)
		{
			_connector.Disconnect();
		}
        return Ok(_connector.GetConnectionInfo());
    }
}
