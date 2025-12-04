namespace FacturasSRI.Application.Dtos.Reports
{
    public class VentasPorProductoDto
    {
        public DateTime Fecha { get; set; }
        public string? Vendedor { get; set; }
        public string? CodigoProducto { get; set; }
        public string NombreProducto { get; set; } = string.Empty;
        public decimal CantidadVendida { get; set; }
        public decimal PrecioPromedio { get; set; }
        public decimal TotalVendido { get; set; }
    }
}
