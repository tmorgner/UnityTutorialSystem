using UnityEngine;
using UnityEngine.Events;

namespace UnityTutorialSystem.Events
{
    public class NextEventSelector : NextEventSelectorBase
    {
        [SerializeField] StreamEventSource nextMessageSource;
        [SerializeField] bool autoPopulate;
        [SerializeField] UnityEvent enableForNextMessage;
        [SerializeField] UnityEvent disableForNextMessage;


        public override StreamEventSource NextMessageSource => nextMessageSource;

        public override bool AutoPopulate => autoPopulate;

        public UnityEvent EnableForNextMessage => enableForNextMessage;

        public UnityEvent DisableForNextMessage => disableForNextMessage;

        protected override void OnEnableForNextMessage()
        {
            enableForNextMessage?.Invoke();
        }

        protected override void OnDisableForNextMessage()
        {
            disableForNextMessage?.Invoke();
        }
    }
}