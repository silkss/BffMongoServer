namespace OptionTraderWebGui.Pages.Containers;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Strategies.BatmanStrategy;
using System.Collections.Generic;
using Traders;

public class IndexModel : PageModel
{
    private readonly ContainerTrader _trader;

    public IndexModel(ContainerTrader trader)
	{
        _trader = trader;
    }

    [BindProperty]
    public IEnumerable<BatmanContainer>? Containers { get; set; }
    public void OnGet()
    {
        Containers = _trader.GetAllContainers();
    }
}
