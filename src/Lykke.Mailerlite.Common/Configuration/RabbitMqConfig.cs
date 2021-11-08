namespace Lykke.Mailerlite.Common.Configuration
{
    public class RabbitMqConfig
    {
        public string HostUrl { get; set; }

        public string VirtualHost { set; get; } = "/";

        public string Username { get; set; }

        public string Password { get; set; }
        
        public ushort Port { set; get; }
    }
}
