using System;
using System.Collections.Generic;
using System.Text;
using Akka.Actor;
using CasinoInsight.Actors.Messages;
using CasinoInsight.Actors.Settings;
using Newtonsoft.Json;
using NLog;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace CasinoInsight.Actors
{
    /// <summary>
    ///     Responsible for replaying events received from a rabbitmq exchange to the NesperActor
    /// </summary>
    public class MessageRelayBrokerActor : UntypedActor
    {
        private readonly ConnectionFactory _factory = new ConnectionFactory();
        private readonly Logger _log = LogManager.GetCurrentClassLogger();
        private readonly RabbitMqSettings _settings;
        private ActorSelection _cepActor;
        private IModel _channel;
        private IConnection _connection;
        private EventingBasicConsumer _eventingConsumer;

        public MessageRelayBrokerActor(RabbitMqSettings settings)
        {
            _settings = settings;
        }

        protected override void OnReceive(object message)
        {
        }

        #region Protected Override Methods

        protected override void PreStart()
        {
            _cepActor = Context.ActorSelection("../cepactor");
            _factory.HostName = _settings.Host.Server;
            _factory.VirtualHost = _settings.Host.VirtualHost;
            _factory.UserName = _settings.Host.Username;
            _factory.Password = _settings.Host.Password;
            _connection = _factory.CreateConnection();
            _channel = _connection.CreateModel();
            _channel.ExchangeDeclare(_settings.Exchange.ExchangeName, _settings.Exchange.ExchangeType, true);
            _channel.QueueDeclare(_settings.Queue.QueueName, true, false, false, null);
            _channel.QueueBind(_settings.Queue.QueueName, _settings.Exchange.ExchangeName, _settings.Queue.RoutingKey);
            _eventingConsumer = new EventingBasicConsumer(_channel);
            _eventingConsumer.Received += _eventingConsumer_Received;
        }

        private void _eventingConsumer_Received(object sender, BasicDeliverEventArgs e)
        {
            var payload = JsonConvert.DeserializeObject<Dictionary<string, object>>(Encoding.UTF8.GetString(e.Body));

            _cepActor.Tell(new NewEvent(e.RoutingKey, DateTime.Now, new Dictionary<string, object>(payload),
                new Dictionary<string, object>(e.BasicProperties.Headers), e.BasicProperties.MessageId, e.ConsumerTag,
                e.BasicProperties.CorrelationId));
        }

        #endregion
    }
}