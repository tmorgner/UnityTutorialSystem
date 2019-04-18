using UnityEngine;
using UnityTutorialSystem.Events;

namespace UnityTutorialSystem.Tutorial
{
    /// <summary>
    ///  An event stream type that contains <see cref="TutorialEventMessage"/> instances
    ///  instead of plain <see cref="BasicEventStreamMessage"/> instances.
    /// </summary>
    [CreateAssetMenu(menuName = "Event Stream/Tutorial Event Stream")]
    public class TutorialEventStream : BasicEventStream
    {
        protected override BasicEventStreamMessage CreateNode()
        {
            return CreateInstance<TutorialEventMessage>();
        }

        /// <summary>
        ///   Validates that the message given is valid for this stream.
        ///   A message is valid if it is both an TutorialEventMessage
        ///   instance and a registered child of the stream.
        /// </summary>
        /// <param name="msg">message to test</param>
        /// <returns>true, if the message is owned by this stream, false otherwise.</returns>
        public override bool IsValidMessage(BasicEventStreamMessage msg)
        {
            return base.IsValidMessage(msg) && msg is TutorialEventMessage;
        }
    }
}