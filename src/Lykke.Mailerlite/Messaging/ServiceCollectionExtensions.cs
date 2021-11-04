using System;
using Lykke.Mailerlite.Common.Commands;
using Lykke.Mailerlite.Common.Configuration;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Swisschain.Extensions.MassTransit;

namespace Lykke.Mailerlite.Messaging
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddMessaging(this IServiceCollection services, RabbitMqConfig rabbitMqConfig)
        {
            ConfigureCommands();
            
            services.AddMassTransit(x =>
            {
                var schedulerEndpoint = new Uri("queue:lykke-pulsar");

                x.AddMessageScheduler(schedulerEndpoint);
                
                x.UsingRabbitMq((context, cfg) =>
                {
                    cfg.Host(rabbitMqConfig.HostUrl,
                        host =>
                        {
                            host.Username(rabbitMqConfig.Username);
                            host.Password(rabbitMqConfig.Password);
                        });
                    
                    cfg.UseMessageScheduler(schedulerEndpoint);

                    cfg.UseDefaultRetries(context);
                });
            });
            
            services.AddMassTransitBusHost();

            return services;
        }

        private static void ConfigureCommands()
        {
            EndpointConvention.Map<CreateCustomerCommand>(new Uri("queue:lykke-mailerlite-create-customer-command"));
            EndpointConvention.Map<UpdateCustomerKycCommand>(new Uri("queue:lykke-mailerlite-update-customer-kyc-command"));
            EndpointConvention.Map<UpdateCustomerDepositedCommand>(new Uri("queue:lykke-mailerlite-update-customer-deposited-command"));
        }
    }
}
