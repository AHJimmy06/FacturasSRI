using FacturasSRI.Application.Dtos;
using FacturasSRI.Application.Interfaces;
using FacturasSRI.Domain.Entities;
using FacturasSRI.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FacturasSRI.Infrastructure.Services
{
    public class PurchaseService : IPurchaseService
    {
        private readonly FacturasSRIDbContext _context;
        private readonly ILogger<PurchaseService> _logger;

        public PurchaseService(FacturasSRIDbContext context, ILogger<PurchaseService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<bool> CreatePurchaseAsync(PurchaseDto purchaseDto)
        {
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    var producto = await _context.Productos.FindAsync(purchaseDto.ProductoId);
                    if (producto == null) throw new InvalidOperationException("El producto no existe.");
                    if (!producto.ManejaInventario) throw new InvalidOperationException("No se puede registrar una compra para un producto que no maneja inventario.");

                    if (producto.ManejaLotes)
                    {
                        var lote = new Lote
                        {
                            Id = Guid.NewGuid(),
                            ProductoId = purchaseDto.ProductoId,
                            CantidadComprada = purchaseDto.Cantidad,
                            CantidadDisponible = purchaseDto.Cantidad,
                            PrecioCompraUnitario = purchaseDto.PrecioCosto,
                            FechaCompra = DateTime.UtcNow,
                            FechaCaducidad = purchaseDto.FechaCaducidad?.ToUniversalTime(),
                            UsuarioIdCreador = purchaseDto.UsuarioIdCreador,
                            FechaCreacion = DateTime.UtcNow
                        };
                        _context.Lotes.Add(lote);

                        var cuentaPorPagarLote = new CuentaPorPagar
                        {
                            Id = Guid.NewGuid(),
                            MontoTotal = purchaseDto.Cantidad * purchaseDto.PrecioCosto,
                            SaldoPendiente = purchaseDto.Cantidad * purchaseDto.PrecioCosto,
                            UsuarioIdCreador = purchaseDto.UsuarioIdCreador,
                            FechaCreacion = DateTime.UtcNow
                        };
                        _context.CuentasPorPagar.Add(cuentaPorPagarLote);
                    }
                    else
                    {
                        producto.StockTotal += purchaseDto.Cantidad;
                        
                        var cuentaPorPagarGeneral = new CuentaPorPagar
                        {
                            Id = Guid.NewGuid(),
                            LoteId = null,
                            Proveedor = purchaseDto.Proveedor,
                            NumeroFactura = purchaseDto.NumeroFactura,
                            FechaEmision = DateTime.UtcNow,
                            FechaVencimiento = DateTime.UtcNow.AddDays(30),
                            MontoTotal = purchaseDto.Cantidad * purchaseDto.PrecioCosto,
                            SaldoPendiente = purchaseDto.Cantidad * purchaseDto.PrecioCosto,
                            UsuarioIdCreador = purchaseDto.UsuarioIdCreador,
                            FechaCreacion = DateTime.UtcNow
                        };
                        _context.CuentasPorPagar.Add(cuentaPorPagarGeneral);
                    }
                    
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                    return true;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "EXCEPCIÓN al crear la compra. Revirtiendo transacción.");
                    await transaction.RollbackAsync();
                    return false;
                }
            }
        }
        
        public async Task<List<PurchaseListItemDto>> GetPurchasesAsync()
        {
            var purchases = await (from lote in _context.Lotes
                                   join producto in _context.Productos on lote.ProductoId equals producto.Id
                                   join cuentaPorPagar in _context.CuentasPorPagar on lote.Id equals cuentaPorPagar.LoteId into cpps
                                   from cpp in cpps.DefaultIfEmpty()
                                   join usuario in _context.Usuarios on lote.UsuarioIdCreador equals usuario.Id into usuarioJoin
                                   from usuario in usuarioJoin.DefaultIfEmpty()
                                   orderby lote.FechaCompra descending
                                   select new PurchaseListItemDto
                                   {
                                       LoteId = lote.Id,
                                       ProductName = producto.Nombre,
                                       CantidadComprada = lote.CantidadComprada,
                                       CantidadDisponible = lote.CantidadDisponible,
                                       PrecioCompraUnitario = lote.PrecioCompraUnitario,
                                       ValorTotalCompra = lote.CantidadComprada * lote.PrecioCompraUnitario,
                                       FechaCompra = lote.FechaCompra,
                                       FechaCaducidad = lote.FechaCaducidad,
                                       Proveedor = cpp != null ? cpp.Proveedor : "N/A",
                                       CreadoPor = usuario != null ? usuario.PrimerNombre + " " + usuario.PrimerApellido : "Usuario no encontrado"
                                   }).ToListAsync();

            return purchases;
        }
    }
}