using System.Collections.Generic;
using UnityEngine;
using UnityTutorialSystem.Aggregators;
using UnityTutorialSystem.Helpers;

namespace UnityTutorialSystem.Events
{
    public abstract class NextEventSelectorBase : MonoBehaviour
    {
        protected readonly HashSet<EventMessageAggregator> effectiveSources;
        [SerializeField] List<EventMessageAggregator> sources;
        [SerializeField] bool detailedDebugging;
        List<EventMessageState> messageBuffer;
        bool started;

        protected NextEventSelectorBase()
        {
            effectiveSources = new HashSet<EventMessageAggregator>();
            messageBuffer = new List<EventMessageState>();
            sources = new List<EventMessageAggregator>();
        }

        public abstract StreamEventSource NextMessageSource { get; }

        public abstract bool AutoPopulate { get; }

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

                var buffer = new List<EventMessageState>();
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

                    if (IsValidMessageSource(s, buffer))
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

        protected virtual bool IsValidMessageSource(EventMessageAggregator s, List<EventMessageState> buffer)
        {
            if (NextMessageSource == null)
            {
                Debug.LogWarning("No such message source");
                return false;
            }

            buffer = s.ListEvents(buffer);
            foreach (var b in buffer)
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

            DebugLog("Filtered " + s + " because " + buffer.Count + " " + s.enabled);
            return false;
        }

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

        protected virtual void OnEnableForNextMessage()
        {
        }

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

        protected virtual void OnEnable()
        {
            if (!started)
            {
                return;
            }

            foreach (var s in effectiveSources)
            {
                s.MatchProgress.AddListener(OnMatchProgress);
                s.MatchStarting.AddListener(OnMatchRestart);
                s.MatchComplete.AddListener(OnMatchRestart);
            }
        }

        protected virtual void OnDisable()
        {
            foreach (var s in effectiveSources)
            {
                s.MatchProgress.RemoveListener(OnMatchProgress);
                s.MatchStarting.RemoveListener(OnMatchRestart);
                s.MatchComplete.RemoveListener(OnMatchRestart);
            }
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