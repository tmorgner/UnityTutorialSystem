using System;
using System.Collections.Generic;
using UnityEngine;
using UnityTutorialSystem.Events;
using UnityTutorialSystem.Helpers;

namespace UnityTutorialSystem.Aggregators
{
    /// <summary>
    ///     A simple example of a message aggregator. This class counts the incoming events that match the given message.
    /// </summary>
    public class EventMessageCounter : EventMessageAggregator
    {
        readonly HashSet<UnityObjectWrapper<BasicEventStreamMessage>> messagePool;
        [SerializeField] int targetCount;

        public EventMessageCounter()
        {
            messagePool = new HashSet<UnityObjectWrapper<BasicEventStreamMessage>>();
        }

        public int Count { get; private set; }

        /// <inheritdoc />
        protected override void RegisterValidMessage(BasicEventStreamMessage m)
        {
            messagePool.Add(new UnityObjectWrapper<BasicEventStreamMessage>(m));
        }

        /// <inheritdoc />
        public override void ResetMatch()
        {
            MatchResult = EventMessageMatcherState.Waiting;
            Count = 0;
        }

        /// <inheritdoc />
        public override List<EventMessageState> ListEvents(List<EventMessageState> buffer = null)
        {
            buffer = EnsureBufferValid(buffer, Messages.Count);

            var completed = MatchResult == EventMessageMatcherState.Success;
            var expected = MatchResult == EventMessageMatcherState.Waiting;
            foreach (var m in Messages)
            {
                buffer.Add(new EventMessageState(m, completed, expected));
            }

            return buffer;
        }

        /// <inheritdoc />
        protected override bool OnEventReceived(BasicEventStreamMessage received)
        {
            if (!messagePool.Contains(new UnityObjectWrapper<BasicEventStreamMessage>(received)))
            {
                return false;
            }

            Count += 1;
            if (targetCount == Count)
            {
                MatchResult = EventMessageMatcherState.Success;
                MatchComplete?.Invoke();
            }

            return true;

        }
    }
}