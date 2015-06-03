using Akka.Actor;
using CasinoInsight.Actors.Messages;
using Newtonsoft.Json;
using NLog;

namespace CasinoInsight.Actors
{
    public class QueryConsoleSubscriberActor : ReceiveActor
    {
        #region Private Members

        private readonly Logger _logger = LogManager.GetCurrentClassLogger();

        #endregion

        public QueryConsoleSubscriberActor()
        {
            Receive<StandingQueryResult>(r => Handler_StandingQueryResult(r));
        }

        #region Message Handlers

        private void Handler_StandingQueryResult(StandingQueryResult message)
        {
            var json = JsonConvert.SerializeObject(message.Payload);
            _logger.Trace("result -> {0} - {1} - {2}", message.TimeStamp, message.StandingQueryId, json);
        }

        #endregion
    }
}