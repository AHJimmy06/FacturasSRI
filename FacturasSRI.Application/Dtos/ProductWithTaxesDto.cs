using System;
using System.Collections.Generic;

namespace FacturasSRI.Application.Dtos
{
    public class ProductWithTaxesDto : ProductDto
    {
        public List<Guid> TaxIds { get; set; } = new();
    }
}
