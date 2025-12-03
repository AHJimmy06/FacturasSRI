namespace FacturasSRI.Application.Dtos.Reports
{
    public class ClienteActividadDto
    {
        public string NombreCliente { get; set; } = string.Empty;
        public string Identificacion { get; set; } = string.Empty;
        public DateTime? UltimaCompra { get; set; }
        public int DiasDesdeUltimaCompra { get; set; }
        public int NumeroDeCompras { get; set; }
        public decimal TotalComprado { get; set; }
    }
}
