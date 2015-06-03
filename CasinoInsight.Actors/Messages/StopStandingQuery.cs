namespace CasinoInsight.Actors.Messages
{
    public class StopStandingQuery
    {
        public StopStandingQuery(string standingQueryId)
        {
            StandingQueryId = standingQueryId;
        }

        public string StandingQueryId { get; private set; }
    }
}