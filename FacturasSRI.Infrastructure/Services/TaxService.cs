using FacturasSRI.Application.Dtos;
using FacturasSRI.Application.Interfaces;
using FacturasSRI.Domain.Entities;
using FacturasSRI.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FacturasSRI.Infrastructure.Services
{
    public class TaxService : ITaxService
    {
        private readonly FacturasSRIDbContext _context;

        public TaxService(FacturasSRIDbContext context)
        {
            _context = context;
        }

        public async Task<TaxDto> CreateTaxAsync(TaxDto taxDto)
        {
            var tax = new Impuesto
            {
                Id = Guid.NewGuid(),
                Nombre = taxDto.Nombre,
                CodigoSRI = taxDto.CodigoSRI,
                Porcentaje = taxDto.Porcentaje,
                EstaActivo = taxDto.EstaActivo
            };
            _context.Impuestos.Add(tax);
            await _context.SaveChangesAsync();
            taxDto.Id = tax.Id;
            return taxDto;
        }

        public async Task DeleteTaxAsync(Guid id)
        {
            var tax = await _context.Impuestos.FindAsync(id);
            if (tax != null)
            {
                tax.EstaActivo = false;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<TaxDto?> GetTaxByIdAsync(Guid id)
        {
            var tax = await _context.Impuestos.FindAsync(id);
            if (tax == null || !tax.EstaActivo)
            {
                return null;
            }
            return new TaxDto
            {
                Id = tax.Id,
                Nombre = tax.Nombre,
                CodigoSRI = tax.CodigoSRI,
                Porcentaje = tax.Porcentaje,
                EstaActivo = tax.EstaActivo
            };
        }

        public async Task<List<TaxDto>> GetTaxesAsync()
        {
            return await _context.Impuestos.Where(t => t.EstaActivo).Select(tax => new TaxDto
            {
                Id = tax.Id,
                Nombre = tax.Nombre,
                CodigoSRI = tax.CodigoSRI,
                Porcentaje = tax.Porcentaje,
                EstaActivo = tax.EstaActivo
            }).ToListAsync();
        }

        public async Task UpdateTaxAsync(TaxDto taxDto)
        {
            var tax = await _context.Impuestos.FindAsync(taxDto.Id);
            if (tax != null)
            {
                tax.Nombre = taxDto.Nombre;
                tax.CodigoSRI = taxDto.CodigoSRI;
                tax.Porcentaje = taxDto.Porcentaje;
                tax.EstaActivo = taxDto.EstaActivo;
                await _context.SaveChangesAsync();
            }
        }
    }
}
