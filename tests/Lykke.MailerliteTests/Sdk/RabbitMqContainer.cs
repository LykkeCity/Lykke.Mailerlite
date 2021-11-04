using System;
using System.Diagnostics;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Ductus.FluentDocker.Builders;
using Ductus.FluentDocker.Services;
using RabbitMQ.Client;
using Swisschain.Extensions.Testing.DockerContainers;
using Swisschain.Extensions.Testing.DockerContainers.Postgres;

namespace Lykke.MailerliteTests.Sdk
{
    public class RabbitMqContainer
    {
        private readonly IContainerService _containerService;
        private readonly int _containerAmpqPort = 5672;
        private readonly int _containerManagementPort = 15672;

        public RabbitMqContainer(string containerName = "",
            int hostAmpqPort = 5672,
            int hostManagementPort = 15672,
            string user = "rabbit",
            string password = "pass",
            bool reuseIfExists = false,
            string version = "3.8.6-management-alpine")
        {
            this.HostAmpqPort = hostAmpqPort;
            this.HostManagementPort = hostManagementPort;
            this.User = user;
            this.Password = password;
            var str = "rabbitmq:" + version;
            var containerBuilder = new Builder().UseContainer()
                .WithName(containerName)
                .UseImage(str)
                .ExposePort(hostAmpqPort, this._containerAmpqPort)
                .ExposePort(hostManagementPort, this._containerManagementPort)
                .WaitForPort(string.Format("{0}/tcp", (object)this._containerAmpqPort), TimeSpan.FromMinutes(2.0))
                .WithEnvironment("RABBITMQ_DEFAULT_USER=" + user, "RABBITMQ_DEFAULT_PASS=" + password);
            if (reuseIfExists)
                containerBuilder.ReuseIfExists();
            else
                ContainerRemover.RemoveIfExists(containerName, str);
            this._containerService = containerBuilder.Build();
        }

        public int HostAmpqPort { get; }

        public int HostManagementPort { get; }

        public string User { get; }

        public string Password { get; }

        public string AmpqUrl => $"amqp://localhost:{HostAmpqPort}";

        public async Task Start()
        {
            this._containerService.Start();
            await new RabbitMqProbe(AmpqUrl, User, Password,
                TimeSpan.FromSeconds(1.0),
                TimeSpan.FromSeconds(30.0)).WaitUntilAvailable(CancellationToken.None);
        }

        public void Stop()
        {
            this._containerService.Stop();
            this._containerService.Remove();
        }
    }

    internal class RabbitMqProbe
    {
        private readonly string _host;
        private readonly TimeSpan _initialWaitTime;
        private readonly TimeSpan _maxWaitTime;
        private readonly string _username;
        private readonly string _password;

        public RabbitMqProbe(string host,
            string username,
            string password,
            TimeSpan initialWaitTime,
            TimeSpan maxWaitTime)
        {
            _host = host;
            _username = username;
            _password = password;
            _initialWaitTime = initialWaitTime;
            _maxWaitTime = maxWaitTime;
        }

        [DebuggerStepThrough]
        public async Task WaitUntilAvailable(CancellationToken cancellation)
        {
            await Task.Delay((int)_initialWaitTime.TotalMilliseconds, cancellation);

            var maxWaitTimeFromStart = DateTime.UtcNow.Add(_maxWaitTime);
            var factory = new ConnectionFactory()
            {
                Uri = new Uri(_host),
                UserName = _username,
                Password = _password
            };
            Exception lastException = null;
            while (DateTime.UtcNow < maxWaitTimeFromStart && !cancellation.IsCancellationRequested)
            {
                await Task.Delay(500, cancellation);

                try
                {
                    using var connection = factory.CreateConnection();
                    
                    if(connection.IsOpen)
                        return;
                }
                // TODO: Specific exception
                catch (Exception ex)
                {
                    lastException = ex;
                }
            }

            throw new TimeoutException($"The {nameof(RabbitMqContainer)} instance did not become available in a timely fashion.", lastException);
        }
    }
}
