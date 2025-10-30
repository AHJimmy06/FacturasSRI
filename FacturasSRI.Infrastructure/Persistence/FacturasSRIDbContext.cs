
using Microsoft.EntityFrameworkCore;

namespace FacturasSRI.Infrastructure.Persistence
{
    public class FacturasSRIDbContext : DbContext
    {
        public FacturasSRIDbContext(DbContextOptions<FacturasSRIDbContext> options) : base(options)
        {
        }
    }
}
