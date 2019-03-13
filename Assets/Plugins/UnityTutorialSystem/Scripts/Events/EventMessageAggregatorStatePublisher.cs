using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityTutorialSystem.Aggregators;

namespace UnityTutorialSystem.Events
{
    public class EventMessageAggregatorStatePublisher : StreamEventSource
    {
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

        public ProgressEvent StateChanged => stateChanged;
        public BasicEventStreamMessage SuccessMessage => successMessage;

        public EventMessageMatcherState MatchResult => stateChain.MatchResult;

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

            Debug.Log("Fired success message " + successMessage);
        }
    }
}