using Lykke.Mailerlite.Common;
using Lykke.Mailerlite.Common.Commands;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Lykke.Mailerlite.Common.Configuration;
using Lykke.Mailerlite.Common.Persistence;
using Lykke.Mailerlite.Worker.Messaging;
using Swisschain.Extensions.EfCore;
using Swisschain.Extensions.Idempotency;
using Swisschain.Extensions.Idempotency.EfCore;
using Swisschain.Extensions.Idempotency.MassTransit;
using Swisschain.LykkeExchange.Lykke.Mailerlite.MessagingContract.Customers;
using Swisschain.Sdk.Server.Common;

namespace Lykke.Mailerlite.Worker
{
    public sealed class MailerliteWorkerStartup : SwisschainStartup<AppConfig>
    {
        public MailerliteWorkerStartup(IConfiguration configuration)
            : base(configuration)
        {
        }

        protected override void ConfigureServicesExt(IServiceCollection services)
        {
            base.ConfigureServicesExt(services);

            services
                .AddHttpClient()
                .AddPersistence(Config.Db.ConnectionString)
                .AddDomainDependencies(Config.Mailerlite)
                .AddEfCoreDbMigration(options =>
                {
                    options.UseDbContextFactory(factory =>
                    {
                        var builder = factory.GetRequiredService<DbContextOptionsBuilder<DatabaseContext>>();
                        var context = new DatabaseContext(builder.Options);
                        return context;
                    });
                })
                .AddIdempotency<UnitOfWork>(c =>
                {
                    c.DispatchWithMassTransit();
                    c.PersistWithEfCore(s =>
                    {
                        var optionsBuilder = s.GetRequiredService<DbContextOptionsBuilder<DatabaseContext>>();

                        return new DatabaseContext(optionsBuilder.Options);
                    }, o =>
                    {
                        o.OutboxDeserializer.AddAssembly(typeof(CustomerCreated).Assembly);
                        o.OutboxDeserializer.AddAssembly(typeof(CreateCustomerCommand).Assembly);
                    });
                })
                .AddMessaging(Config.RabbitMq);
        }
    }
}
