using System;

namespace FacturasSRI.Application.Dtos
{
    public class CategoriaDto
    {
        public Guid Id { get; set; }
        public required string Nombre { get; set; }
    }
}