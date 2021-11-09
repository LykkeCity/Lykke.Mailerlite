using System;
using System.Net.Http;
using Grpc.Core;
using Grpc.Core.Interceptors;
using Grpc.Net.Client;
using Grpc.Net.Client.Web;
using Lykke.Mailerlite.ApiContract;

namespace Lykke.Mailerlite.ApiClient
{
    public class LykkeMailerliteClient : ILykkeMailerliteClient, IDisposable
    {
        private readonly GrpcChannel _channel;

        public LykkeMailerliteClient(string uri)
        {
            AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
            _channel = GrpcChannel.ForAddress(uri, new GrpcChannelOptions
            {
                HttpHandler = new GrpcWebHandler(new HttpClientHandler())
            });

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
