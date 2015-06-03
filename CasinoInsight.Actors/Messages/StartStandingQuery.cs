using CasinoInsight.Actors.Models;

namespace CasinoInsight.Actors.Messages
{
    public class StartStandingQuery
    {
        public StartStandingQuery(StandingQuery query)
        {
            Query = query;
        }

        public StandingQuery Query { get; private set; }
    }
}