using NaughtyAttributes;
using UnityEngine;
using UnityTutorialSystem.Aggregators;
using UnityTutorialSystem.Events;

namespace UnityTutorialSystem.Tutorial
{
    /// <summary>
    ///  An extended stream message type that contains separate texts for each
    ///  state the message can assume in an <see cref="EventMessageAggregator"/>.
    /// </summary>
    public class TutorialEventMessage : BasicEventStreamMessage
    {
        [SerializeField] [ResizableTextArea] string taskOpenMessage;
        [SerializeField] [ResizableTextArea] string taskSuccessMessage;
        [SerializeField] [ResizableTextArea] string taskFailureMessage;

        public string TaskOpenMessage => taskOpenMessage;

        public string TaskSuccessMessage => taskSuccessMessage;

        public string TaskFailureMessage => taskFailureMessage;
    }
}