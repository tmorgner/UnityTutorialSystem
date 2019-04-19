using System;
using UnityTutorialSystem.Events;

namespace UnityTutorialSystem.Aggregators
{
    /// <summary>
    ///   A struct representing the matching state of an EventMessageAggregator.
    /// </summary>
    public struct EventMessageState
    {
        /// <summary>
        ///  The event stream message represented by this state. 
        /// </summary>
        public readonly BasicEventStreamMessage Message;
        /// <summary>
        ///   Has this message been seen by the aggregator?
        /// </summary>
        public readonly bool Completed;
        /// <summary>
        ///   Is this one of the messages the aggregator expects to receive next?
        /// </summary>
        public readonly bool ExpectedNext;

        public EventMessageState(BasicEventStreamMessage message, bool completed, bool expectedNext)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            Message = message;
            Completed = completed;
            ExpectedNext = expectedNext;
        }
    }
}