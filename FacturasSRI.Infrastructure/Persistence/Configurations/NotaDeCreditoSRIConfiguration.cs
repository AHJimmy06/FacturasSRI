using FacturasSRI.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FacturasSRI.Infrastructure.Persistence.Configurations
{
    public class NotaDeCreditoSRIConfiguration : IEntityTypeConfiguration<NotaDeCreditoSRI>
    {
        public void Configure(EntityTypeBuilder<NotaDeCreditoSRI> builder)
        {
            builder.HasKey(ns => ns.NotaDeCreditoId);

            builder.HasOne(ns => ns.NotaDeCredito)
                .WithOne(n => n.InformacionSRI)
                .HasForeignKey<NotaDeCreditoSRI>(ns => ns.NotaDeCreditoId);
        }
    }
}
