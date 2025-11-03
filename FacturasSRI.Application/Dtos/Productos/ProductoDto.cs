namespace FacturasSRI.Application.Dtos.Productos;
public class ProductoDto
{
    public Guid Id { get; set; }
    public string CodigoPrincipal { get; set; }
    public string Nombre { get; set; }
    public decimal PrecioVentaUnitario { get; set; }
    public bool EstaActivo { get; set; }
}