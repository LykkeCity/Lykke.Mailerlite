using System;
using System.Collections.Generic;
using System.Linq;
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
        public string AddCustomerToGroupUrl { set; get; }
        public string FindGroupIdByNameUrl { set; get; }
        public string DeleteCustomerFromGroupUrl { set; get; }
        public IEnumerable<string> NewCustomerFromAllAreasGroups { set; get; }
        public IEnumerable<string> NewCustomerFromUnrestrictedAreaGroups { set; get; }
        public string KycReminderGroup { set; get; }
        public IEnumerable<string> StatusesToDeleteFromKycReminderGroup { set; get; }
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
            AddCustomerToGroupUrl = "http://fakeurl.com/groups/add";
            FindGroupIdByNameUrl = "http://fakeurl.com/groups/search";
            DeleteCustomerFromGroupUrl = "http://fakeurl.com/groups/delete/{0}/{1}";
            NewCustomerFromAllAreasGroups = new List<string>
            {
                "test1",
                "test2"
            };
            NewCustomerFromUnrestrictedAreaGroups = new List<string>
            {
                "test1",
                "test2"
            };
            KycReminderGroup = "test_kyc";
            StatusesToDeleteFromKycReminderGroup = new List<string>
            {
                "status1",
                "status2"
            };
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
                {"RabbitMq:VirtualHost", _rabbitMqFixture.VirtualHost},
                {"Mailerlite:CustomerCreateUrl", CustomerCreateUrl},
                {"Mailerlite:ApiKey", MailerliteApiKey},
                {"Mailerlite:CustomerUpdateFieldUrl", CustomerUpdateFieldUrl},
                {"Mailerlite:AddCustomerToGroupUrl", AddCustomerToGroupUrl},
                {"Mailerlite:FindGroupIdByNameUrl", FindGroupIdByNameUrl},
                {"Mailerlite:DeleteCustomerFromGroupUrl", DeleteCustomerFromGroupUrl},
                {"Mailerlite:KycReminderGroup", KycReminderGroup},
            };
            var newCustomerFromAllAreasGroupsList = NewCustomerFromAllAreasGroups.ToList();
            for (int index = 0; index < newCustomerFromAllAreasGroupsList.Count; index++)
            {
                defaultAppSettings.Add($"Mailerlite:NewCustomerFromAllAreasGroups:{index}", newCustomerFromAllAreasGroupsList[index]);
            }
            var newCustomerFromUnrestrictedAreaGroupsList = NewCustomerFromUnrestrictedAreaGroups.ToList();
            for (int index = 0; index < newCustomerFromUnrestrictedAreaGroupsList.Count; index++)
            {
                defaultAppSettings.Add($"Mailerlite:NewCustomerFromUnrestrictedAreaGroups:{index}", newCustomerFromUnrestrictedAreaGroupsList[index]);
            }
            var statusesToDeleteFromKycReminderGroupList = StatusesToDeleteFromKycReminderGroup.ToList();
            for (int index = 0; index < statusesToDeleteFromKycReminderGroupList.Count; index++)
            {
                defaultAppSettings.Add($"Mailerlite:StatusesToDeleteFromKycReminderGroup:{index}", statusesToDeleteFromKycReminderGroupList[index]);
            }
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
                                        CustomerUpdateFieldUrl = CustomerUpdateFieldUrl,
                                        AddCustomerToGroupUrl = AddCustomerToGroupUrl,
                                        FindGroupIdByNameUrl = FindGroupIdByNameUrl,
                                        DeleteCustomerFromGroupUrl = DeleteCustomerFromGroupUrl,
                                        NewCustomerFromAllAreasGroups = NewCustomerFromAllAreasGroups,
                                        NewCustomerFromUnrestrictedAreaGroups = NewCustomerFromUnrestrictedAreaGroups,
                                        KycReminderGroup = KycReminderGroup,
                                        StatusesToDeleteFromKycReminderGroup = StatusesToDeleteFromKycReminderGroup
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
