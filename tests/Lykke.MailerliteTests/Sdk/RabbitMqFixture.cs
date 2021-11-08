using System.Threading.Tasks;
using Swisschain.Extensions.Testing;
using Swisschain.Extensions.Testing.DockerContainers.RabbitMq;
using Xunit;

namespace Lykke.MailerliteTests.Sdk
{
    public class RabbitMqFixture : IAsyncLifetime
    {
        private readonly RabbitMqContainer _container;
        
        public RabbitMqFixture()
        {
            var rabbitContainerName = "tests-rabbit";
            _container = new RabbitMqContainer(
                rabbitContainerName, 
                PortManager.GetNextPort(), 
                PortManager.GetNextPort());
        }

        public string AmpqUrl => _container.AmpqUrl;
        public string User => _container.User;
        public string Password => _container.Password;
        public string VirtualHost => "/";

        public async Task InitializeAsync()
        {
            await _container.Start();
        }

        public async Task DisposeAsync()
        {
            _container.Stop();
        }
    }
}
