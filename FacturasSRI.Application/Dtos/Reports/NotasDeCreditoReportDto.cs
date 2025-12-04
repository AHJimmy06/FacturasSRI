namespace FacturasSRI.Application.Dtos.Reports
{
    public class NotasDeCreditoReportDto
    {
        public string? Vendedor { get; set; }
        public string NumeroNotaCredito { get; set; } = string.Empty;
        public DateTime FechaEmision { get; set; }
        public string? NombreCliente { get; set; }
        public string FacturaModificada { get; set; } = string.Empty;
        public string Motivo { get; set; } = string.Empty;
        public decimal ValorTotal { get; set; }
    }
}
