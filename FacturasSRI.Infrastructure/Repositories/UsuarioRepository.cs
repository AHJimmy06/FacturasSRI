using FacturasSRI.Application.Interfaces;
using FacturasSRI.Domain.Entities;
using FacturasSRI.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace FacturasSRI.Infrastructure.Repositories
{
    public class UsuarioRepository : IUsuarioRepository
    {
        private readonly FacturasSRIDbContext _context;

        public UsuarioRepository(FacturasSRIDbContext context)
        {
            _context = context;
        }

        public async Task<Usuario?> GetByEmailAsync(string email)
        {
            Console.WriteLine($"--- REPOSITORIO: Buscando usuario con email: '{email}' ---");
            var normalizedEmail = email.ToUpper();
            
            var usuarioEncontrado = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Email.ToUpper() == normalizedEmail);

            if (usuarioEncontrado == null)
            {
                Console.WriteLine("--- REPOSITORIO: No se encontró ningún usuario. ---");
            }
            else
            {
                Console.WriteLine($"--- REPOSITORIO: Usuario encontrado: {usuarioEncontrado.Email} ---");
            }

            return usuarioEncontrado;
        }

        public async Task<Usuario> AddAsync(Usuario usuario)
        {
            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();
            return usuario;
        }
    }
}