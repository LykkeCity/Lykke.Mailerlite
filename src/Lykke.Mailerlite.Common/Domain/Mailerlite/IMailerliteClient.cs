using System;
using System.Threading.Tasks;

namespace Lykke.Mailerlite.Common.Domain.Mailerlite
{
    public interface IMailerliteClient
    {
        Task CreateCustomerAsync(string email);
        Task SetCustomerKycAsync(string email, string kycState);
        Task SetCustomerRegisteredAsync(string email, DateTime registered);
        Task SetCustomerDepositedAsync(string email, bool value);
        Task SetCustomerSubmittedDocumentsAsync(string email, bool value);
        Task AddCustomerToGroupAsync(string email, string groupName);
        Task<int?> FindGroupIdByNameAsync(string groupName);
        Task DeleteCustomerFromGroupAsync(string email, int groupId);
    }
}
