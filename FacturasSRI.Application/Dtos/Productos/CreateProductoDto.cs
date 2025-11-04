using FacturasSRI.Domain.Enums;
namespace FacturasSRI.Application.Dtos.Productos;
public class CreateProductoDto
{
    public string CodigoPrincipal { get; set; }
    public string Nombre { get; set; }
    public string Descripcion { get; set; }
    public decimal PrecioVentaUnitario { get; set; }
    public TipoImpuestoIVA TipoImpuestoIVA { get; set; } 
}