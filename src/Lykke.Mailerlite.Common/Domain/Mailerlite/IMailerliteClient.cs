using System.Threading.Tasks;

namespace Lykke.Mailerlite.Common.Domain.Mailerlite
{
    public interface IMailerliteClient
    {
        Task CreateCustomerAsync(string email);
        Task SetCustomerKycAsync(string email, string kycState);
        Task SetCustomerDepositedAsync(string email, bool value);
        Task SetCustomerSubmittedDocumentsAsync(string email, bool value);
    }
}
