using FacturasSRI.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FacturasSRI.Infrastructure.Persistence.Configurations
{
    public class ProductoImpuestoConfiguration : IEntityTypeConfiguration<ProductoImpuesto>
    {
        public void Configure(EntityTypeBuilder<ProductoImpuesto> builder)
        {
            builder.HasKey(pi => new { pi.ProductoId, pi.ImpuestoId });

            builder.HasOne(pi => pi.Producto)
                .WithMany(p => p.ProductoImpuestos)
                .HasForeignKey(pi => pi.ProductoId);

            builder.HasOne(pi => pi.Impuesto)
                .WithMany(i => i.ProductoImpuestos)
                .HasForeignKey(pi => pi.ImpuestoId);
        }
    }
}
