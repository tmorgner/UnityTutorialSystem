using UnityEngine;

namespace TutorialSystem.Events
{
    public abstract class StreamEventSource : MonoBehaviour
    {
        public abstract bool WillGenerateMessage(BasicEventStreamMessage msg);
    }
}