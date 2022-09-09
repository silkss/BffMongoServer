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
	private ConnectorModel getModel() => new ConnectorModel
	{
		Host = _connector.Host,
		Port = _connector.Port,
		ClientId = _connector.ClientId,
		IsConnected = _connector.IsConnected,
		Accounts = _connector.GetAccounts(),
	};
	public ConnectorController(IConnector connector)
	{
		_connector = connector;
	}

	[HttpGet]
	public ActionResult<ConnectorModel> Get()
	{
		var info = getModel();
		return Ok(info);
	}
	[HttpPost]
	public IActionResult Post(ConnectorModel model)
	{
		if (model.IsConnected)
		{
			if (_connector.IsConnected)
				_connector.Disconnect();
			_connector.Connect(model.Host, model.Port, model.ClientId);
		}
		else if (!model.IsConnected)
		{
			_connector.Disconnect();
		}
        return Ok(getModel());
    }
}
