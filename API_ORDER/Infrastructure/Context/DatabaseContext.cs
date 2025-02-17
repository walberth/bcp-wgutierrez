using API_ORDER.Domain.Client;
using API_ORDER.Domain.Order;
using Microsoft.EntityFrameworkCore;

namespace API_ORDER.Infrastructure.Context
{
    public class DatabaseContext : DbContext
    {
        public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options)
        {
        }

        public virtual DbSet<Order> Order { get; set; }
        public virtual DbSet<Client> Client { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Order>(entity =>
            {
                entity.HasOne(p => p.Client)
                    .WithOne(p => p.Order)
                    .HasForeignKey<Order>(p => p.IdCliente);
            });
        }
    }
}
