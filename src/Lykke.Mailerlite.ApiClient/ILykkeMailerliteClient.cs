using Lykke.Mailerlite.ApiContract;

namespace Lykke.Mailerlite.ApiClient
{
    public interface ILykkeMailerliteClient
    {
        Monitoring.MonitoringClient Monitoring { get; }
        Customers.CustomersClient Customers { get; }
    }
}
