namespace FacturasSRI.Application.Dtos.Productos;
public class ProductoDto
{
    public Guid Id { get; set; }
    public string CodigoPrincipal { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public decimal PrecioVentaUnitario { get; set; }
    public bool EstaActivo { get; set; }
    public string Descripcion { get; set; } = string.Empty;
}