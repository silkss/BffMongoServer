namespace OptionTraderWebGui.Pages.Containers;

using Traders;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Traders.Strategies.BatmanStrategy;

public class DetailsModel : PageModel
{
    private readonly ContainerTrader _trader;

    public DetailsModel(ContainerTrader trader)
    {
        _trader = trader;
    }

    public List<BatmanOptionStrategy>? Strategies { get; set; }

    public void OnGet(string? id)
    {
        if (id != null)
        {
            if (_trader.GetById(id) is BatmanContainer container)
            {
                Strategies = container.Strategies;
            }
        }
    }
}
