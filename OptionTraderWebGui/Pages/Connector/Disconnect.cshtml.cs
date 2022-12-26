using Connectors;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace OptionTraderWebGui.Pages.Connector;

public class DisconnectModel : PageModel
{
    private readonly IConnector _connector;

    public DisconnectModel(IConnector connector)
	{
        _connector = connector;
    }
    public void OnGet()
    {
        _connector.Disconnect();
    }
}
