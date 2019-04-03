using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityTutorialSystem.Aggregators;
using UnityTutorialSystem.UI;

namespace UnityTutorialSystem.Events
{
    /// <summary>
    ///    <para>A companion <see cref="MonoBehaviour"/> for <see cref="EventMessageAggregator"/> instances that monitors the
    ///    EventMessageAggregator and republishes the events as UnityEvents. The <see cref="EventMessageAggregatorStatePublisher"/>
    ///    is also used by the <see cref="EventStreamTreeModelBuilder"/> to detect parent-child relationships between
    ///    EventMessageAggregator instances.</para>
    ///    <para>If no <see cref="EventMessageAggregator"/>  is injected into the <see cref="stateChain"/> field, this
    ///    class will try to locate a EventMessageAggregator on the same GameObject.
    ///    </para>
    /// </summary>
    public class EventMessageAggregatorStatePublisher : StreamEventSource
    {
        /// <summary>
        ///    Materialization of a generic UnityEvent.
        /// </summary>
        public class ProgressEvent : UnityEvent<EventMessageAggregatorStatePublisher, BasicEventStreamMessage>
        {
        }

        [SerializeField] EventMessageAggregator stateChain;
        [SerializeField] BasicEventStreamMessage successMessage;
        [SerializeField] ProgressEvent stateChanged;

        public EventMessageAggregatorStatePublisher()
        {
            stateChanged = new ProgressEvent();
        }

        /// <summary>
        ///    Republishes the EventMessageAggregator's matchProgress event.
        /// </summary>
        /// <seealso cref="EventMessageAggregator.matchProgress"/>
        public ProgressEvent StateChanged => stateChanged;

        /// <summary>
        ///    A BasicEventStreamMessage that is fired when the associated EventMessageAggregator successfully
        ///    finished its matching. The EventStream associated with this message will be considered a parent
        ///    stream by the EventStreamTreeModelBuilder.
        /// </summary>
        public BasicEventStreamMessage SuccessMessage => successMessage;

        /// <summary>
        ///    Republishes the associated EventMessageAggregator's current match result state.
        /// </summary>
        public EventMessageMatcherState MatchResult => stateChain.MatchResult;

        /// <summary>
        ///   Queries the internal state of the associated message aggregator. The given buffer will be filled
        ///   with <see cref="EventMessageState"/> structs containing the expected event in the order they are
        ///   expected to be seen as well as flags indicating whether the event has been seen or is expected to
        ///   be seen next. 
        /// </summary>
        /// <param name="buffer">
        ///    A receive buffer. If null, a new list will be created. If the list is non-empty,
        ///    the list will be cleared.
        /// </param>
        /// <returns>The buffer provided or a new list if the buffer given is null.</returns>
        public List<EventMessageState> FetchEvents(List<EventMessageState> buffer)
        {
            return stateChain.ListEvents(buffer);
        }

        void Awake()
        {
            if (stateChain == null)
            {
                stateChain = GetComponent<EventMessageAggregator>();
            }
        }

        void OnEnable()
        {
            if (stateChain != null)
            {
                stateChain.MatchStarting.AddListener(OnMatchStarting);
                stateChain.MatchComplete.AddListener(OnMatchComplete);
                stateChain.MatchProgress.AddListener(OnMatchProgress);
            }
        }

        void OnDisable()
        {
            if (stateChain != null)
            {
                stateChain.MatchStarting.RemoveListener(OnMatchStarting);
                stateChain.MatchComplete.RemoveListener(OnMatchComplete);
                stateChain.MatchProgress.RemoveListener(OnMatchProgress);
            }
        }

        /// <summary>
        ///   Checks whether the message given matches the success message that is fired when the
        ///   EventMessageAggregator successfully matches its events received.
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        public override bool WillGenerateMessage(BasicEventStreamMessage msg)
        {
            return Equals(SuccessMessage, msg);
        }

        void OnMatchStarting()
        {
            StateChanged.Invoke(this, null);
        }

        void OnMatchProgress(EventMessageAggregator source, BasicEventStreamMessage message)
        {
            StateChanged.Invoke(this, message);
        }

        void OnMatchComplete()
        {
            if (successMessage != null)
            {
                successMessage.Publish();
            }
        }
    }
}