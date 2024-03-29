﻿namespace Lykke.Mailerlite.Common.Configuration
{
    public class AppConfig
    {
        public DbConfig Db { get; set; }

        public RabbitMqConfig RabbitMq { get; set; }
        
        public MailerliteConfig Mailerlite { set; get; }
    }
}
