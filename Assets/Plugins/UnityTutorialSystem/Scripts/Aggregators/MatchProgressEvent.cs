using UnityEngine.Events;
using UnityTutorialSystem.Events;

namespace UnityTutorialSystem.Aggregators
{
    /// <summary>
    ///  MatchProgressEvents are fired every time the state of an <see cref="EventMessageAggregator"/> changes.
    /// </summary>
    /// <remarks>
    ///  This is an empty materialization of a generic UnityEvent as Unity cannot handle generics well.
    /// </remarks>
    public class MatchProgressEvent : UnityEvent<EventMessageAggregator, BasicEventStreamMessage>
    {
    }
}