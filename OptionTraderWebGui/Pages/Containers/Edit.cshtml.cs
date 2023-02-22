namespace OptionTraderWebGui.Pages.Containers;

using Connectors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OptionTraderWebGui.Models;
using Traders;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Strategies.BatmanStrategy;

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
    [BindProperty] public BatmanSettings? Settings { get; set; }


    public void OnGet(string? id)
    {
        if (id == null) return;
        if (_trader.GetById(id) is BatmanContainer container)
        {
            if (container.Instrument != null)
            {
                Instrument = new InstrumentRequestSettings
                {
                    Name = container.Instrument.FullName!,
                    Exchange = container.Instrument.Exchange!
                };
            }
            if (container.Settings != null)
                Settings= container.Settings;
        }
    }
    public async Task<IActionResult> OnPostAsync(string? id)
    {
        if (id != null)
        {
            if (_trader.GetById(id) is BatmanContainer container)
            {
                container.Settings = Settings;
                return RedirectToPage("./Index");
            }
        }
        var sec = await _connector.RequestInstrumentAsync(Instrument!.Name, Instrument.Exchange);

        if (sec != null)
        {
            var container = new BatmanContainer
            {
                Instrument = sec,
                Settings = Settings
            };

            await _trader.AddContainerAsync(container);
        }
        return RedirectToPage("./Index");
    }
}
