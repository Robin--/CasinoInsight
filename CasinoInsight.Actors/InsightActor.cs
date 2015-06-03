using Akka.Actor;
using CasinoInsight.Actors.Settings;

namespace CasinoInsight.Actors
{
    public class InsightActor : ReceiveActor
    {
        private IActorRef _nesperActorRef;
        private IActorRef _relayActorRef;

        #region

        protected override void PreStart()
        {
            var nesperprop = Props.Create(() => new CepActor());
            var relayprop = Props.Create(() => new MessageRelayBrokerActor(new RabbitMqSettings
            {
                BindQueue = true,
                Exchange = new RabbitMqSettings.ExchangeSettings
                {
                    ExchangeName = "insight",
                    ExchangeType = "topic"
                },
                Host = new RabbitMqSettings.HostSettings
                {
                    Password = "password123",
                    Port = 5672,
                    Server = "localhost",
                    Username = "insightuser",
                    VirtualHost = "casinoinsight"
                },
                Queue = new RabbitMqSettings.QueueSettings
                {
                    QueueName = "casinoinsight.inbound",
                    RoutingKey = "#"
                }
            }));

            _nesperActorRef = Context.ActorOf(nesperprop, "cepactor");
            _relayActorRef = Context.ActorOf(relayprop, "relayactor");
        }

        #endregion
    }
}