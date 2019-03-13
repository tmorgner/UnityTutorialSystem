using System;
using UnityEngine;
using UnityEngine.Events;

namespace TutorialSystem.Events
{
    public class LogEventStream : MonoBehaviour
    {
        [SerializeField] BasicEventStream stream;
        [SerializeField] bool enabled;
        readonly UnityAction<BasicEventStreamMessage> messageHandler;

        public LogEventStream()
        {
            messageHandler = OnEventReceived;
        }

        void OnDisable()
        {
            if (stream != null)
            {
                stream.ReceivedEvent.RemoveListener(messageHandler);
            }
        }

        void OnEnable()
        {
            if (stream != null)
            {
                stream.ReceivedEvent.AddListener(messageHandler);
            }
        }

        void OnEventReceived(BasicEventStreamMessage message)
        {
            if (enabled)
            {
                Debug.Log(message);
            }
        }

    }
}