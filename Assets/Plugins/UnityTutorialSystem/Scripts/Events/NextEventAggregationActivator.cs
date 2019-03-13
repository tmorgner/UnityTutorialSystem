using UnityTutorialSystem.Aggregators;

namespace UnityTutorialSystem.Events
{
    public class NextEventAggregationActivator : NextEventSelectorBase
    {
        EventMessageAggregator target;
        StreamEventSource nextMessageSource;

        public override bool AutoPopulate => true;

        public override StreamEventSource NextMessageSource => nextMessageSource;

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