using FacturasSRI.Application.Dtos;
using FacturasSRI.Application.Interfaces;
using FacturasSRI.Domain.Entities;
using FacturasSRI.Infrastructure.Persistence;
using System;
using System.Threading.Tasks;

namespace FacturasSRI.Infrastructure.Services
{
    public class PurchaseService : IPurchaseService
    {
        private readonly FacturasSRIDbContext _context;

        public PurchaseService(FacturasSRIDbContext context)
        {
            _context = context;
        }

        public async Task CreatePurchaseAsync(PurchaseDto purchaseDto)
        {
            var lote = new Lote
            {
                Id = Guid.NewGuid(),
                ProductoId = purchaseDto.ProductoId,
                CantidadComprada = purchaseDto.Cantidad,
                CantidadDisponible = purchaseDto.Cantidad,
                PrecioCompraUnitario = purchaseDto.PrecioCosto,
                FechaCompra = DateTime.UtcNow,
                FechaCaducidad = purchaseDto.FechaCaducidad,
                FechaCreacion = DateTime.UtcNow
            };

            var cuentaPorPagar = new CuentaPorPagar
            {
                Id = Guid.NewGuid(),
                LoteId = lote.Id,
                Proveedor = purchaseDto.Proveedor,
                NumeroFactura = purchaseDto.NumeroFactura,
                FechaEmision = DateTime.UtcNow,
                FechaVencimiento = DateTime.UtcNow.AddDays(30), // Assuming 30 days to pay
                MontoTotal = purchaseDto.Cantidad * purchaseDto.PrecioCosto,
                SaldoPendiente = purchaseDto.Cantidad * purchaseDto.PrecioCosto,
                Pagada = false,
                FechaCreacion = DateTime.UtcNow
            };

            _context.Lotes.Add(lote);
            _context.CuentasPorPagar.Add(cuentaPorPagar);

            await _context.SaveChangesAsync();
        }
    }
}
