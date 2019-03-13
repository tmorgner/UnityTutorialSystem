using System;
using NaughtyAttributes;
using UnityEngine;
using UnityTutorialSystem.Events;

namespace UnityTutorialSystem.Tutorial
{
    public enum TutorialEventState
    {
        Open,
        Success,
        Failure
    }

    public class TutorialEventMessage : BasicEventStreamMessage
    {
        [SerializeField] [ResizableTextArea] string taskOpenMessage;
        [SerializeField] [ResizableTextArea] string taskSuccessMessage;
        [SerializeField] [ResizableTextArea] string taskFailureMessage;

        public string TaskOpenMessage => taskOpenMessage;

        public string TaskSuccessMessage => taskSuccessMessage;

        public string TaskFailureMessage => taskFailureMessage;

        public string Message(TutorialEventState msg)
        {
            switch (msg)
            {
                case TutorialEventState.Open:
                    return TaskOpenMessage;
                case TutorialEventState.Success:
                    return TaskSuccessMessage;
                case TutorialEventState.Failure:
                    return TaskFailureMessage;
                default:
                    throw new ArgumentOutOfRangeException(nameof(msg), msg, null);
            }
        }

        public void CopyFrom(BasicEventStreamMessage msg)
        {
            Stream = msg.Stream;
            taskOpenMessage = msg.name;
            taskFailureMessage = msg.name;
            taskSuccessMessage = msg.name;
            taskOpenMessage = msg.name;
        }
    }
}