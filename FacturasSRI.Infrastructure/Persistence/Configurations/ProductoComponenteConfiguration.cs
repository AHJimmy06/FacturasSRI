using FacturasSRI.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FacturasSRI.Infrastructure.Persistence.Configurations
{
    public class ProductoComponenteConfiguration : IEntityTypeConfiguration<ProductoComponente>
    {
        public void Configure(EntityTypeBuilder<ProductoComponente> builder)
        {
            builder.HasKey(pc => pc.Id);

            builder.HasOne(pc => pc.ProductoKit)
                .WithMany(p => p.Componentes)
                .HasForeignKey(pc => pc.ProductoKitId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(pc => pc.ProductoComponenteItem)
                .WithMany()
                .HasForeignKey(pc => pc.ProductoComponenteId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
