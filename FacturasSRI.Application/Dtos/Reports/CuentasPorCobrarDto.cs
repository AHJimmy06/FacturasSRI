namespace FacturasSRI.Application.Dtos.Reports
{
    public class CuentasPorCobrarDto
    {
        public string? Vendedor { get; set; }
        public string NombreCliente { get; set; } = string.Empty;
        public string NumeroFactura { get; set; } = string.Empty;
        public DateTime FechaEmision { get; set; }
        public int DiasVencida { get; set; }
        public decimal MontoFactura { get; set; }
        public decimal MontoPagado { get; set; }
        public decimal SaldoPendiente { get; set; }
    }
}
