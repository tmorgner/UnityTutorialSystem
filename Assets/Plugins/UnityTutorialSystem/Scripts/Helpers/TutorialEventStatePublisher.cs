using System.Collections.Generic;
using TutorialSystem.Aggregators;
using TutorialSystem.Events;
using UnityEngine;
using UnityEngine.Events;

namespace TutorialSystem.Helpers
{
    public class TutorialProgressEvent : UnityEvent<TutorialEventStatePublisher, BasicEventStreamMessage>
    {
    }

    public class TutorialEventStatePublisher : StreamEventSource
    {
        [SerializeField] EventMessageAggregator stateChain;
        [SerializeField] TutorialEventMessage successMessage;
        [SerializeField] TutorialProgressEvent stateChanged;

        public TutorialEventStatePublisher()
        {
            stateChanged = new TutorialProgressEvent();
        }

        public TutorialProgressEvent StateChanged => stateChanged;
        public TutorialEventMessage SuccessMessage => successMessage;

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