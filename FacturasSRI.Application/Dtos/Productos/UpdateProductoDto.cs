namespace FacturasSRI.Application.Dtos.Productos;
public class UpdateProductoDto
{
    public string Nombre { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public decimal PrecioVentaUnitario { get; set; }
}