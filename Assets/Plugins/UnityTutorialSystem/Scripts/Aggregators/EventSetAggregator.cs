using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityTutorialSystem.Events;

namespace UnityTutorialSystem.Aggregators
{
    /// <summary>
    ///     <para>
    ///         An aggregator that waits for all events to occur. The events can
    ///         occur in any order. If strict mode is enabled, a non-matching
    ///         event will fail this aggregator.
    ///     </para>
    ///     <para>
    ///         Duplicate declared messages will be collated into a single expected message.
    ///         To match multiple messages of the same type, use a <see cref="EventMessageCounter"/>
    ///         as a child matcher. 
    ///     </para>
    /// </summary>
    public class EventSetAggregator : EventMessageAggregator
    {
        /// <summary>
        ///   Defines how strict incoming messages are matched against the set of
        ///   received messages. 
        /// </summary>
        public enum MatchMode
        {
            /// <summary>
            ///   Requests an lenient matching mode. Duplicate messages will be ignored.
            /// </summary>
            Lenient = 0,
            /// <summary>
            ///   Requires strict matching. Any duplicate event received will fail the
            ///   aggregator.
            /// </summary>
            Strict = 2
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

        /// <inheritdoc />
        protected override void RegisterValidMessage(BasicEventStreamMessage m)
        {
            if (matchState.ContainsKey(m))
            {
                return;
            }

            messages.Add(m);
            matchState[m] = false;
        }

        /// <inheritdoc />
        public override void ResetMatch()
        {
            foreach (var k in matchState)
            {
                matchState[k.Key] = false;
            }

            MatchResult = EventMessageMatcherState.Waiting;
            matchCount = 0;
        }

        /// <inheritdoc />
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

        /// <inheritdoc />
        public override List<EventMessageState> ListEvents(List<EventMessageState> buffer)
        {
            buffer = EnsureBufferValid(buffer, messages.Count);

            foreach (var m in messages)
            {
                bool matchCompleted;
                matchState.TryGetValue(m, out matchCompleted);
                buffer.Add(new EventMessageState(m, matchCompleted, enabled && !matchCompleted));
            }

            return buffer;
        }
    }
}