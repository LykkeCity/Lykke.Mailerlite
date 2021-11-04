using System;

namespace Lykke.Mailerlite.Common.Commands
{
    public class UpdateCustomerKycCommand
    {
        public string RequestId { set; get; }
        public string CustomerId { set; get; }
        public string KycState { set; get; }
        public DateTime Timestamp { set; get; }
    }
}
