using NaughtyAttributes;
using UnityEngine;
using UnityTutorialSystem.Events;

namespace UnityTutorialSystem.Tutorial
{
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