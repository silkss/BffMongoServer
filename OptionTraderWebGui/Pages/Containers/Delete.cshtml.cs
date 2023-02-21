namespace OptionTraderWebGui.Pages.Containers;

using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using Traders;
using Strategies.Base;
using Strategies.BatmanStrategy;

public class DeleteModel : PageModel
{
    private readonly ContainerTrader _trader;

    public DeleteModel(ContainerTrader trader)
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
