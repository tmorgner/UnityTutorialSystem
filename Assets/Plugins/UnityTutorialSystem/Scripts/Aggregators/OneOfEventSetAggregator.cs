using System;
using System.Collections.Generic;
using UnityTutorialSystem.Events;

namespace UnityTutorialSystem.Aggregators
{
    /// <summary>
    ///     An aggregator component that waits for any of the given messages and fires a
    ///     success message if at least one of the listed messages has been encountered.
    /// </summary>
    public class OneOfEventSetAggregator : EventMessageAggregator
    {
        readonly List<BasicEventStreamMessage> messages;
        readonly HashSet<BasicEventStreamMessage> matchState;
        int matchCount;

        public OneOfEventSetAggregator()
        {
            matchState = new HashSet<BasicEventStreamMessage>();
            messages = new List<BasicEventStreamMessage>();
        }

        /// <inheritdoc />
        protected override void RegisterValidMessage(BasicEventStreamMessage m)
        {
            if (matchState.Add(m))
            {
                messages.Add(m);
            }
        }

        /// <inheritdoc />
        public override void ResetMatch()
        {
            MatchResult = EventMessageMatcherState.Waiting;
            matchCount = 0;
        }

        /// <inheritdoc />
        protected override bool OnEventReceived(BasicEventStreamMessage messageReceived)
        {
            if (matchCount > 0)
            {
                return false;
            }

            if (matchState.Contains(messageReceived))
            {
                MatchResult = EventMessageMatcherState.Success;
                MatchComplete?.Invoke();

                matchCount += 1;
                return true;
            }

            return false;
        }

        /// <inheritdoc />
        public override List<EventMessageState> ListEvents(List<EventMessageState> buffer)
        {
            if (buffer == null)
            {
                buffer = new List<EventMessageState>(messages.Count);
            }
            else
            {
                buffer.Clear();
                buffer.Capacity = Math.Max(buffer.Capacity, messages.Count);
            }

            var next = MatchResult == EventMessageMatcherState.Waiting;
            var completed = MatchResult == EventMessageMatcherState.Success;
            foreach (var m in messages)
            {
                buffer.Add(new EventMessageState(m, completed, enabled && next));
            }

            return buffer;
        }
    }
}