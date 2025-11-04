using FacturasSRI.Application.Dtos.Auth;
using FacturasSRI.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using FacturasSRI.Domain.Entities;

namespace FacturasSRI.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly IConfiguration _configuration;

        public AuthController(IUsuarioRepository usuarioRepository, IConfiguration configuration)
        {
            _usuarioRepository = usuarioRepository;
            _configuration = configuration;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDto registerRequest)
        {
            var existingUser = await _usuarioRepository.GetByEmailAsync(registerRequest.Email);
            if (existingUser != null)
            {
                return BadRequest(new { message = "El correo electrónico ya está en uso." });
            }

            var newUser = new Usuario
            {
                Id = Guid.NewGuid(),
                PrimerNombre = registerRequest.PrimerNombre,
                SegundoNombre = registerRequest.SegundoNombre,
                PrimerApellido = registerRequest.PrimerApellido,
                SegundoApellido = registerRequest.SegundoApellido,
                Email = registerRequest.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(registerRequest.Password),
                Rol = registerRequest.Rol,
                EstaActivo = true
            };

            await _usuarioRepository.AddAsync(newUser);

            return Ok(new { message = "Usuario registrado exitosamente." });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto loginRequest)
        {
            Console.WriteLine($"\n--- CONTROLADOR: Petición de login recibida para: '{loginRequest.Email}' ---");
            
            var usuario = await _usuarioRepository.GetByEmailAsync(loginRequest.Email);

            if (usuario == null)
            {
                Console.WriteLine("--- CONTROLADOR: El usuario es NULL. Devolviendo Unauthorized. ---");
                return Unauthorized(new { message = "Credenciales inválidas o usuario inactivo." });
            }

            Console.WriteLine($"--- CONTROLADOR: Usuario de la BD: '{usuario.Email}', Hash: '{usuario.PasswordHash}' ---");
            Console.WriteLine($"--- CONTROLADOR: Verificando contraseña: '{loginRequest.Password}' ---");
            
            bool esPasswordValido = BCrypt.Net.BCrypt.Verify(loginRequest.Password, usuario.PasswordHash);

            Console.WriteLine($"--- CONTROLADOR: ¿La contraseña es válida? -> {esPasswordValido} ---");

            if (!usuario.EstaActivo || !esPasswordValido)
            {
                Console.WriteLine("--- CONTROLADOR: Usuario inactivo o contraseña inválida. Devolviendo Unauthorized. ---");
                return Unauthorized(new { message = "Credenciales inválidas o usuario inactivo." });
            }

            Console.WriteLine("--- CONTROLADOR: ¡Login exitoso! Generando token. ---");
            var token = GenerateJwtToken(usuario);

            return Ok(new LoginResponseDto { Token = token });
        }

        private string GenerateJwtToken(Domain.Entities.Usuario usuario)
        {
            var claims = new[]
            {
                new Claim("sub", usuario.Id.ToString()),
                new Claim("name", $"{usuario.PrimerNombre} {usuario.PrimerApellido}"),
                new Claim("email", usuario.Email),
                new Claim("jti", Guid.NewGuid().ToString()),
                new Claim("role", usuario.Rol.ToString())
            };

            var secretKey = _configuration["Jwt:SecretKey"];
            if (string.IsNullOrEmpty(secretKey))
            {
                throw new InvalidOperationException("Jwt:SecretKey no está configurada.");
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.UtcNow.AddDays(1);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: expires,
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}