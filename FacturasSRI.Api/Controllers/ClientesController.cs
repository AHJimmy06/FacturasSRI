using FacturasSRI.Application.Dtos.Clientes;
using FacturasSRI.Application.Interfaces;
using FacturasSRI.Application.Mappings;
using FacturasSRI.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace FacturasSRI.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ClientesController : ControllerBase
    {
        private readonly IClienteRepository _clienteRepository;

        public ClientesController(IClienteRepository clienteRepository)
        {
            _clienteRepository = clienteRepository;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ClienteDto>>> GetAll()
        {
            var clientes = await _clienteRepository.GetAllAsync();
            return Ok(clientes.Select(c => c.ToDto()));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ClienteDto>> GetById(Guid id)
        {
            var cliente = await _clienteRepository.GetByIdAsync(id);
            if (cliente == null)
            {
                return NotFound();
            }
            return Ok(cliente.ToDto());
        }

        [HttpPost]
        public async Task<ActionResult<ClienteDto>> Create(CreateClienteDto createClienteDto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }
            
            var cliente = new Cliente
            {
                TipoIdentificacion = createClienteDto.TipoIdentificacion,
                NumeroIdentificacion = createClienteDto.NumeroIdentificacion,
                RazonSocial = createClienteDto.RazonSocial,
                Email = createClienteDto.Email ?? string.Empty,
                Direccion = createClienteDto.Direccion ?? string.Empty,
                Telefono = createClienteDto.Telefono ?? string.Empty,
                UsuarioIdCreador = Guid.Parse(userId)
            };

            var nuevoCliente = await _clienteRepository.AddAsync(cliente);
            
            return CreatedAtAction(nameof(GetById), new { id = nuevoCliente.Id }, nuevoCliente.ToDto());
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, UpdateClienteDto updateClienteDto)
        {
            var clienteExistente = await _clienteRepository.GetByIdAsync(id);
            if (clienteExistente == null)
            {
                return NotFound();
            }

            clienteExistente.TipoIdentificacion = updateClienteDto.TipoIdentificacion;
            clienteExistente.NumeroIdentificacion = updateClienteDto.NumeroIdentificacion;
            clienteExistente.RazonSocial = updateClienteDto.RazonSocial;
            clienteExistente.Email = updateClienteDto.Email ?? string.Empty;
            clienteExistente.Direccion = updateClienteDto.Direccion ?? string.Empty;
            clienteExistente.Telefono = updateClienteDto.Telefono ?? string.Empty;

            await _clienteRepository.UpdateAsync(clienteExistente);

            return NoContent();
        }

        [HttpPatch("{id}/deactivate")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Deactivate(Guid id)
        {
            var clienteExistente = await _clienteRepository.GetByIdAsync(id);
            if (clienteExistente == null)
            {
                return NotFound();
            }

            await _clienteRepository.DeactivateAsync(id);
            return NoContent();
        }

        [HttpPatch("{id}/activate")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Activate(Guid id)
        {
            var clienteExistente = await _clienteRepository.GetByIdAsync(id);
            if (clienteExistente == null)
            {
                return NotFound();
            }

            await _clienteRepository.ActivateAsync(id);
            return NoContent();
        }
    }
}