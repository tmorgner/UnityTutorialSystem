using NaughtyAttributes;
using UnityEngine;

namespace UnityTutorialSystem.Events
{
    public class BasicEventStreamMessage : ScriptableObject
    {
        [SerializeField] [ReadOnly] BasicEventStream stream;
        [SerializeField] bool allowOutOfOrderExecution;

        public bool AllowOutOfOrderExecution => allowOutOfOrderExecution;

        public BasicEventStream Stream
        {
            get { return stream; }
            protected internal set { stream = value; }
        }

        internal void SetUpStream(BasicEventStream eventStream)
        {
            stream = eventStream;
        }

        public void Publish()
        {
            if (stream != null)
            {
                stream.Publish(this);
            }
        }
    }
}