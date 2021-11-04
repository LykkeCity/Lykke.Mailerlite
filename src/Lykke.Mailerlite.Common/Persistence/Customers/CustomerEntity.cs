using System;

namespace Lykke.Mailerlite.Common.Persistence.Customers
{
    public class CustomerEntity
    {
        public string Id { get; set; }
        public string Email { set; get; }
        public string KycState { get; set; }
        public DateTime? KycStateTimestamp { get; set; }
        public bool Deposited { set; get; }
    }
}
