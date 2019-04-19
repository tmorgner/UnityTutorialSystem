using UnityEngine;
using UnityEngine.Events;
using UnityTutorialSystem.Aggregators;
using UnityTutorialSystem.Events;

namespace UnityTutorialSystem.Predictors
{
    /// <summary>
    /// <para>
    ///   A general purpose next-event selector. Use this to fire events when the event produced
    ///   by the given <see cref="nextMessageSource"/> StreamEventSource is one of the next
    ///   expected messages of the collaborating <see cref="EventMessageAggregator"/>s.
    /// </para>
    /// <para>
    ///   One usual use for this class is to activate and deactivate visual indicators that guide
    ///   the player to the location of the next task that should be done. 
    /// </para>
    /// </summary>
    public class NextEventSelector : NextEventSelectorBase
    {
        [SerializeField] StreamEventSource nextMessageSource;
        [SerializeField] bool autoPopulate;
        [SerializeField] UnityEvent enableForNextMessage;
        [SerializeField] UnityEvent disableForNextMessage;


        protected override StreamEventSource NextMessageSource => nextMessageSource;

        protected override bool AutoPopulate => autoPopulate;

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