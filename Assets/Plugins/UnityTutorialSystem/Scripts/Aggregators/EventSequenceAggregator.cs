using System;
using System.Collections.Generic;
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
    ///     patiently wait until it sees a next suitable event. Non-strict matchers will
    ///     never fire a match-failed event.
    /// </summary>
    public class EventSequenceAggregator : EventMessageAggregator
    {
        public enum ValidationMode
        {
            Lenient,
            AllowOutOfOrderEvents,
            Strict
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

        protected override void RegisterValidMessage(BasicEventStreamMessage m)
        {
            validMessages.Add(m);
            matchState.Add(false);
        }

        public override void ResetMatch()
        {
            nextEvent = 0;
            MatchResult = EventMessageMatcherState.Waiting;

            for (var i = 0; i < matchState.Count; i++)
            {
                matchState[i] = false;
            }
        }

        public override List<EventMessageState> ListEvents(List<EventMessageState> buffer)
        {
            if (buffer == null)
            {
                buffer = new List<EventMessageState>(validMessages.Count);
            }
            else
            {
                buffer.Clear();
                buffer.Capacity = Math.Max(buffer.Capacity, Messages.Count);
            }

            for (var index = 0; index < validMessages.Count; index++)
            {
                var m = validMessages[index];
                buffer.Add(new EventMessageState(m, matchState[index], enabled && (index == nextEvent)));
            }

            return buffer;
        }

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

            if (mode == ValidationMode.AllowOutOfOrderEvents)
            {
                for (var idx = nextEvent + 1; idx < validMessages.Count; idx += 1)
                {
                    var msg = validMessages[idx];
                    if (!msg.AllowOutOfOrderExecution)
                    {
                        break;
                    }

                    if (msg == messageReceived)
                    {
                        if (!matchState[idx])
                        {
                            matchState[idx] = true;
                            return true;
                        }
                    }
                }
            }

            if (mode == ValidationMode.Strict)
            {
                MatchResult = EventMessageMatcherState.Failure;
                matchFailed?.Invoke();
                return true;
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