using System;

namespace Lykke.Mailerlite.ApiClient
{
    public class LykkeMailerliteServiceOptions
    {
        public Uri GrpcServiceUrl { get; set; }
        public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(60);
    }
}
