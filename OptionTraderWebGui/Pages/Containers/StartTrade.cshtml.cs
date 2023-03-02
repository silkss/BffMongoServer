using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Traders;

namespace OptionTraderWebGui.Pages.Containers;

public class StartTradeModel : PageModel
{
    private readonly ContainerTrader _trader;
    private readonly ILogger<StartTradeModel> _logger;
    public string? Message;
    public StartTradeModel(ContainerTrader trader, ILogger<StartTradeModel> logger)
    {
        _trader = trader;
        _logger = logger;
    }
    public void OnGet(string? id)
    {
        if (id != null)
            _trader.StartTrade(id);
    }
}
