using Connectors;
using Connectors.Info;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using OptionTraderWebGui.Models;
using Strategies;
using Strategies.Settings;
using System.Collections.Generic;
using System.Threading.Tasks;
using Traders;

namespace OptionTraderWebGui.Pages.Containers;

public class CreateModel : PageModel
{
    private readonly IConnector _connector;
    private readonly ContainerTrader _trader;

    #region BindProperty
    [BindProperty] public InstrumentRequestSettings? Instrument { get; set; }

    [BindProperty] public ContainerSettings? ContainerSettings { get; set; }

    [BindProperty] public OptionStrategySettings? OptionStrategySettings { get; set; }
    #endregion

    public CreateModel(IConnector connector, ContainerTrader trader)
	{
        _connector = connector;
        _trader = trader;
        AccountsSL = new SelectList(_connector.GetAccounts());
    }
    public ConnectorInfo? Info { get; set; }
    public SelectList AccountsSL { get; set; }

    
    public void OnGet()
    {
        Info = _connector.GetConnectionInfo();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var sec = await _connector.RequestInstrumentAsync(Instrument.Name, Instrument.Exchange);

        if (sec != null)
        {
            var container = new Container
            {
                Instrument = sec,
                ContainerSettings = ContainerSettings,
                OptionStrategySettings = OptionStrategySettings,
            };
            await _trader.AddContainerAsync(container);
        }
        return RedirectToPage("./Index");
    }
}
