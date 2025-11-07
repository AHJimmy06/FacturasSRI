using FacturasSRI.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FacturasSRI.Infrastructure.Persistence.Configurations
{
    public class FacturaSRIConfiguration : IEntityTypeConfiguration<FacturaSRI>
    {
        public void Configure(EntityTypeBuilder<FacturaSRI> builder)
        {
            builder.HasKey(fs => fs.FacturaId);

            builder.HasOne(fs => fs.Factura)
                .WithOne(f => f.InformacionSRI)
                .HasForeignKey<FacturaSRI>(fs => fs.FacturaId);
        }
    }
}
