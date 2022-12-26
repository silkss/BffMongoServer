using Microsoft.AspNetCore.Mvc.RazorPages;
using Traders;

namespace OptionTraderWebGui.Pages.Containers;

public class StartTradeModel : PageModel
{
    private readonly ContainerTrader _trader;
    public string? Message;
    public StartTradeModel(ContainerTrader trader)
    {
        _trader = trader;
    }
    public void OnGet(string? id)
    {
        if (id != null)
            _trader.StartTrade(id);
    }
}
