using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Mailerlite;
using Lykke.Mailerlite.Messaging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Swisschain.Extensions.Testing;
using Swisschain.Sdk.Server.Common;
using Swisschain.Sdk.Server.Configuration;
using Xunit;
using Xunit.Abstractions;

namespace Lykke.MailerliteTests.Sdk
{
    public class ApiFixture : IAsyncLifetime
    {
        private readonly LoggerFactory _loggerFactory;
        private IHost _host;
        private RabbitMqFixture _rabbitMqFixture;

        public ApiFixture(ITestOutputHelper testOutputHelper, RabbitMqFixture rabbitMqFixture)
        {
            _rabbitMqFixture = rabbitMqFixture;
            _loggerFactory = new LoggerFactory();
            _loggerFactory.AddProvider(new XunitLoggerProvider(testOutputHelper));
        }
        
        public async Task InitializeAsync()
        {
            var defaultAppSettings = new Dictionary<string, string>
            {
                { "implementationInstance", "test_api_instance" },
                {"RabbitMq:HostUrl", _rabbitMqFixture.AmpqUrl},
                {"RabbitMq:Username", _rabbitMqFixture.User},
                {"RabbitMq:Password", _rabbitMqFixture.Password},
            };
            var hostBuilder = new HostBuilder()
                .SwisschainService<MailerliteApiStartup>(
                    options => { options.UseLoggerFactory(_loggerFactory); },
                    builder => { },
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
        }
    }
}
