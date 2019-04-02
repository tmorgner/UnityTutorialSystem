using NaughtyAttributes;
using UnityEngine;

namespace UnityTutorialSystem.Events
{
    /// <summary>
    ///      The BasicEventStreamMessage is a generic message emitted by a BasicEventStream.
    ///      Messages are identified by their asset name. Each message holds a reference to the
    ///      message stream that can publish it. Use the <see cref="Publish"/> method to send 
    ///      this event to all subscribers of the message stream.
    /// </summary>
    public class BasicEventStreamMessage : ScriptableObject
    {
        [SerializeField] [ReadOnly] BasicEventStream stream;
        [SerializeField] bool allowOutOfOrderExecution;

        /// <summary>
        ///   Defines whether this message accepts out-of-order processing of sibling events in the
        ///   same event message aggregator.
        /// </summary>
        public bool AllowOutOfOrderExecution => allowOutOfOrderExecution;

        /// <summary>
        ///   The message stream associated with this message.
        /// </summary>
        public BasicEventStream Stream
        {
            get => stream;
            protected set => stream = value;
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