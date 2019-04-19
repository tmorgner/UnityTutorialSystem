using System.Collections.Generic;
using JetBrains.Annotations;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Events;
using UnityTutorialSystem.Events;

namespace UnityTutorialSystem.Aggregators
{
    /// <summary>
    ///     A event sequence matcher. The matcher checks incoming events against a
    ///     predefined sequence of events.  If the matching is strict, any incoming event
    ///     that does not match the expected event will mark the sequence as failed.
    ///     A non-strict matcher will simply ignore events that do not match and will
    ///     patiently wait until it sees a suitable next event. Therefore non-strict matchers will
    ///     never fire a match-failed event.
    /// </summary>
    /// <remarks>
    ///     Acceptable events are stored in the 'validMessages' list. 
    ///     This class tracks each received message in a list of flags, one entry for each
    ///     accepted message. The success message is fired when all events have been seen at
    ///     least once. 
    /// </remarks>
    public class EventSequenceAggregator : EventMessageAggregator
    {
        /// <summary>
        ///   Defines the validation mode for this aggregator. 
        /// </summary>
        public enum ValidationMode
        {
            /// <summary>
            ///   Requests lenient matching. A lenient matcher will ignore out of order events.
            /// </summary>
            [UsedImplicitly] Lenient = 0,
            /// <summary>
            ///   Requests that the matcher accepts events in a relaxed order. Events that form a
            ///   series of events that all are marked as out-of-order executable will be treated
            ///   as an event set instead of an event list. Out of order processing will not skip
            ///   events that are not marked for out of order execution. 
            /// </summary>
            AllowOutOfOrderEvents = 1,
            /// <summary>
            ///   Requires strict matching. Any event not seen in the correct order will mark the
            ///   aggregator as failed.
            /// </summary>
            Strict = 2
        }

        readonly List<BasicEventStreamMessage> validMessages;
        readonly List<bool> matchState;

        [SerializeField] UnityEvent matchFailed;
        [SerializeField] ValidationMode mode;
        [ShowNonSerializedField] int nextEvent;

        public EventSequenceAggregator()
        {
            validMessages = new List<BasicEventStreamMessage>();
            matchState = new List<bool>();
        }

        /// <inheritdoc />
        protected override void RegisterValidMessage(BasicEventStreamMessage m)
        {
            validMessages.Add(m);
            matchState.Add(false);
        }

        /// <inheritdoc />
        public override void ResetMatch()
        {
            nextEvent = 0;
            MatchResult = EventMessageMatcherState.Waiting;

            for (var i = 0; i < matchState.Count; i++)
            {
                matchState[i] = false;
            }
        }

        /// <inheritdoc />
        public override List<EventMessageState> ListEvents(List<EventMessageState> buffer = null)
        {
            buffer = EnsureBufferValid(buffer, Messages.Count);

            for (var index = 0; index < validMessages.Count; index++)
            {
                var m = validMessages[index];
                buffer.Add(new EventMessageState(m, matchState[index], enabled && (index == nextEvent)));
            }

            return buffer;
        }

        /// <inheritdoc />
        protected override bool OnEventReceived(BasicEventStreamMessage messageReceived)
        {
            if (nextEvent >= validMessages.Count)
            {
                return false;
            }

            var nextEventMessage = validMessages[nextEvent];
            if (messageReceived == nextEventMessage)
            {
                matchState[nextEvent] = true;
                nextEvent = FindNextOpenEvent();
                if (nextEvent == validMessages.Count)
                {
                    MatchResult = EventMessageMatcherState.Success;
                    MatchComplete?.Invoke();
                }

                return true;
            }

            switch (mode)
            {
                case ValidationMode.AllowOutOfOrderEvents:
                {
                    for (var idx = nextEvent + 1; idx < validMessages.Count; idx += 1)
                    {
                        var msg = validMessages[idx];
                        if (!msg.AllowOutOfOrderExecution)
                        {
                            break;
                        }

                        if (msg == messageReceived && !matchState[idx])
                        {
                            matchState[idx] = true;
                            return true;
                        }
                    }

                    break;
                }
                case ValidationMode.Strict:
                {
                    MatchResult = EventMessageMatcherState.Failure;
                    matchFailed?.Invoke();
                    return true;
                }
            }

            return false;
        }

        int FindNextOpenEvent()
        {
            for (var idx = nextEvent + 1; idx < validMessages.Count; idx += 1)
            {
                if (matchState[idx] == false)
                {
                    return idx;
                }
            }

            return validMessages.Count;
        }
    }
}