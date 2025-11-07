using FacturasSRI.Application.Dtos;
using FacturasSRI.Application.Interfaces;
using FacturasSRI.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FacturasSRI.Infrastructure.Services
{
    public class InvoiceService : IInvoiceService
    {
        private readonly List<Factura> _invoices = new();

        public Task<InvoiceDto> CreateInvoiceAsync(InvoiceDto invoiceDto)
        {
            var invoice = new Factura
            {
                Id = Guid.NewGuid(),
                FechaEmision = invoiceDto.FechaEmision,
                NumeroFactura = invoiceDto.NumeroFactura,
                ClienteId = invoiceDto.ClienteId,
                SubtotalSinImpuestos = invoiceDto.SubtotalSinImpuestos,
                TotalDescuento = invoiceDto.TotalDescuento,
                TotalIVA = invoiceDto.TotalIVA,
                Total = invoiceDto.Total,
                FechaCreacion = DateTime.UtcNow,
                Detalles = invoiceDto.Detalles.Select(d => new FacturaDetalle
                {
                    Id = Guid.NewGuid(),
                    ProductoId = d.ProductoId,
                    Cantidad = d.Cantidad,
                    PrecioVentaUnitario = d.PrecioVentaUnitario,
                    Descuento = d.Descuento,
                    Subtotal = d.Subtotal
                }).ToList()
            };
            _invoices.Add(invoice);
            invoiceDto.Id = invoice.Id;
            return Task.FromResult(invoiceDto);
        }

        public Task DeleteInvoiceAsync(Guid id)
        {
            var invoice = _invoices.FirstOrDefault(i => i.Id == id);
            if (invoice != null)
            {
                _invoices.Remove(invoice);
            }
            return Task.CompletedTask;
        }

        public Task<InvoiceDto?> GetInvoiceByIdAsync(Guid id)
        {
            var invoice = _invoices.FirstOrDefault(i => i.Id == id);
            if (invoice == null)
            {
                return Task.FromResult<InvoiceDto?>(null);
            }
            var invoiceDto = new InvoiceDto
            {
                Id = invoice.Id,
                FechaEmision = invoice.FechaEmision,
                NumeroFactura = invoice.NumeroFactura,
                ClienteId = invoice.ClienteId,
                SubtotalSinImpuestos = invoice.SubtotalSinImpuestos,
                TotalDescuento = invoice.TotalDescuento,
                TotalIVA = invoice.TotalIVA,
                Total = invoice.Total,
                Detalles = invoice.Detalles.Select(d => new InvoiceDetailDto
                {
                    Id = d.Id,
                    ProductoId = d.ProductoId,
                    Cantidad = d.Cantidad,
                    PrecioVentaUnitario = d.PrecioVentaUnitario,
                    Descuento = d.Descuento,
                    Subtotal = d.Subtotal
                }).ToList()
            };
            return Task.FromResult<InvoiceDto?>(invoiceDto);
        }

        public Task<List<InvoiceDto>> GetInvoicesAsync()
        {
            var invoiceDtos = _invoices.Select(invoice => new InvoiceDto
            {
                Id = invoice.Id,
                FechaEmision = invoice.FechaEmision,
                NumeroFactura = invoice.NumeroFactura,
                ClienteId = invoice.ClienteId,
                SubtotalSinImpuestos = invoice.SubtotalSinImpuestos,
                TotalDescuento = invoice.TotalDescuento,
                TotalIVA = invoice.TotalIVA,
                Total = invoice.Total
            }).ToList();
            return Task.FromResult(invoiceDtos);
        }

        public Task UpdateInvoiceAsync(InvoiceDto invoiceDto)
        {
            var invoice = _invoices.FirstOrDefault(i => i.Id == invoiceDto.Id);
            if (invoice != null)
            {
                invoice.FechaEmision = invoiceDto.FechaEmision;
                invoice.NumeroFactura = invoiceDto.NumeroFactura;
                invoice.ClienteId = invoiceDto.ClienteId;
                invoice.SubtotalSinImpuestos = invoiceDto.SubtotalSinImpuestos;
                invoice.TotalDescuento = invoiceDto.TotalDescuento;
                invoice.TotalIVA = invoiceDto.TotalIVA;
                invoice.Total = invoiceDto.Total;
                // Note: Updating details would be more complex in a real scenario
            }
            return Task.CompletedTask;
        }
    }
}
