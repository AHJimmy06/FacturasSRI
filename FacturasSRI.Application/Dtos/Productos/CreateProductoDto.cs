using FacturasSRI.Domain.Enums;
namespace FacturasSRI.Application.Dtos.Productos;
public class CreateProductoDto
{
    public string CodigoPrincipal { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public decimal PrecioVentaUnitario { get; set; }
    public TipoImpuestoIVA TipoImpuestoIVA { get; set; } 
}