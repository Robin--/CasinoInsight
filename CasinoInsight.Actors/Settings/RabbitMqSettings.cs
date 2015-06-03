namespace CasinoInsight.Actors.Settings
{
    public class RabbitMqSettings
    {
        public bool BindQueue { get; set; }
        public HostSettings Host { get; set; }
        public ExchangeSettings Exchange { get; set; }
        public QueueSettings Queue { get; set; }

        public class HostSettings
        {
            public string Server { get; set; }
            public string VirtualHost { get; set; }
            public string Username { get; set; }
            public string Password { get; set; }
            public int Port { get; set; }
        }

        public class ExchangeSettings
        {
            public string ExchangeName { get; set; }
            public string ExchangeType { get; set; }
        }

        public class QueueSettings
        {
            public string QueueName { get; set; }
            public string RoutingKey { get; set; }
        }
    }
}