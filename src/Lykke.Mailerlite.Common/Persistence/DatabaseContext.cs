using Lykke.Mailerlite.Common.Persistence.Customers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Swisschain.Extensions.Idempotency.EfCore;

namespace Lykke.Mailerlite.Common.Persistence
{
    public class DatabaseContext : DbContext, IDbContextWithOutbox, IDbContextWithIdGenerator
    {
        public static string SchemaName { get; } = "lykke-mailerlite";
        public static string MigrationHistoryTable { get; } = HistoryRepository.DefaultTableName;

        public DatabaseContext(DbContextOptions<DatabaseContext> options)
            : base(options)
        {
        }
        
        public DbSet<OutboxEntity> Outbox { get; set; }
        public DbSet<IdGeneratorEntity> IdGenerator { get; set; }
        public DbSet<CustomerEntity> Customers { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);

            optionsBuilder.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema(SchemaName);

            modelBuilder.BuildIdempotency(c => { c.AddIdGenerator("id_generator_deposit_updates", 500_000_000); });

            BuildCustomers(modelBuilder);

            base.OnModelCreating(modelBuilder);
        }
        
        private static void BuildCustomers(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CustomerEntity>()
                .ToTable(Tables.Customers)
                .HasKey(c => c.Id);
        }
    }
}
