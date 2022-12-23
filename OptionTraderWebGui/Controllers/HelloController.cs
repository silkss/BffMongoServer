using Microsoft.AspNetCore.Mvc;

namespace OptionTraderWebGui.Controllers;

[Route("api/[controller]")]
[ApiController]
public class HelloController : ControllerBase
{
    [HttpGet]
    public ActionResult Get() => Ok("Hello from API controller");
}
