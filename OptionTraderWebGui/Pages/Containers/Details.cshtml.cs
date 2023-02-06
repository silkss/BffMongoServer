namespace OptionTraderWebGui.Pages.Containers;

using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using Strategies;
using Traders;

public class DetailsModel : PageModel
{
    private readonly ContainerTrader _trader;

    public DetailsModel(ContainerTrader trader)
    {
        _trader = trader;
    }

    public List<OptionStrategy>? Strategies { get; set; }

    public void OnGet(string? id)
    {
        if (id != null)
        {
            if (_trader.GetById(id) is Container container)
            {
                Strategies = container.OptionStrategies;
            }
        }
    }
}
