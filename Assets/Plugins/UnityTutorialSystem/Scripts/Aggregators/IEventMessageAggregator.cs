using System;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityTutorialSystem.Events;

namespace UnityTutorialSystem.Aggregators
{
    public enum EventMessageMatcherState
    {
        Waiting,
        Success,
        Failure
    }

    public struct EventMessageState
    {
        public readonly BasicEventStreamMessage Message;
        public readonly bool Completed;
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

    public interface IEventMessageAggregator
    {
        UnityEvent MatchComplete { get; }
        MatchProgressEvent MatchProgress { get; }

        EventMessageMatcherState MatchResult { get; }

        List<EventMessageState> ListEvents(List<EventMessageState> buffer);
        void ResetMatch();
    }
}