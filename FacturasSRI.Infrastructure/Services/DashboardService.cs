using FacturasSRI.Application.Dtos;
using FacturasSRI.Application.Interfaces;
using FacturasSRI.Domain.Enums;
using FacturasSRI.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace FacturasSRI.Infrastructure.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly FacturasSRIDbContext _context;

        public DashboardService(FacturasSRIDbContext context)
        {
            _context = context;
        }

        public async Task<DashboardStatsDto> GetDashboardStatsAsync()
        {
            var totalFacturas = await _context.Facturas
                .CountAsync(f => f.Estado == EstadoFactura.Autorizada);

            var totalClientes = await _context.Clientes
                .CountAsync(c => c.EstaActivo);

            var hoy = DateTime.UtcNow;
            var primerDiaDelMes = new DateTime(hoy.Year, hoy.Month, 1, 0, 0, 0, DateTimeKind.Utc);
            var primerDiaDelSiguienteMes = primerDiaDelMes.AddMonths(1);

            var ingresosMes = await _context.Facturas
                .Where(f => f.Estado == EstadoFactura.Autorizada && 
                            f.FechaEmision >= primerDiaDelMes && 
                            f.FechaEmision < primerDiaDelSiguienteMes)
                .Join(_context.CuentasPorCobrar, 
                      f => f.Id, 
                      c => c.FacturaId, 
                      (f, c) => new { f.Total, c.Pagada })
                .Where(x => x.Pagada)
                .SumAsync(x => x.Total);

            var recentInvoices = await _context.Facturas
                .Include(f => f.Cliente)
                .Where(f => f.Estado == EstadoFactura.Autorizada)
                .OrderByDescending(f => f.FechaEmision)
                .Take(5)
                .Select(f => new RecentInvoiceDto
                {
                    Id = f.Id,
                    NumeroFactura = f.NumeroFactura,
                    ClienteNombre = f.Cliente != null ? f.Cliente.RazonSocial : "Consumidor Final",
                    FechaEmision = f.FechaEmision,
                    Total = f.Total,
                    Estado = f.Estado.ToString()
                })
                .ToListAsync();

            var topProducts = await _context.FacturaDetalles
                .Include(d => d.Factura)
                .Include(d => d.Producto)
                .Where(d => d.Factura.Estado == EstadoFactura.Autorizada)
                .GroupBy(d => new { d.ProductoId, d.Producto.Nombre })
                .Select(g => new TopProductDto
                {
                    ProductName = g.Key.Nombre,
                    QuantitySold = g.Sum(d => d.Cantidad),
                    TotalRevenue = g.Sum(d => d.Subtotal)
                })
                .OrderByDescending(p => p.QuantitySold)
                .Take(5)
                .ToListAsync();

            return new DashboardStatsDto
            {
                TotalFacturasEmitidas = totalFacturas,
                TotalClientesRegistrados = totalClientes,
                IngresosEsteMes = ingresosMes,
                RecentInvoices = recentInvoices,
                TopProducts = topProducts
            };
        }
    }
}