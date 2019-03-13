using UnityEngine;
using UnityEngine.Events;

namespace UnityTutorialSystem.Events
{
    public class LogEventStream : MonoBehaviour
    {
        [SerializeField] BasicEventStream stream;
        [SerializeField] bool printMessages;
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
            if (printMessages)
            {
                Debug.Log(message);
            }
        }

    }
}