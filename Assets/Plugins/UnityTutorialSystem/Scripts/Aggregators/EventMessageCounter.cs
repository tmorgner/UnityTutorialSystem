using System;
using System.Collections.Generic;
using UnityEngine;
using UnityTutorialSystem.Events;

namespace UnityTutorialSystem.Aggregators
{
    /// <summary>
    ///     A simple example of an aggregator function. This behaviour listens for
    ///     incoming events that match the given message.
    /// </summary>
    public class EventMessageCounter : EventMessageAggregator
    {
        readonly HashSet<BasicEventStreamMessage> messagePool;
        [SerializeField] int targetCount;

        public EventMessageCounter()
        {
            messagePool = new HashSet<BasicEventStreamMessage>();
        }

        public int Count { get; set; }

        protected override void RegisterValidMessage(BasicEventStreamMessage m)
        {
            messagePool.Add(m);
        }

        public override void ResetMatch()
        {
            MatchResult = EventMessageMatcherState.Waiting;
            Count = 0;
        }

        public override List<EventMessageState> ListEvents(List<EventMessageState> buffer)
        {
            if (buffer == null)
            {
                buffer = new List<EventMessageState>(Messages.Count);
            }
            else
            {
                buffer.Clear();
                buffer.Capacity = Math.Max(buffer.Capacity, Messages.Count);
            }

            var completed = MatchResult == EventMessageMatcherState.Success;
            var expected = MatchResult == EventMessageMatcherState.Waiting;
            foreach (var m in Messages)
            {
                buffer.Add(new EventMessageState(m, completed, expected));
            }

            return buffer;
        }

        protected override bool OnEventReceived(BasicEventStreamMessage received)
        {
            if (messagePool.Contains(received))
            {
                Count += 1;
                if (targetCount == Count)
                {
                    MatchResult = EventMessageMatcherState.Success;
                    MatchComplete?.Invoke();
                }

                return true;
            }

            return false;
        }
    }
}