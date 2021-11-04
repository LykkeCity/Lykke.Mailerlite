using System.Threading.Tasks;
using Xunit;

namespace Lykke.MailerliteTests.Sdk
{
    [Collection(nameof(PersistenceTests))]
    public abstract class PersistenceTests : IClassFixture<RepositoryPersistenceFixture>, IAsyncLifetime
    {
        public PersistenceTests(RepositoryPersistenceFixture fixture)
        {
            Fixture = fixture;
        }

        public RepositoryPersistenceFixture Fixture { get; }

        public async Task InitializeAsync()
        {
            await Fixture.CreateTestDb();
            await Fixture.ApplyMigrations();
        }

        public async Task DisposeAsync()
        {
            await Fixture.DropTestDb();
        }
    }
}
