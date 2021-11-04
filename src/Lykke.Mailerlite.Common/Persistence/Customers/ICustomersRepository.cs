using System.Threading.Tasks;
using Lykke.Mailerlite.Common.Domain.Customers;

namespace Lykke.Mailerlite.Common.Persistence.Customers
{
    public interface ICustomersRepository
    {
        Task<Customer> GetOrDefault(string id);
        Task AddOrIgnoreAsync(Customer customer);
        Task Update(Customer customer);
    }
}
