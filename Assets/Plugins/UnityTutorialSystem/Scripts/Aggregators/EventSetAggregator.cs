using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityTutorialSystem.Events;

namespace UnityTutorialSystem.Aggregators
{
    /// <summary>
    ///     A aggregator that waits for all events to occur. The events can
    ///     occur in any order. If strict mode is enabled, a non-matching
    ///     event will fail this aggregator.
    /// </summary>
    public class EventSetAggregator : EventMessageAggregator
    {
        public enum MatchMode
        {
            Lenient,
            IgnoreDuplicates,
            Strict
        }

        readonly Dictionary<BasicEventStreamMessage, bool> matchState;
        readonly List<BasicEventStreamMessage> messages;

        [SerializeField] UnityEvent matchFailed;
        [SerializeField] MatchMode matchMode;
        int matchCount;

        public EventSetAggregator()
        {
            matchState = new Dictionary<BasicEventStreamMessage, bool>();
            messages = new List<BasicEventStreamMessage>();
        }

        protected override void RegisterValidMessage(BasicEventStreamMessage m)
        {
            if (matchState.ContainsKey(m))
            {
                return;
            }

            messages.Add(m);
            matchState[m] = false;
        }

        public override void ResetMatch()
        {
            foreach (var k in matchState)
            {
                matchState[k.Key] = false;
            }

            MatchResult = EventMessageMatcherState.Waiting;
            matchCount = 0;
        }

        protected override bool OnEventReceived(BasicEventStreamMessage messageReceived)
        {
            if (matchCount == matchState.Count)
            {
                return false;
            }

            bool matching;
            if (!matchState.TryGetValue(messageReceived, out matching))
            {
                // no such element in the list of possible matches.
                return false;
            }

            if (matching)
            {
                if (matchMode == MatchMode.Strict)
                {
                    MatchResult = EventMessageMatcherState.Failure;
                    matchFailed?.Invoke();
                    return true;
                }

                // repeated invocations of the same event should not trigger the alarm.
                return false;
            }

            matchState[messageReceived] = true;
            matchCount += 1;
            if (matchCount == matchState.Count)
            {
                MatchResult = EventMessageMatcherState.Success;
                MatchComplete?.Invoke();
            }

            return true;
        }

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

            for (var index = 0; index < messages.Count; index++)
            {
                var m = messages[index];
                bool matchCompleted;
                matchState.TryGetValue(m, out matchCompleted);
                buffer.Add(new EventMessageState(m, matchCompleted, enabled && !matchCompleted));
            }

            return buffer;
        }
    }
}