using UnityEngine;
using UnityTutorialSystem.Events;

namespace UnityTutorialSystem.Tutorial
{
    [CreateAssetMenu(menuName = "Event Stream/Tutorial Event Stream")]
    public class TutorialEventStream : BasicEventStream
    {
        protected override BasicEventStreamMessage CreateNode()
        {
            return CreateInstance<TutorialEventMessage>();
        }

        public override bool IsValidMessage(BasicEventStreamMessage msg)
        {
            return base.IsValidMessage(msg) && msg is TutorialEventMessage;
        }
    }
}