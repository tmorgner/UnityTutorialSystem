using System.Collections.Generic;
using UnityEngine;
using UnityTutorialSystem.Aggregators;
using UnityTutorialSystem.Helpers;

namespace UnityTutorialSystem.Events
{
    /// <summary>
    /// <para>
    ///   A base class for components that react to 'next event' changes.
    ///   Due to the structured nature of the <see cref="EventMessageAggregator"/>s
    ///   we can ask them to tell when a given message would be expected next.
    /// </para>
    /// <para>
    ///   This class handles all common initialization and maintenance of the
    ///   event listeners.
    /// </para>
    /// </summary>
    public abstract class NextEventSelectorBase : MonoBehaviour
    {
        readonly HashSet<EventMessageAggregator> effectiveSources;
        [SerializeField] List<EventMessageAggregator> sources;
        [SerializeField] bool detailedDebugging;
        List<EventMessageState> messageBuffer;
        bool started;

        /// <inheritdoc />
        protected NextEventSelectorBase()
        {
            effectiveSources = new HashSet<EventMessageAggregator>();
            messageBuffer = new List<EventMessageState>();
            sources = new List<EventMessageAggregator>();
        }

        /// <summary>
        ///   Returns the (modifiable) set of effective message aggregator sources.
        ///   Normally you should not need to modify the collection, but if you do
        ///   while this behaviour is already enabled, make sure you call <see cref="SubscribeTo"/>
        ///   to ensure that this behaviour gets notified of state changes.
        /// </summary>
        protected ICollection<EventMessageAggregator> EffectiveSources => effectiveSources;

        /// <summary>
        ///   Returns the event producer that defines the 'next message' this selector is looking for. This
        ///   is usually the event source that would fire that message if certain success conditions are met.
        /// </summary>
        protected abstract StreamEventSource NextMessageSource { get; }

        /// <summary>
        ///   Defines whether this source will auto-populate the event message aggregator objects from instances
        ///   defined in the scene. If set to false, it will only look at the sources defined in the injected
        ///   <see cref="sources"/> collection.
        /// </summary>
        protected abstract bool AutoPopulate { get; }

        /// <summary>
        ///   An internal method called during the initialization that collects the <see cref="EventMessageAggregator"/>
        ///   instances referenced in the sources list and scene (if auto-populate is enabled).
        /// </summary>
        protected virtual void PopulateSources()
        {
            foreach (var s in sources)
            {
                if (s != null)
                {
                    effectiveSources.Add(s);
                }
            }

            if (AutoPopulate)
            {
                DebugLog("Start auto-populating " + name);
                var aggregator = SharedMethods.FindEvenInactiveComponents<EventMessageAggregator>(true);
                if (aggregator.Length == 0)
                {
                    DebugLog("No aggregators found.");
                }

                foreach (var s in aggregator)
                {
                    if (s == null)
                    {
                        continue;
                    }

                    if (IsValidMessageSource(s))
                    {
                        effectiveSources.Add(s);
                    }
                }
            }

            DebugLog("After populate " + name + " contains " + effectiveSources.Count + " active sources.");
        }

        void DebugLog(string message)
        {
            if (detailedDebugging)
            {
                Debug.Log(message);
            }
        }

        /// <summary>
        ///   Validates that the given message aggregator is able to generate the <see cref="BasicEventStreamMessage"/>
        ///   produced by the NextMessageSource. If an aggregator cannot generate that message, there is no point
        ///   in actually subscribing for update events.
        /// </summary>
        /// <param name="source">the potential message source</param>
        /// <returns>true if the source can generate the NextMessage, false otherwise</returns>
        protected virtual bool IsValidMessageSource(EventMessageAggregator source)
        {
            if (NextMessageSource == null)
            {
                Debug.LogWarning("No such message source");
                return false;
            }

            messageBuffer = source.ListEvents(messageBuffer);
            foreach (var b in messageBuffer)
            {
                if (FilterMessage(b.Message))
                {
                    DebugLog("Filtered " + b.Message);
                    continue;
                }

                if (NextMessageSource.WillGenerateMessage(b.Message))
                {
                    return true;
                }

                DebugLog("No match for " + b.Message);
            }

            DebugLog("Filtered " + source + " because " + messageBuffer.Count + " " + source.enabled);
            return false;
        }

        /// <summary>
        ///    Tests whether this message should be ignored. This default implementation simply checks the
        ///    <see cref="BasicEventStream.IgnoreForNextEventIndicatorHint"/> property of the
        ///    <see cref="BasicEventStream"/>.
        /// </summary>
        /// <param name="msg">The message.</param>
        /// <returns>true if the message should be ignored, false otherwise.</returns>
        protected virtual bool FilterMessage(BasicEventStreamMessage msg)
        {
            var stream = msg.Stream;
            if (stream != null)
            {
                return stream.IgnoreForNextEventIndicatorHint;
            }

            return false;
        }

        void Start()
        {
            if (!started)
            {
                started = true;
                PopulateSources();
                OnEnable();
            }

            OnMatchRestart();
        }

        /// <summary>
        ///   An override point that is called when the next-message is expected next. Use this to enable
        ///   your dependent behaviours.
        /// </summary>
        protected virtual void OnEnableForNextMessage()
        {
        }

        /// <summary>
        ///   An override point that is called when the next-message is expected next. Use this to disable
        ///   your dependent behaviours.
        /// </summary>
        protected virtual void OnDisableForNextMessage()
        {
        }


        void OnMatchProgress(EventMessageAggregator source, BasicEventStreamMessage message)
        {
            if (!source.enabled)
            {
                return;
            }

            if (FilterMessage(message))
            {
                return;
            }

            // Find a message source that can generate this message.
            // If none is found, then this is not one of the handled messages.
            messageBuffer = source.ListEvents(messageBuffer);
            var isMatch = false;
            foreach (var m in messageBuffer)
            {
                if (m.ExpectedNext && NextMessageSource.WillGenerateMessage(m.Message))
                {
                    isMatch = true;
                    DebugLog("OnMatchProgress: " + transform.name + " (match) via " + m.Message);
                    break;
                }
            }


            if (isMatch)
            {
                OnEnableForNextMessage();
            }
            else
            {
                OnDisableForNextMessage();
            }
        }

        /// <summary>
        ///   Unity callback when the behaviour is enabled. If you override this, make sure
        ///   you call this base method.
        /// </summary>
        protected virtual void OnEnable()
        {
            if (!started)
            {
                return;
            }

            foreach (var s in effectiveSources)
            {
                SubscribeTo(s);
            }
        }

        /// <summary>
        ///   Unity callback when the behaviour is disabled. If you override this, make sure
        ///   you call this base method.
        /// </summary>
        protected virtual void OnDisable()
        {
            foreach (var s in effectiveSources)
            {
                UnsubscribeFrom(s);
            }
        }

        /// <summary>
        ///   Subscribes to all relevant events on the given <see cref="EventMessageAggregator"/>.
        ///   Use this if your own instances in a <see cref="PopulateSources"/> override.
        /// </summary>
        /// <param name="s">The message aggregator to subscribe to</param>
        protected void SubscribeTo(EventMessageAggregator s)
        {
            s.MatchProgress.AddListener(OnMatchProgress);
            s.MatchStarting.AddListener(OnMatchRestart);
            s.MatchComplete.AddListener(OnMatchRestart);
        }

        /// <summary>
        ///   Unsubscribes from all relevant events on the given <see cref="EventMessageAggregator"/>.
        ///   Use this if your own instances in a <see cref="PopulateSources"/> override.
        /// </summary>
        /// <param name="s">The message aggregator to subscribe to</param>
        protected void UnsubscribeFrom(EventMessageAggregator s)
        {
            s.MatchProgress.RemoveListener(OnMatchProgress);
            s.MatchStarting.RemoveListener(OnMatchRestart);
            s.MatchComplete.RemoveListener(OnMatchRestart);
        }

        void OnMatchRestart()
        {
            var isMatch = false;
            foreach (var source in effectiveSources)
            {
                if (!source.enabled)
                {
                    continue;
                }

                messageBuffer = source.ListEvents(messageBuffer);
                foreach (var m in messageBuffer)
                {
                    if (FilterMessage(m.Message))
                    {
                        continue;
                    }

                    if (m.ExpectedNext && NextMessageSource.WillGenerateMessage(m.Message))
                    {
                        isMatch = true;
                        DebugLog("OnStart: " + transform.name + " " + (isMatch ? "(match)" : "(no match)") + " via " + m.Message);
                        break;
                    }
                }
            }

            if (!isMatch)
            {
                DebugLog("OnStart: " + transform.name + " (no match) -> " + messageBuffer.Count + " => " + effectiveSources.Count);
            }


            if (isMatch)
            {
                OnEnableForNextMessage();
            }
            else
            {
                OnDisableForNextMessage();
            }
        }
    }
}