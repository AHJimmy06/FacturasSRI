using System;
using System.Collections.Generic;

namespace FacturasSRI.Application.Dtos
{
    public class DashboardStatsDto
    {
        public int TotalFacturasEmitidas { get; set; }
        public int TotalClientesRegistrados { get; set; }
        public decimal IngresosEsteMes { get; set; }
        public List<RecentInvoiceDto> RecentInvoices { get; set; } = new();
        public List<TopProductDto> TopProducts { get; set; } = new();
    }

    public class RecentInvoiceDto
    {
        public Guid Id { get; set; }
        public string NumeroFactura { get; set; } = string.Empty;
        public string ClienteNombre { get; set; } = string.Empty;
        public DateTime FechaEmision { get; set; }
        public decimal Total { get; set; }
        public string Estado { get; set; } = string.Empty;
    }

    public class TopProductDto
    {
        public string ProductName { get; set; } = string.Empty;
        public int QuantitySold { get; set; }
        public decimal TotalRevenue { get; set; }
    }
}