
using Microsoft.AspNetCore.Mvc;

namespace FacturasSRI.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class GreetingsController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            return Ok(new { message = "Hello from the API!" });
        }
    }
}
