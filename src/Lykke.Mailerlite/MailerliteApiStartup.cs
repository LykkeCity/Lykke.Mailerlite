using System;
using Lykke.Mailerlite.Common;
using Lykke.Mailerlite.Common.Commands;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Lykke.Mailerlite.Common.Configuration;
using Lykke.Mailerlite.Common.Persistence;
using Lykke.Mailerlite.GrpcServices;
using Lykke.Mailerlite.Messaging;
using MassTransit;
using Swisschain.Extensions.EfCore;
using Swisschain.Sdk.Server.Common;

namespace Lykke.Mailerlite
{
    public sealed class MailerliteApiStartup : SwisschainStartup<AppConfig>
    {
        public MailerliteApiStartup(IConfiguration configuration)
            : base(configuration)
        {
        }

        protected override void ConfigureServicesExt(IServiceCollection services)
        {
            base.ConfigureServicesExt(services);

            services
                .AddMessaging(Config.RabbitMq);
        }

        protected override void RegisterEndpoints(IEndpointRouteBuilder endpoints)
        {
            base.RegisterEndpoints(endpoints);

            endpoints.MapGrpcService<MonitoringService>();
            endpoints.MapGrpcService<CustomersService>();
        }
    }
}
