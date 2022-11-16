using System.Collections.Generic;

namespace Lykke.Mailerlite.Common.Configuration
{
    public class MailerliteConfig
    {
        public string ApiKey { set; get; }
        public string CustomerCreateUrl { set; get; }
        public string CustomerUpdateFieldUrl { set; get; }
        public string AddCustomerToGroupUrl { set; get; }
        public string FindGroupIdByNameUrl { set; get; }
        public string DeleteCustomerFromGroupUrl { set; get; }
        public IEnumerable<string> NewCustomerFromAllAreasGroups { set; get; }
        public IEnumerable<string> NewCustomerFromUnrestrictedAreaGroups { set; get; }
        public string KycReminderGroup { set; get; }
        public IEnumerable<string> StatusesToDeleteFromKycReminderGroup { set; get; }
    }
}
