using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Strategies;
using System.Collections.Generic;
using Traders;

namespace OptionTraderWebGui.Pages.Containers;

public class IndexModel : PageModel
{
    private readonly ContainerTrader _trader;

    public IndexModel(ContainerTrader trader)
	{
        _trader = trader;
    }

    [BindProperty]
    public IEnumerable<Container>? Containers { get; set; }
    public void OnGet()
    {
        Containers = _trader.GetAllContainers();
    }
}
