using FacturasSRI.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FacturasSRI.Infrastructure.Persistence.Configurations
{
    public class ClienteConfiguration : IEntityTypeConfiguration<Cliente>
    {
        public void Configure(EntityTypeBuilder<Cliente> builder)
        {
            builder.HasIndex(c => c.NumeroIdentificacion).IsUnique();
            builder.HasIndex(c => c.RazonSocial).IsUnique();
            builder.HasIndex(c => c.Telefono).IsUnique();
            builder.HasIndex(c => c.Email).IsUnique();
        }
    }
}
