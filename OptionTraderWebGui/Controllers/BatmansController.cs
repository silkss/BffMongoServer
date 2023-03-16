namespace OptionTraderWebGui.Controllers;

using DataTransferObjects.BatmanDto;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using Traders;

[ApiController]
[Route("api/[controller]")]
public class BatmansController {
    private readonly ContainerTrader _trader;

    public BatmansController(ContainerTrader trader) {
        _trader = trader;
    }

    [HttpGet]
    public IEnumerable<BatmanContainerModelDto> Get() => 
        _trader.GetAllContainers().Select(c => new BatmanContainerModelDto(c));
}
