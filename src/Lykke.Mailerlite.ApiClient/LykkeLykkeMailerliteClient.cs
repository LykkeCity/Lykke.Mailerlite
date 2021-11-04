using System;
using Grpc.Net.Client;
using Lykke.Mailerlite.ApiContract;

namespace Lykke.Mailerlite.ApiClient
{
    public class LykkeLykkeMailerliteClient : ILykkeMailerliteClient, IDisposable
    {
        private readonly GrpcChannel _channel;

        public LykkeLykkeMailerliteClient(string serverGrpcUrl)
        {
            AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

            _channel = GrpcChannel.ForAddress(serverGrpcUrl);

            Monitoring = new Monitoring.MonitoringClient(_channel);
            Customers = new Customers.CustomersClient(_channel);
        }

        public Monitoring.MonitoringClient Monitoring { get; }
        public Customers.CustomersClient Customers { get; }

        public void Dispose()
        {
            _channel?.Dispose();
        }
    }
}
