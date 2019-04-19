using System.Collections.Generic;
using UnityEngine.Events;
using UnityTutorialSystem.Events;

namespace UnityTutorialSystem.Aggregators
{
    /// <summary>
    ///     Aggregates low-level event stream messages into higher level aggregate events.
    ///     An typical EventMessageAggregator monitors one or more event streams to detect
    ///     predefined sequences of events in the stream. When an event sequence is fully
    ///     matched, the built-in events of this class can be used to fire a new event stream
    ///     message via an associated <see cref="EventMessageAggregatorStatePublisher" />.
    /// </summary>
    /// <remarks>
    ///     This interface primarily exists for unit testing purposes.
    /// </remarks>
    public interface IEventMessageAggregator
    {
        /// <summary>
        ///  This event is fired when the aggregator is ready to receive messages after this MonoBehaviour became active.
        /// </summary>
        UnityEvent MatchStarting { get; }
        
        /// <summary>
        ///    This event is fired when the aggregator has successfully finished matching events. Be aware that this event
        ///    is NOT fired when the matching fails with an error. To receive error information subscribe to the MatchProgress
        ///    event and check the sender's MatchResult property for failures. 
        /// </summary>
        UnityEvent MatchComplete { get; }
        
        /// <summary>
        ///    This event is fired whenever any progress has been made. Any internal state change will always result in a
        ///    MatchProgress event.
        /// </summary>
        MatchProgressEvent MatchProgress { get; }

        /// <summary>
        ///   Resets the aggregator to the initial state as if no message has been received yet.
        /// </summary>
        void ResetMatch();
               
        /// <summary>
        ///   Queries the internal state of the message aggregator. The given buffer will be filled with <see cref="EventMessageState"/>
        ///   structs containing the expected event in the order they are expected to be seen as well as flags indicating whether
        ///   the event has been seen or is expected to be seen next. 
        /// </summary>
        /// <param name="buffer">A receive buffer. If null, a new list will be created. If the list is non-empty, the list will be cleared.</param>
        /// <returns>The buffer provided or a new list if the buffer given is null.</returns>
        List<EventMessageState> ListEvents(List<EventMessageState> buffer = null);
    }
}