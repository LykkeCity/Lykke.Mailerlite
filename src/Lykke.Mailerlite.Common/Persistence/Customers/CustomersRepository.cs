using System;
using System.Threading.Tasks;
using Lykke.Mailerlite.Common.Domain.Customers;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Lykke.Mailerlite.Common.Persistence.Customers
{
    public class CustomersRepository : ICustomersRepository
    {
        private readonly DatabaseContext _dbContext;

        public CustomersRepository(DatabaseContext dbContext)
        {
            _dbContext = dbContext;
        }
        
        public async Task AddOrIgnoreAsync(Customer customer)
        {
            var newEntity = MapToEntity(customer);

            _dbContext.Customers.Add(newEntity);

            try
            {
                await _dbContext.SaveChangesAsync();
            }
            catch (DbUpdateException exception)
                when (exception.InnerException is PostgresException pgException &&
                      pgException.SqlState == PostgresErrorCodes.UniqueViolation)
            {
            }
        }

        public async Task Update(Customer customer)
        {
            var entity = MapToEntity(customer);
            
            _dbContext.Customers.Update(entity);
            
            await _dbContext.SaveChangesAsync();
        }
        
        public async Task<Customer> GetOrDefault(string id)
        {
            var entity = (await _dbContext
                .Customers
                .FirstOrDefaultAsync(x => x.Id == id));

            return MapToDomain(entity);
        }
        
        private static Customer MapToDomain(CustomerEntity entity)
        {
            if (entity == null)
                return null;

            var customer = Customer.Restore(entity.Id, entity.Email, entity.KycState, entity.KycStateTimestamp, entity.Deposited, entity.HasEverSubmittedDocuments);

            return customer;
        }

        private static CustomerEntity MapToEntity(Customer domainModel)
        {
            var entity = new CustomerEntity
            {
                Id = domainModel.Id,
                Email = domainModel.Email,
                KycState = domainModel.KycState,
                KycStateTimestamp = domainModel.KycStateTimestamp,
                Deposited = domainModel.Deposited,
                HasEverSubmittedDocuments = domainModel.HasEverSubmittedDocuments
            };

            return entity;
        }
    }
}
