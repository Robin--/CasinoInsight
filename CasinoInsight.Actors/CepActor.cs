using System;
using System.Collections.Generic;
using System.Linq;
using Akka.Actor;
using com.espertech.esper.client;
using com.espertech.esper.compat.collections;
using com.espertech.esper.core.service;
using CasinoInsight.Actors.Messages;
using Castle.Core.Internal;
using Newtonsoft.Json;
using NLog;

namespace CasinoInsight.Actors
{
    /// <summary>
    ///     Nesper Runtime Actor Hosting all the EPL and Streams
    /// </summary>
    public class CepActor : ReceiveActor
    {
        public CepActor()
        {
            Receive<NewEvent>(r => Handle_NewEvent(r));
            Receive<StartStandingQuery>(r => Handle_StartStandingQuery(r));
            Receive<StopStandingQuery>(r => Handle_StopStandingQuery(r));
            Receive<AddEventType>(r => Handle_AddEventType(r));
        }

        #region Private Methods

        private void RegisterEventType(string eventType, Dictionary<string, object> map)
        {
            if (_service.EPAdministrator.Configuration.EventTypes.All(et => et.Name != eventType))
            {
                _service.EPAdministrator.Configuration.AddEventType(eventType, map);
            }
        }

        #endregion

        #region Private Members

        private readonly Logger _log = LogManager.GetCurrentClassLogger();
        private EPRuntime _runtime;
        private EPServiceProvider _service;
        private HashMap<string, EPStatement> _statements;
        private IActorRef _querySubscriberCoordinatorActorRef;

        #endregion

        #region Message Handlers

        private void Handle_AddEventType(AddEventType message)
        {
            RegisterEventType(message.EventType, message.Map);
            _log.Info("EventType {0} registered", message.EventType);
        }

        private void Handle_NewEvent(NewEvent message)
        {
            _runtime.SendEvent(message.Payload, message.EventType);
        }

        public void Handle_StartStandingQuery(StartStandingQuery message)
        {
            if (_statements.ContainsKey(message.Query.StandingQueryId) &&
                _statements[message.Query.StandingQueryId].IsStarted) return;
            var statement = _service.EPAdministrator.CreateEPL(message.Query.EplStatement, message.Query.StandingQueryId,
                message.Query);
            statement.AddEventHandlerWithReplay(SimpleEventHandler);
            _statements.Add(message.Query.StandingQueryId, statement);
            statement.Start();
            _log.Info("StandingQuery {0} - {1} Started", message.Query.StandingQueryId, message.Query.Description);
        }

        public void Handle_StopStandingQuery(StopStandingQuery message)
        {
            if (_statements.ContainsKey(message.StandingQueryId) &&
                _statements[message.StandingQueryId].IsStarted)
            {
                _statements[message.StandingQueryId].Stop();
                _log.Info("StandingQuery {0} Stopped", message.StandingQueryId);
            }
        }

        #endregion

        #region Override Methods

        protected override void PreStart()
        {
            var config = new Configuration();
            config.EngineDefaults.LoggingConfig.IsEnableExecutionDebug = true;
            config.EngineDefaults.LoggingConfig.IsEnableTimerDebug = true;
            config.EngineDefaults.LoggingConfig.IsEnableQueryPlan = true;
            _service = EPServiceProviderManager.GetDefaultProvider(config);
            _runtime = _service.EPRuntime;
            _statements = new HashMap<string, EPStatement>();
            var prop = Props.Create(() => new QuerySubscriberCoordinatorActor());
            _querySubscriberCoordinatorActorRef = Context.ActorOf(prop, "querysubscribercoordinator");
        }

        protected override void PostStop()
        {
            base.PostStop();
            _statements.ForEach(s => s.Value.Stop());
        }

        #endregion

        #region Private Event Handlers

        private void replayquerysubscriber_OnReplayPayload(string queryid, Dictionary<string, object> payload)
        {
            _querySubscriberCoordinatorActorRef.Tell(new StandingQueryResult(queryid, payload, DateTime.Now));
        }

        private void SimpleEventHandler(object o, UpdateEventArgs args)
        {
            if (args.NewEvents != null)
            {
                // ReSharper disable once CanBeReplacedWithTryCastAndCheckForNull
                if (o is StatementResultServiceImpl)
                {
                    var statement = (StatementResultServiceImpl) o;
                    args.NewEvents.ForEach(e =>
                    {
                        var json = JsonConvert.SerializeObject(e.Underlying);
                        _log.Info("new -> {0} -> {1} -> eventhandler", e.EventType.Name, json);
                        _querySubscriberCoordinatorActorRef.Tell(new StandingQueryResult(statement.StatementName,
                            JsonConvert.DeserializeObject<Dictionary<string, object>>(json), DateTime.Now));
                    });
                }
            }
            if (args.OldEvents != null)
            {
                // ReSharper disable once CanBeReplacedWithTryCastAndCheckForNull
                if (o is StatementResultServiceImpl)
                {
                    var statement = (StatementResultServiceImpl) o;
                    args.NewEvents.ForEach(e =>
                    {
                        var json = JsonConvert.SerializeObject(e.Underlying);
                        _log.Info("old -> {0} -> {1} -> eventhandler", e.EventType.Name, json);
                        _querySubscriberCoordinatorActorRef.Tell(new StandingQueryResult(statement.StatementName,
                            JsonConvert.DeserializeObject<Dictionary<string, object>>(json), DateTime.Now));
                    });
                    return;
                }
            }
            _log.Info("No Events");
        }

        #endregion
    }
}