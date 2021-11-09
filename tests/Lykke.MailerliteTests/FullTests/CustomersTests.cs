using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Lykke.Mailerlite.ApiClient;
using Lykke.Mailerlite.ApiContract;
using Lykke.Mailerlite.Common.Domain.Customers;
using Lykke.Mailerlite.Common.Persistence;
using Lykke.Mailerlite.Common.Persistence.Customers;
using Lykke.MailerliteTests.Sdk;
using MassTransit.JobService.Components.StateMachines;
using Moq;
using Moq.Protected;
using Newtonsoft.Json.Linq;
using Shouldly;
using Swisschain.Extensions.Testing;
using Xunit;
using Xunit.Abstractions;
using RabbitMqFixture = Lykke.MailerliteTests.Sdk.RabbitMqFixture;

namespace Lykke.MailerliteTests.FullTests
{
    public class CustomersTests : IClassFixture<PersistenceFixture>, IClassFixture<RabbitMqFixture>, IAsyncLifetime
    {
        private readonly ApiFixture _apiFixture;
        private readonly WorkerFixture _workerFixture;
        private readonly ILykkeMailerliteClient _lykkeMailerliteClient;
        private readonly PersistenceFixture _persistenceFixture;

        public CustomersTests(PersistenceFixture persistenceFixture, RabbitMqFixture rabbitMqFixture, ITestOutputHelper testOutputHelper)
        {
            _apiFixture = new ApiFixture(testOutputHelper, rabbitMqFixture);
            _workerFixture = new WorkerFixture(rabbitMqFixture, persistenceFixture, testOutputHelper);
            _lykkeMailerliteClient = new LykkeMailerliteClient("http://localhost:5001");
            _persistenceFixture = persistenceFixture;
        }

        [Fact]
        public async Task CreatesCustomerInMailerlite()
        {
            var id = Guid.NewGuid().ToString();
            var email = $"{Guid.NewGuid():N}@fake-mail.com";
            var defaultKyc = "DefaultState";
            
            await _lykkeMailerliteClient.Customers.CreateAsync(new CreateCustomerRequest
            {
                RequestId = Guid.NewGuid().ToString(),
                CustomerId = id,
                Email = email,
                KycState = defaultKyc,
                Timestamp = DateTime.UtcNow.ToTimestamp()
            });

            var customerFromDb = await WaitUntilCustomerCreatedAsync(id);
            
            _workerFixture.MockHttpMessageHandler.Protected().Verify<Task<HttpResponseMessage>>(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req => (
                    req.Method == HttpMethod.Post &&
                    Uri.Compare(req.RequestUri, new Uri(_workerFixture.CustomerCreateUrl), UriComponents.Host | UriComponents.PathAndQuery, UriFormat.SafeUnescaped, StringComparison.OrdinalIgnoreCase) == 0 &&
                    req.Content.Headers.Any(x => x.Key == ("X-MailerLite-ApiKey") && x.Value.Any(y => y == _workerFixture.MailerliteApiKey)) &&
                    JObject.Parse(req.Content.ReadAsStringAsync().GetAwaiter().GetResult())["email"].Value<string>() == email
                )),
                ItExpr.IsAny<CancellationToken>()
            );
            
            _workerFixture.MockHttpMessageHandler.Protected().Verify<Task<HttpResponseMessage>>(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req => (
                    req.Method == HttpMethod.Put &&
                    Uri.Compare(req.RequestUri, new Uri(string.Format(_workerFixture.CustomerUpdateFieldUrl, email)), UriComponents.Host | UriComponents.PathAndQuery, UriFormat.SafeUnescaped, StringComparison.OrdinalIgnoreCase) == 0 &&
                    req.Content.Headers.Any(x => x.Key == ("X-MailerLite-ApiKey") && x.Value.Any(y => y == _workerFixture.MailerliteApiKey)) &&
                    JObject.Parse(req.Content.ReadAsStringAsync().GetAwaiter().GetResult())["fields"].Any(v => (new JObject(v).Properties().Any(p => p.Name.Contains("kycstatus")))) &&
                    JObject.Parse(req.Content.ReadAsStringAsync().GetAwaiter().GetResult())["fields"]["kycstatus"].Value<string>() == defaultKyc                )),
                ItExpr.IsAny<CancellationToken>()
            );
            
            _workerFixture.MockHttpMessageHandler.Protected().Verify<Task<HttpResponseMessage>>(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req => (
                    req.Method == HttpMethod.Put &&
                    Uri.Compare(req.RequestUri, new Uri(string.Format(_workerFixture.CustomerUpdateFieldUrl, email)), UriComponents.Host | UriComponents.PathAndQuery, UriFormat.SafeUnescaped, StringComparison.OrdinalIgnoreCase) == 0 &&
                    req.Content.Headers.Any(x => x.Key == ("X-MailerLite-ApiKey") && x.Value.Any(y => y == _workerFixture.MailerliteApiKey)) &&
                    JObject.Parse(req.Content.ReadAsStringAsync().GetAwaiter().GetResult())["fields"].Any(v => (new JObject(v).Properties().Any(p => p.Name.Contains("deposited")))) &&
                    JObject.Parse(req.Content.ReadAsStringAsync().GetAwaiter().GetResult())["fields"]["deposited"].Value<string>() == "false"
                )),
                ItExpr.IsAny<CancellationToken>()
            );
            
            customerFromDb.Email.ShouldBe(email);
            customerFromDb.Id.ShouldBe(id);
            customerFromDb.KycState.ShouldBe(defaultKyc);
            customerFromDb.Deposited.ShouldBeFalse();

            var updatedKyc = "NewKycState";
            
            await _lykkeMailerliteClient.Customers.UpdateKycAsync(new UpdateCustomerKycRequest
            {
                RequestId = Guid.NewGuid().ToString(),
                CustomerId = id,
                KycState = updatedKyc,
                Timestamp = DateTime.UtcNow.ToTimestamp()
            });
            
            customerFromDb = await WaitUntilCustomerCreatedAsync(id, updatedKyc);
            
            _workerFixture.MockHttpMessageHandler.Protected().Verify<Task<HttpResponseMessage>>(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req => (
                    req.Method == HttpMethod.Put &&
                    Uri.Compare(req.RequestUri, new Uri(string.Format(_workerFixture.CustomerUpdateFieldUrl, email)), UriComponents.Host | UriComponents.PathAndQuery, UriFormat.SafeUnescaped, StringComparison.OrdinalIgnoreCase) == 0 &&
                    req.Content.Headers.Any(x => x.Key == ("X-MailerLite-ApiKey") && x.Value.Any(y => y == _workerFixture.MailerliteApiKey)) &&
                    JObject.Parse(req.Content.ReadAsStringAsync().GetAwaiter().GetResult())["fields"].Any(v => (new JObject(v).Properties().Any(p => p.Name.Contains("kycstatus")))) &&
                    JObject.Parse(req.Content.ReadAsStringAsync().GetAwaiter().GetResult())["fields"]["kycstatus"].Value<string>() == updatedKyc
                )),
                ItExpr.IsAny<CancellationToken>()
            );
            
            customerFromDb.Email.ShouldBe(email);
            customerFromDb.Id.ShouldBe(id);
            customerFromDb.KycState.ShouldBe(updatedKyc);
            customerFromDb.Deposited.ShouldBeFalse();
            
            await _lykkeMailerliteClient.Customers.UpdateDepositAsync(new UpdateCustomerDepositRequest
            {
                RequestId = Guid.NewGuid().ToString(),
                CustomerId = id,
                Timestamp = DateTime.UtcNow.ToTimestamp()
            });
            
            customerFromDb = await WaitUntilCustomerCreatedAsync(id, deposited: true);
            
            _workerFixture.MockHttpMessageHandler.Protected().Verify<Task<HttpResponseMessage>>(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req => (
                    req.Method == HttpMethod.Put &&
                    Uri.Compare(req.RequestUri, new Uri(string.Format(_workerFixture.CustomerUpdateFieldUrl, email)), UriComponents.Host | UriComponents.PathAndQuery, UriFormat.SafeUnescaped, StringComparison.OrdinalIgnoreCase) == 0 &&
                    req.Content.Headers.Any(x => x.Key == ("X-MailerLite-ApiKey") && x.Value.Any(y => y == _workerFixture.MailerliteApiKey)) &&
                    JObject.Parse(req.Content.ReadAsStringAsync().GetAwaiter().GetResult())["fields"].Any(v => (new JObject(v).Properties().Any(p => p.Name.Contains("deposited")))) &&
                    JObject.Parse(req.Content.ReadAsStringAsync().GetAwaiter().GetResult())["fields"]["deposited"].Value<string>() == "true"
                )),
                ItExpr.IsAny<CancellationToken>()
            );
            
            customerFromDb.Email.ShouldBe(email);
            customerFromDb.Id.ShouldBe(id);
            customerFromDb.KycState.ShouldBe(updatedKyc);
            customerFromDb.Deposited.ShouldBeTrue();
        }

        private async Task<Customer> WaitUntilCustomerCreatedAsync(string id, string kyc=default, bool? deposited=default)
        {
            var context = new DatabaseContext(_persistenceFixture.DbContextOptionsBuilder.Options);
            var customersRepository = new CustomersRepository(context);

            while (true)
            {
                await Task.Delay(1000);

                var customer = await customersRepository.GetOrDefault(id);

                if (customer != null)
                {
                    if (kyc != default && customer.KycState != kyc)
                        continue;
                    
                    if(deposited != default && customer.Deposited != deposited.Value)
                        continue;

                    return customer;
                }
            }
        }

        public async Task InitializeAsync()
        {
            await _apiFixture.InitializeAsync();
            await _workerFixture.InitializeAsync();
        }

        public async Task DisposeAsync()
        {
            await _apiFixture.DisposeAsync();
            await _workerFixture.DisposeAsync();
        }
    }
}
