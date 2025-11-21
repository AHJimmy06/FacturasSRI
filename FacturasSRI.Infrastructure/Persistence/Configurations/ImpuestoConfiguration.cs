using FacturasSRI.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FacturasSRI.Infrastructure.Persistence.Configurations
{
    public class ImpuestoConfiguration : IEntityTypeConfiguration<Impuesto>
    {
        public void Configure(EntityTypeBuilder<Impuesto> builder)
        {
            builder.HasIndex(i => i.Nombre).IsUnique();
            builder.HasIndex(i => i.CodigoSRI).IsUnique();
        }
    }
}
