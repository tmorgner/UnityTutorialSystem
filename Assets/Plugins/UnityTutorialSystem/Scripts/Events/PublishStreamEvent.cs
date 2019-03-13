using JetBrains.Annotations;
using UnityEngine;

namespace UnityTutorialSystem.Events
{
    /// <summary>
    ///     This class exists as an trigger to fire events from UnityEvent bindings.
    ///     Attach this class to your game object and in your custom scripts, fire
    ///     an event that calls the TriggerEvent method.
    ///     The message defined in the inspector will be published to the message's
    ///     associated event stream. From there aggregators and reactors can pick up
    ///     the state change that has been signaled here.
    ///     Architectural note: Messages do not carry source object information. At
    ///     the moment they also do not carry any parameter information. This is a
    ///     deliberate act, as event stream messages are not intended to replace
    ///     normal event processing. The messages defined in this package are used
    ///     to provide analytics, achievement and tutorial state triggers only.
    /// </summary>
    public class PublishStreamEvent : StreamEventSource
    {
        [SerializeField] BasicEventStreamMessage message;

        public override bool WillGenerateMessage(BasicEventStreamMessage msg)
        {
            return Equals(message, msg);
        }

        [UsedImplicitly]
        public void TriggerEvent()
        {
            if (message != null)
            {
                message.Publish();
            }
        }
    }
}