using System;
using System.Collections.Generic;

namespace CasinoInsight.Actors.Messages
{
    public class StandingQueryResult
    {
        public StandingQueryResult(string standingQueryId, Dictionary<string, object> payload, DateTime timestamp)
        {
            StandingQueryId = standingQueryId;
            Payload = payload;
            TimeStamp = timestamp;
        }

        public string StandingQueryId { get; private set; }
        public Dictionary<string, object> Payload { get; private set; }
        public DateTime TimeStamp { get; private set; }
    }
}