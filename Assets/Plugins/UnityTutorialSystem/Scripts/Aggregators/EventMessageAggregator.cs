using System.Collections.Generic;
using JetBrains.Annotations;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Events;
using UnityTutorialSystem.Events;

namespace UnityTutorialSystem.Aggregators
{
    /// <summary>
    ///     Aggregates low-level event stream messages into higher level aggregate events.
    ///     An typical EventMessageAggregator monitors one or more event streams to detect
    ///     predefined sequences of events in the stream. When an event sequence is fully
    ///     matched, the built-in events of this class can be used to fire a new event stream
    ///     message via an associated <see cref="EventMessageAggregatorStatePublisher" />.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         An EventMessageAggregator follows a defined lifecycle.
    ///     </para>
    ///     <para>
    ///         During the <see cref="Awake" /> event the aggregator will collect data from the
    ///         given configuration (usually a set of <see cref="BasicEventStreamMessage" /> objects
    ///         injected into the <see cref="Messages" /> property) and will build up any
    ///         matcher-specific internal data structures during repeated calls to
    ///         <see cref="RegisterValidMessage" />. As part of the initialization, the aggregator
    ///         will subscribe to the associated event streams of all monitored messages.
    ///     </para>
    ///     <para>
    ///         While the aggregation is active and not in a terminal state (success or failure)
    ///         this class will react to incoming <see cref="BasicEventStreamMessage"/>s. Any
    ///         internal state change will trigger the <see cref="MatchProgress"/> event.
    ///     </para>
    ///     <para>
    ///          This implementation assumes that the set of receivable messages does not change
    ///          during the lifetime of the instance.
    ///     </para>
    /// </remarks>
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

        /// <summary>
        ///  This event is fired when the aggregator is ready to receive messages after this MonoBehaviour became active.
        /// </summary>
        public UnityEvent MatchStarting => matchStarting;
        
        /// <summary>
        ///    This event is fired when the aggregator has successfully finished matching events. Be aware that this event
        ///    is NOT fired when the matching fails with an error. To receive error information subscribe to the MatchProgress
        ///    event and check the sender's MatchResult property for failures. 
        /// </summary>
        public UnityEvent MatchComplete => matchComplete;
        
        /// <summary>
        ///    This event is fired whenever any progress has been made. Any internal state change will always result in a
        ///    MatchProgress event.
        /// </summary>
        public MatchProgressEvent MatchProgress => matchProgress;

        /// <summary>
        ///   Resets the aggregator to the initial state as if no message has been received yet.
        /// </summary>
        public abstract void ResetMatch();

        /// <summary>
        ///   Queries the internal state of the message aggregator. The given buffer will be filled with <see cref="EventMessageState"/>
        ///   structs containing the expected event in the order they are expected to be seen as well as flags indicating whether
        ///   the event has been seen or is expected to be seen next. 
        /// </summary>
        /// <param name="buffer">A receive buffer. If null, a new list will be created. If the list is non-empty, the list will be cleared.</param>
        /// <returns>The buffer provided or a new list if the buffer given is null.</returns>
        public abstract List<EventMessageState> ListEvents(List<EventMessageState> buffer);

        /// <summary>
        ///   Returns the current state of the matching process. 
        /// </summary>
        public virtual EventMessageMatcherState MatchResult { get; protected set; }

        void Awake()
        {
            RegisterValidMessages();
        }

        /// <summary>
        ///   Registers all event messages. This method filters out any messages injected that are
        ///   either <c>null</c> or that have no <see cref="BasicEventStream"/> associated. 
        /// </summary>
        /// <remarks>
        ///   This default processing of the injected event messages can be disabled by overriding <see cref="OnBeforeRegisterMessages"/>
        ///   and returning <c>false</c> in that method.
        /// </remarks>
        void RegisterValidMessages()
        {
            if (OnBeforeRegisterMessages())
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

            OnAfterRegisterMessages();
        }

        /// <summary>
        ///  An extension point to allow to skip the default message registration. 
        /// </summary>
        /// <returns>true if the default message processing should commence, false otherwise.</returns>
        protected virtual bool OnBeforeRegisterMessages()
        {
            return true;
        }

        /// <summary>
        ///  An extension point to allow post-processing of the registered messages and the matcher state.
        /// </summary>
        protected virtual void OnAfterRegisterMessages()
        {
        }

        /// <summary>
        ///  A service callback to register the given message. The message is guaranteed to be non-null and
        ///  to have an associated event stream.
        /// </summary>
        /// <param name="m">The message</param>
        protected abstract void RegisterValidMessage([NotNull] BasicEventStreamMessage m);

        /// <summary>
        ///  Registers as listener to all known event streams and fires the MatchStarting event.  
        /// </summary>
        protected virtual void OnEnable()
        {
            foreach (var stream in streams)
            {
                stream.ReceivedEvent.AddListener(OnEventReceivedWrapper);
            }

            MatchStarting?.Invoke();
        }

        /// <summary>
        ///  Unregisters as listener from all known event streams.  
        /// </summary>
        protected virtual void OnDisable()
        {
            foreach (var stream in streams)
            {
                stream.ReceivedEvent.RemoveListener(OnEventReceivedWrapper);
            }
        }

        /// <summary>
        ///   Callback that is invoked whenever one of the event streams generated a new message. If the message
        ///   received is not one of the defined messages for this aggregator or if the aggregator is in a terminal
        ///   state, the message will be ignored. 
        /// </summary>
        /// <param name="message">The message received from the event streams</param>
        void OnEventReceivedWrapper(BasicEventStreamMessage message)
        {
            if (MatchResult != EventMessageMatcherState.Waiting)
            {
                DebugLog("Not Handling event " + message + " in " + gameObject.name + " -> " + MatchResult);
                return;
            }

            if (!Messages.Contains(message))
            {
                return;
            }

            if (OnEventReceived(message))
            {
                DebugLog("Handling event " + message + " in " + gameObject.name);
                MatchProgress?.Invoke(this, message);
            }
            else
            {
                DebugLog("Rejected event " + message + " in " + gameObject.name);
            }
        }

        /// <summary>
        ///   A callback used when event messages have been received from one of the underlying <see cref="BasicEventStream"/>s.
        /// </summary>
        /// <param name="received">The message received from the event stream.</param>
        /// <returns></returns>
        protected abstract bool OnEventReceived(BasicEventStreamMessage received);
        
        /// <summary>
        ///   Filters the log messages by the <c>debug</c> flag.
        /// </summary>
        /// <param name="message"></param>
        void DebugLog(string message)
        {
#if DEBUG
            if (debug)
            {
                Debug.Log(name + ": " + message, this);
            }
#endif
        }
        
        /// <summary>
        ///  Simple helper function that ensures that the buffer used in ListEvents is correctly
        ///  initialised. If a list is 
        /// </summary>
        /// <param name="buffer">a list buffer, or null to create a new buffer</param>
        /// <returns>The buffer or a new list.</returns>
        [NotNull] protected static List<EventMessageState> EnsureBufferValid([CanBeNull] List<EventMessageState> buffer, int capacity = 0)
        {
            if (buffer == null)
            {
                buffer = new List<EventMessageState>(capacity);
            }
            else
            {
                buffer.Clear();
                buffer.Capacity = Mathf.Max(buffer.Capacity, capacity);
            }

            return buffer;
        }

    }
}