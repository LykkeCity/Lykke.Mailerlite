using System;
using Lykke.Mailerlite.Common.Commands;
using Lykke.Mailerlite.Common.Configuration;
using MassTransit;
using MassTransit.RabbitMqTransport;
using Microsoft.Extensions.DependencyInjection;
using Lykke.Mailerlite.Worker.Messaging.Consumers;
using Swisschain.Extensions.MassTransit;

namespace Lykke.Mailerlite.Worker.Messaging
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddMessaging(this IServiceCollection services, RabbitMqConfig rabbitMqConfig)
        {
            services.AddTransient<CreateCustomerCommandConsumer>();
            services.AddTransient<UpdateCustomerKycCommandConsumer>();
            services.AddTransient<UpdateCustomerDepositedCommandConsumer>();

            ConfigureCommands();
            
            services.AddMassTransit(x =>
            {
                var schedulerEndpoint = new Uri("queue:lykke-pulsar");

                x.AddMessageScheduler(schedulerEndpoint);
                
                x.UsingRabbitMq((context, cfg) =>
                {
                    if (rabbitMqConfig.Port == 0)
                    {
                        cfg.Host(rabbitMqConfig.HostUrl,
                            host =>
                            {
                                host.Username(rabbitMqConfig.Username);
                                host.Password(rabbitMqConfig.Password);
                            });
                    }
                    else
                    {
                        cfg.Host(rabbitMqConfig.HostUrl,
                            rabbitMqConfig.Port,
                            rabbitMqConfig.VirtualHost,
                            host =>
                            {
                                host.Username(rabbitMqConfig.Username);
                                host.Password(rabbitMqConfig.Password);
                            });
                    }
                    
                    cfg.UseMessageScheduler(schedulerEndpoint);

                    cfg.UseDefaultRetries(context);

                    ConfigureReceivingEndpoints(cfg, context);
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

        private static void ConfigureReceivingEndpoints(IBusFactoryConfigurator cfg, IBusRegistrationContext context)
        {
            cfg.ReceiveEndpoint("lykke-mailerlite-create-customer-command", e => { e.Consumer(context.GetRequiredService<CreateCustomerCommandConsumer>); });
            cfg.ReceiveEndpoint("lykke-mailerlite-update-customer-kyc-command", e => { e.Consumer(context.GetRequiredService<UpdateCustomerKycCommandConsumer>); });
            cfg.ReceiveEndpoint("lykke-mailerlite-update-customer-deposited-command", e => { e.Consumer(context.GetRequiredService<UpdateCustomerDepositedCommandConsumer>); });
        }
    }
}
