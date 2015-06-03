using System.Collections.Generic;

namespace CasinoInsight.Actors.Messages
{
    /// <summary>
    ///     Add the EventType Definiton to the CEP Engine Runtime
    /// </summary>
    public class AddEventType
    {
        public AddEventType(string eventType, Dictionary<string, object> map)
        {
            EventType = eventType;
            Map = map;
        }

        /// <summary>
        ///     Name of the EventType, commonly known as the stream name.
        /// </summary>
        public string EventType { get; private set; }

        /// <summary>
        ///     Property Name and Type Name Dictionary
        /// </summary>
        public Dictionary<string, object> Map { get; private set; }
    }
}