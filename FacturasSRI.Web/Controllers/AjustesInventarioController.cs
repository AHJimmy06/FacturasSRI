using FacturasSRI.Application.Dtos;
using FacturasSRI.Application.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace FacturasSRI.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Administrador,Bodeguero")]
    public class AjustesInventarioController : ControllerBase
    {
        private readonly IAjusteInventarioService _ajusteInventarioService;

        public AjustesInventarioController(IAjusteInventarioService ajusteInventarioService)
        {
            _ajusteInventarioService = ajusteInventarioService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateAdjustment([FromBody] AjusteInventarioDto ajusteDto)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdString, out var userId))
            {
                return Unauthorized();
            }

            ajusteDto.UsuarioIdAutoriza = userId;
            
            try
            {
                await _ajusteInventarioService.CreateAdjustmentAsync(ajusteDto);
                return Ok();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, "Ocurrió un error inesperado al procesar el ajuste.");
            }
        }
    }
}