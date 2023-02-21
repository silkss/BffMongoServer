namespace OptionTraderWebGui.Pages.Containers;

using Microsoft.AspNetCore.Mvc.RazorPages;
using Strategies.BatmanStrategy;
using Traders;

public class CloseModel : PageModel
{
    private readonly ContainerTrader _trader;

    public CloseModel(ContainerTrader trader) => _trader = trader;

    public void OnGetAsync(string? id)
    {
        if (id == null) return;
        if (_trader.GetById(id) is BatmanContainer container)
        {
            container.Close();
        }
    }
}
