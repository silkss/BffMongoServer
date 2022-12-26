using Connectors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OptionTraderWebGui.Models;
using System.Threading.Tasks;

namespace OptionTraderWebGui.Pages.Connector;

public class ConnectModel : PageModel
{
    private readonly IConnector _connector;

    public ConnectModel(IConnector connector)
    {
        _connector = connector;
    }

    [BindProperty]
    public IbConnectionSettings? Settings { get; set; }

    public IActionResult OnPost()
    {
        if (Settings != null)
        {
            _connector.Connect(Settings.Host, Settings.Port, Settings.ClientId);
        }
        //if (!ModelState.IsValid)
        //{
        //    return Page();
        //}

        //if (Settings != null) _context.Customer.Add(Customer);
        //await _context.SaveChangesAsync();

        return RedirectToPage("./Index");
    }
}
