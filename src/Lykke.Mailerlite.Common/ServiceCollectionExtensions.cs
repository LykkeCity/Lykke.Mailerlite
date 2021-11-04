using System.Net.Http;
using Lykke.Mailerlite.Common.Configuration;
using Lykke.Mailerlite.Common.Domain.Mailerlite;
using Microsoft.Extensions.DependencyInjection;

namespace Lykke.Mailerlite.Common
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddDomainDependencies(
            this IServiceCollection services,
            MailerliteConfig mailerliteConfig)
        {
            services.AddSingleton<IMailerliteClient>(
                new MailerliteClient(
                    new HttpClientHandler(),
                    mailerliteConfig));

            return services;
        }
    }
}
