using System;
using System.Collections.Generic;

namespace FacturasSRI.Application.Dtos
{
    public class ProductDetailDto : ProductDto
    {
        public List<TaxDto> Taxes { get; set; } = new();
    }
}
