using Akka.Actor;
using CasinoInsight.Actors.Messages;

namespace CasinoInsight.Actors
{
    public class QuerySubscriberCoordinatorActor : ReceiveActor
    {
        public QuerySubscriberCoordinatorActor()
        {
            Receive<StandingQueryResult>(r => Handle_StandingQueryResult(r));
        }

        #region Message Handlers

        private void Handle_StandingQueryResult(StandingQueryResult message)
        {
            _queryConsoleSubscriberActorRef.Tell(new StandingQueryResult(message.StandingQueryId, message.Payload,
                message.TimeStamp));
        }

        #endregion

        #region Protected Overrides

        protected override void PreStart()
        {
            var prop = Props.Create(() => new QueryConsoleSubscriberActor());
            _queryConsoleSubscriberActorRef = Context.ActorOf(prop, "queryconsolesubscriber");
        }

        #endregion

        #region Private Members

        private IActorRef _queryConsoleSubscriberActorRef;
        private ActorSelection _subscribers;

        #endregion
    }
}