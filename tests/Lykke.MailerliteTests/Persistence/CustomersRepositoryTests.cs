using System;
using System.Threading.Tasks;
using Lykke.Mailerlite.Common.Domain.Customers;
using Lykke.Mailerlite.Common.Persistence;
using Lykke.Mailerlite.Common.Persistence.Customers;
using Lykke.MailerliteTests.Sdk;
using Shouldly;
using Xunit;

namespace Lykke.MailerliteTests.Persistence
{
    public class CustomersRepositoryTests : PersistenceTests
    {
        public CustomersRepositoryTests(RepositoryPersistenceFixture fixture) : base(fixture)
        {
        }

        [Fact]
        public async Task CanCreateCustomerAndGetById()
        {
            var context = new DatabaseContext(Fixture.DbContextOptionsBuilder.Options);
            var customersRepository = new CustomersRepository(context);

            var id = Guid.NewGuid().ToString();
            var email = "email@mail.com";
            
            await customersRepository.AddOrIgnoreAsync(Customer.Create(id, email));

            var itemFromDb = await customersRepository.GetOrDefault(id);

            itemFromDb.ShouldNotBeNull();
            itemFromDb.Id.ShouldBe(id);
            itemFromDb.Email.ShouldBe(email);
            itemFromDb.KycState.ShouldBeNull();
            itemFromDb.Deposited.ShouldBeFalse();
            itemFromDb.KycStateTimestamp.ShouldBeNull();
        }

        [Fact]
        public async Task CanIgnoreExistingCustomer()
        {
            var context = new DatabaseContext(Fixture.DbContextOptionsBuilder.Options);
            var customersRepository = new CustomersRepository(context);

            var id = Guid.NewGuid().ToString();
            var email = "email@mail.com";
            
            await customersRepository.AddOrIgnoreAsync(Customer.Create(id, email));
            
            context = new DatabaseContext(Fixture.DbContextOptionsBuilder.Options);
            customersRepository = new CustomersRepository(context);
            
            await customersRepository.AddOrIgnoreAsync(Customer.Create(id, email));

            var itemFromDb = await customersRepository.GetOrDefault(id);

            itemFromDb.ShouldNotBeNull();
            itemFromDb.Id.ShouldBe(id);
            itemFromDb.Email.ShouldBe(email);
            itemFromDb.KycState.ShouldBeNull();
            itemFromDb.Deposited.ShouldBeFalse();
            itemFromDb.KycStateTimestamp.ShouldBeNull();
        }

        [Fact]
        public async Task CanUpdateCustomerKyc()
        {
            var context = new DatabaseContext(Fixture.DbContextOptionsBuilder.Options);
            var customersRepository = new CustomersRepository(context);

            var id = Guid.NewGuid().ToString();
            var email = "email2@mail.com";
            
            await customersRepository.AddOrIgnoreAsync(Customer.Create(id, email));

            var itemFromDb = await customersRepository.GetOrDefault(id);

            var kycState = "some_state";
            var timestamp = DateTime.UtcNow;
            
            itemFromDb.UpdateKycState(kycState, timestamp);
            
            context = new DatabaseContext(Fixture.DbContextOptionsBuilder.Options);
            customersRepository = new CustomersRepository(context);

            await customersRepository.Update(itemFromDb);
            
            itemFromDb = await customersRepository.GetOrDefault(id);
            
            itemFromDb.ShouldNotBeNull();
            itemFromDb.Id.ShouldBe(id);
            itemFromDb.Email.ShouldBe(email);
            itemFromDb.KycState.ShouldBe(kycState);
            itemFromDb.Deposited.ShouldBeFalse();
        }
        
        [Fact]
        public async Task CanUpdateDeposited()
        {
            var context = new DatabaseContext(Fixture.DbContextOptionsBuilder.Options);
            var customersRepository = new CustomersRepository(context);

            var id = Guid.NewGuid().ToString();
            var email = "email3@mail.com";
            
            await customersRepository.AddOrIgnoreAsync(Customer.Create(id, email));

            var itemFromDb = await customersRepository.GetOrDefault(id);
            
            itemFromDb.UpdateDeposited();
            
            context = new DatabaseContext(Fixture.DbContextOptionsBuilder.Options);
            customersRepository = new CustomersRepository(context);

            await customersRepository.Update(itemFromDb);
            
            itemFromDb = await customersRepository.GetOrDefault(id);
            
            itemFromDb.ShouldNotBeNull();
            itemFromDb.Id.ShouldBe(id);
            itemFromDb.Email.ShouldBe(email);
            itemFromDb.Deposited.ShouldBeTrue();
        }
    }
}
