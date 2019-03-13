using System.Collections.Generic;
using UnityEngine;

namespace UnityTutorialSystem.Events
{
    public class StaticStreamEventSource : StreamEventSource
    {
        [SerializeField] List<BasicEventStreamMessage> messages;

        public override bool WillGenerateMessage(BasicEventStreamMessage msg)
        {
            return (messages != null) && messages.Contains(msg);
        }
    }
}