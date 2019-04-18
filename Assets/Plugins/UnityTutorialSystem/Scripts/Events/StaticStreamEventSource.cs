using System.Collections.Generic;
using UnityEngine;

namespace UnityTutorialSystem.Events
{
    /// <summary>
    ///   A stream event source that checks against a set of messages. Use this
    ///   if you need to indicate state or trigger common behaviour for more than one
    ///   message source.  
    /// </summary>
    public class StaticStreamEventSource : StreamEventSource
    {
        [SerializeField] List<BasicEventStreamMessage> messages;

        public override bool WillGenerateMessage(BasicEventStreamMessage msg)
        {
            return (messages != null) && messages.Contains(msg);
        }
    }
}