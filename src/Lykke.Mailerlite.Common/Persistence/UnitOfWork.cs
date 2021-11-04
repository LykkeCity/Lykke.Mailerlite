using Lykke.Mailerlite.Common.Persistence.Customers;
using Microsoft.EntityFrameworkCore;
using Swisschain.Extensions.Idempotency.EfCore;

namespace Lykke.Mailerlite.Common.Persistence
{
    public class UnitOfWork : UnitOfWorkBase<DatabaseContext>
    {
        public ICustomersRepository Customers { get; private set; }
        
        protected override void ProvisionRepositories(DatabaseContext dbContext)
        {
            Customers = new CustomersRepository(dbContext);
        }
    }
}
