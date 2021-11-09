using System;
using System.Threading.Tasks;
using Grpc.Core;
using Lykke.Mailerlite.ApiContract;
using Microsoft.Extensions.DependencyInjection;
using Swisschain.Extensions.Grpc.Abstractions;
using Swisschain.Extensions.Grpc.Abstractions.ServiceDeadline;

namespace Lykke.Mailerlite.ApiClient
{
    public static class LykkeMailerliteServiceCollectionExtensions
    {
        private const string Prefix = "Lykke-Mailerlite-Api";

        private static string NameFor<T>()
        {
            return $"{Prefix}-{typeof(T).FullName}";
        }
        
        public static IServiceCollection AddMailerliteClient(this IServiceCollection serviceCollection, LykkeMailerliteServiceOptions options)
        {
            if (options.GrpcServiceUrl == null)
            {
                throw new ArgumentNullException(nameof(options.GrpcServiceUrl));
            }
            
            serviceCollection.AddGrpcClient<Customers.CustomersClient>(options);

            serviceCollection.AddTransient<ILykkeMailerliteClient, LykkeLykkeMailerliteClient>();
            
            return serviceCollection;
        }
        
        
        private static void AddGrpcClient<T>(
            this IServiceCollection serviceCollection,
            LykkeMailerliteServiceOptions options) where T : class
        {
            serviceCollection
                .AddGrpcClient<T>(NameFor<T>(), o => o.Address = options.GrpcServiceUrl)
                .WithGrpcGlobalDeadline(new GlobalDeadlineInterceptorOptions { Timeout = options.Timeout });
        }
    }
}
