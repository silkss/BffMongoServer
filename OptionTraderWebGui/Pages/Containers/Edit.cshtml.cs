namespace OptionTraderWebGui.Pages.Containers;

using Connectors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OptionTraderWebGui.Models;
using Traders;
using Strategies;
using Strategies.Settings;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;

public class EditModel : PageModel
{
    private readonly ContainerTrader _trader;
    private readonly IConnector _connector;

    public EditModel(ContainerTrader trader, IConnector connector)
    {
        _trader = trader;
        _connector = connector;
        AccountsSL = new SelectList(_connector.GetAccounts());
    }

    public SelectList AccountsSL { get; set; }

    [BindProperty] public InstrumentRequestSettings? Instrument { get; set; }
    [BindProperty] public ContainerSettings? ContainerSettings { get; set; }
    [BindProperty] public OptionStrategySettings? OptionStrategySettings { get; set; }
    [BindProperty] public SpreadSettings? SpreadSettings { get; set; }

    public void OnGet(string? id)
    {
        if (id == null) return;
        if (_trader.GetById(id) is Container container)
        {
            if (container.Instrument != null)
            {
                Instrument = new InstrumentRequestSettings
                {
                    Name = container.Instrument.FullName!,
                    Exchange = container.Instrument.Exchange!
                };
            }
            if (container.ContainerSettings != null)
                ContainerSettings = container.ContainerSettings;
            if (container.SpreadSettings != null)
                SpreadSettings = container.SpreadSettings;
            if (container.OptionStrategySettings != null)
                OptionStrategySettings = container.OptionStrategySettings;
        }
    }
    public async Task<IActionResult> OnPostAsync(string? id)
    {
        if (id != null)
        {
            if (_trader.GetById(id) is Container container)
            {
                container.SpreadSettings = SpreadSettings;
                if (ContainerSettings.Account != null)
                    container.ContainerSettings = ContainerSettings;
                container.OptionStrategySettings = OptionStrategySettings;

                return RedirectToPage("./Index");
            }
        }
        var sec = await _connector.RequestInstrumentAsync(Instrument.Name, Instrument.Exchange);

        if (sec != null)
        {
            var container = new Container
            {
                Instrument = sec,
                ContainerSettings = ContainerSettings,
                OptionStrategySettings = OptionStrategySettings,
                SpreadSettings = SpreadSettings,
            };
            await _trader.AddContainerAsync(container);
        }
        return RedirectToPage("./Index");
    }
}
