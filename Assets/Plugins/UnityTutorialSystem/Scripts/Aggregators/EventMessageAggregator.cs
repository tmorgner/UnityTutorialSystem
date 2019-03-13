using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Events;
using UnityTutorialSystem.Events;

namespace UnityTutorialSystem.Aggregators
{
    public class MatchProgressEvent : UnityEvent<EventMessageAggregator, BasicEventStreamMessage>
    {
    }

    public abstract class EventMessageAggregator : MonoBehaviour, IEventMessageAggregator
    {
        readonly HashSet<BasicEventStream> streams;
        [SerializeField] [ReorderableList] protected List<BasicEventStreamMessage> Messages;
        [SerializeField] UnityEvent matchStarting;
        [SerializeField] UnityEvent matchComplete;
        [SerializeField] MatchProgressEvent matchProgress;
        [SerializeField] bool debug;

        protected EventMessageAggregator()
        {
            streams = new HashSet<BasicEventStream>();
            matchComplete = new UnityEvent();
            matchProgress = new MatchProgressEvent();
        }

        public UnityEvent MatchStarting => matchStarting;
        public UnityEvent MatchComplete => matchComplete;
        public MatchProgressEvent MatchProgress => matchProgress;

        public abstract void ResetMatch();

        public abstract List<EventMessageState> ListEvents(List<EventMessageState> buffer);

        public virtual EventMessageMatcherState MatchResult { get; protected set; }

        protected abstract bool OnEventReceived(BasicEventStreamMessage received);

        void Awake()
        {
            RegisterValidMessages();
        }

        void DebugLog(string message)
        {
#if DEBUG
            if (debug)
            {
                Debug.Log(name + ": " + message);
            }
#endif
        }

        protected virtual void OnEnable()
        {
            foreach (var stream in streams)
            {
                stream.ReceivedEvent.AddListener(OnEventReceivedWrapper);
            }

            MatchStarting?.Invoke();
        }

        void OnEventReceivedWrapper(BasicEventStreamMessage arg0)
        {
            if (MatchResult != EventMessageMatcherState.Waiting)
            {
                DebugLog("Not Handling event " + arg0 + " in " + gameObject.name + " -> " + MatchResult);
                return;
            }


            if (OnEventReceived(arg0))
            {
                DebugLog("Handling event " + arg0 + " in " + gameObject.name);
                MatchProgress?.Invoke(this, arg0);
            }
            else
            {
                DebugLog("Rejected event " + arg0 + " in " + gameObject.name);
            }
        }

        protected abstract void RegisterValidMessage(BasicEventStreamMessage m);

        void RegisterValidMessages()
        {
            DebugLog("Registering .. " + name);
            foreach (var m in Messages)
            {
                if (m == null)
                {
                    continue;
                }

                RegisterValidMessage(m);
                if (m.Stream != null)
                {
                    streams.Add(m.Stream);
                }
                else
                {
                    Debug.LogError("Unable to process message that has no publishing stream. " + m);
                }
            }
        }

        protected virtual void OnDisable()
        {
            foreach (var stream in streams)
            {
                stream.ReceivedEvent.RemoveListener(OnEventReceivedWrapper);
            }
        }
    }
}