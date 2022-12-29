using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Threading.Tasks;
using Traders;

namespace OptionTraderWebGui.Pages.Containers;

public class StopModel : PageModel
{
    private readonly ContainerTrader _trader;

    public StopModel(ContainerTrader trader)
    {
        _trader = trader;
    }

    public async Task OnGetAsync(string? id)
    {
        if (id != null)
            await _trader.StopContainerAsync(id);
    }
}
