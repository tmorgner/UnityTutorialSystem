using UnityEngine;

namespace TutorialSystem.Events
{
    [CreateAssetMenu(menuName = "Event Stream/Logging")]
    public class LogEventStream : ScriptableObject
    {
        [SerializeField] BasicEventStream stream;
        [SerializeField] bool enabled;

        void OnDisable()
        {
            if (stream != null)
            {
                stream.ReceivedEvent.RemoveListener(OnEventReceived);
            }
        }

        void OnEventReceived(BasicEventStreamMessage message)
        {
            if (enabled)
            {
                Debug.Log(message);
            }
        }

        void OnEnable()
        {
            if (stream != null)
            {
                stream.ReceivedEvent.AddListener(OnEventReceived);
            }
        }
    }
}