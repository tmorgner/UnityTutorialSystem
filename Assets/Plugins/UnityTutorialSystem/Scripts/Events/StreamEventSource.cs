using UnityEngine;

namespace UnityTutorialSystem.Events
{
    public abstract class StreamEventSource : MonoBehaviour
    {
        public abstract bool WillGenerateMessage(BasicEventStreamMessage msg);
    }
}