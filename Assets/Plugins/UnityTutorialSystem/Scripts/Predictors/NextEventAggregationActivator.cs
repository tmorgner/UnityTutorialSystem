using UnityEngine;
using UnityTutorialSystem.Aggregators;
using UnityTutorialSystem.Events;

namespace UnityTutorialSystem.Predictors
{
    /// <summary>
    ///   An low-maintenance event aggregation activator for activating dependent
    ///   EventMessageAggregator instances. Add this to a child-level aggregator
    ///   that is paired with a <see cref="StreamEventSource"/>, usually a
    ///   <see cref="EventMessageAggregatorStatePublisher"/>. This will selectively enable the
    ///    aggregator while the parent stream expects the aggregator's result event message
    ///    as one of its next messages. 
    /// </summary>
    [RequireComponent(typeof(StreamEventSource))]
    [RequireComponent(typeof(EventMessageAggregator))]
    public class NextEventAggregationActivator : NextEventSelectorBase
    {
        EventMessageAggregator target;
        StreamEventSource nextMessageSource;

        protected override bool AutoPopulate => true;

        protected override StreamEventSource NextMessageSource => nextMessageSource;

        void Awake()
        {
            nextMessageSource = GetComponent<StreamEventSource>();
            target = GetComponent<EventMessageAggregator>();
            target.enabled = false;
        }

        protected override void OnEnableForNextMessage()
        {
            if (target != null)
            {
                target.enabled = true;
            }
        }

        protected override void OnDisableForNextMessage()
        {
            if (target != null)
            {
                target.enabled = false;
            }
        }
    }
}