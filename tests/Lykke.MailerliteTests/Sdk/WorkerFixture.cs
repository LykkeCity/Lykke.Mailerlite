using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Lykke.Mailerlite;
using Lykke.Mailerlite.Common.Configuration;
using Lykke.Mailerlite.Common.Domain.Mailerlite;
using Lykke.Mailerlite.Worker;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using Swisschain.Extensions.Testing;
using Swisschain.Sdk.Server.Common;
using Xunit;
using Xunit.Abstractions;

namespace Lykke.MailerliteTests.Sdk
{
    public class WorkerFixture
    {
        private readonly RabbitMqFixture _rabbitMqFixture;
        private readonly PersistenceFixture _persistenceFixture;
        private readonly LoggerFactory _loggerFactory;
        private IHost _host;
        
        public Mock<HttpMessageHandler> MockHttpMessageHandler { get; }
        public string CustomerCreateUrl { get; }
        public string CustomerUpdateFieldUrl { get; }
        public string MailerliteApiKey { get; }

        public WorkerFixture(RabbitMqFixture rabbitMqFixture, PersistenceFixture persistenceFixture, ITestOutputHelper testOutputHelper)
        {
            _rabbitMqFixture = rabbitMqFixture;
            _persistenceFixture = persistenceFixture;
            _loggerFactory = new LoggerFactory();
            _loggerFactory.AddProvider(new XunitLoggerProvider(testOutputHelper));
            
            MockHttpMessageHandler = new Mock<HttpMessageHandler>();
            MockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage { StatusCode = HttpStatusCode.OK });

            MailerliteApiKey = "fakekey";
            CustomerCreateUrl = "http://fakeurl.com";
            CustomerUpdateFieldUrl = "http://fakeurl.com/{0}";
        }
        
        public async Task InitializeAsync()
        {
            await _persistenceFixture.CreateTestDb();
            await _persistenceFixture.ApplyMigrations();
            
            var defaultAppSettings = new Dictionary<string, string>
            {
                { "implementationInstance", "test_worker_instance" },
                { "Db:ConnectionString", _persistenceFixture.GetConnectionString("test_db") },
                {"RabbitMq:HostUrl", _rabbitMqFixture.AmpqUrl},
                {"RabbitMq:Username", _rabbitMqFixture.User},
                {"RabbitMq:Password", _rabbitMqFixture.Password},
                {"Mailerlite:CustomerCreateUrl", CustomerCreateUrl},
                {"Mailerlite:ApiKey", MailerliteApiKey},
                {"Mailerlite:CustomerUpdateFieldUrl", CustomerUpdateFieldUrl},
            };
            var hostBuilder = new HostBuilder()
                .SwisschainService<MailerliteWorkerStartup>(
                    options =>
                    {
                        options.UsePorts(5002, 5003);
                        options.UseLoggerFactory(_loggerFactory);
                    },
                    builder =>
                    {
                        builder.ConfigureTestServices(services =>
                        {
                            services.AddSingleton<IMailerliteClient>(
                                new MailerliteClient(
                                    MockHttpMessageHandler.Object,
                                    new MailerliteConfig
                                    {
                                        ApiKey = MailerliteApiKey,
                                        CustomerCreateUrl = CustomerCreateUrl,
                                        CustomerUpdateFieldUrl = CustomerUpdateFieldUrl
                                    }));
                        });
                    },
                    (context, builder) =>
                    {
                        builder.Sources.Clear();
                        builder.AddInMemoryCollection(defaultAppSettings);
                    });

            _host = hostBuilder.Build();

            await _host.StartAsync();
        }

        public async Task DisposeAsync()
        {
            await _host.StopAsync();
            _host.Dispose();
            await _persistenceFixture.DropTestDb();
        }
    }
}
