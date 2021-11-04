using System.Threading.Tasks;
using Lykke.Mailerlite.Common.Persistence;
using Microsoft.EntityFrameworkCore;
using Swisschain.Extensions.Testing;

namespace Lykke.MailerliteTests.Sdk
{
    public class RepositoryPersistenceFixture : PostgresFixture
    {
        public RepositoryPersistenceFixture() :
            base("lykke-mailerlite-test-postgres-persistence")
        {
        }

        public DbContextOptionsBuilder<DatabaseContext> DbContextOptionsBuilder { get; private set; }

        public async Task ApplyMigrations()
        {
            await using var context = new DatabaseContext(DbContextOptionsBuilder.Options);
            await context.Database.MigrateAsync();
        }

        protected override async Task InitializeAsync()
        {
            await base.InitializeAsync();

            DbContextOptionsBuilder = CreateDbContextOptionsBuilder();
        }

        private DbContextOptionsBuilder<DatabaseContext> CreateDbContextOptionsBuilder()
        {
            var dbContextOptionsBuilder = new DbContextOptionsBuilder<DatabaseContext>();

            dbContextOptionsBuilder
                .UseNpgsql(Container.GetConnectionString("test_db"),
                    builder =>
                        builder.MigrationsHistoryTable(
                            DatabaseContext.MigrationHistoryTable,
                            DatabaseContext.SchemaName));

            return dbContextOptionsBuilder;
        }
    }
}
