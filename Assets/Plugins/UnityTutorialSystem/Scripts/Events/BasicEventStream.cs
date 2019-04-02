using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Events;

namespace UnityTutorialSystem.Events
{
    /// <summary>
    ///     A event stream publishes preconfigured messages. Both the event stream and the
    ///     messages are <see cref="ScriptableObject"/>s defined as assets in the Unity editor.
    ///     Each message defined for an event stream object contains a reference to the event
    ///     stream that declared it. You can publish the message via its <see cref="BasicEventStreamMessage.Publish"/>
    ///     method.
    /// </summary>
    /// <remarks>
    ///     Publishing messages on an EventStreams is a reentrant operation. Messages are guaranteed
    ///     to be sent in the order received. If - as an result of an published message - a listener
    ///     of this stream publishes new messages these messages will be processed after all other pending
    ///     messages have been finished. This class enforces a hard limit of 250 processed message per
    ///     message cascade. Any excess message will be dropped. 
    /// </remarks>
    [CreateAssetMenu(menuName = "Event Stream/Event Stream")]
    public class BasicEventStream : ScriptableObject
    {
        [SerializeField] bool ignoreForNextEventIndicatorHint;
        [SerializeField] [ReorderableList] List<string> messageTypes;
        [SerializeField] bool debug;

        [NonSerialized] readonly Queue<BasicEventStreamMessage> queuedMessages;
        [NonSerialized] bool processingMessage;

        public BasicEventStream()
        {
            messageTypes = new List<string>();
            receivedEvent = new EventStreamReceivedEvent();
            queuedMessages = new Queue<BasicEventStreamMessage>();
            processingMessage = false;
        }

        /// <summary>
        ///   A flag defining that messages defined by this event stream should not be used to
        ///   display visual indicators for actions that generate the next events. Use this
        ///   for events that represent internal state. Remember that aggregator implementations
        ///   can track events from multiple sources.   
        /// </summary>
        public bool IgnoreForNextEventIndicatorHint => ignoreForNextEventIndicatorHint;

        /// <summary>
        ///   Checks whether the message given is a valid message that belongs to this event stream.   
        /// </summary>
        /// <param name="msg">the message being tested</param>
        /// <returns></returns>
        public virtual bool IsValidMessage([CanBeNull] BasicEventStreamMessage msg)
        {
            return msg != null && messageTypes.Contains(msg.name);
        }

        void Awake()
        {
            processingMessage = false;
        }

#if UNITY_EDITOR
        /// <summary>
        /// An editor function to generate message objects from the list of messages defined in <see cref="messageTypes"/>.
        /// </summary>
        [Button("Generate Message Handles")]
        [UsedImplicitly]
        internal void Regenerate()
        {
            var retainedNodes = BasicEventStreamEditorSupport.CleanGraph(this);
            var generatedNodes = new List<BasicEventStreamMessage>();

            foreach (var node in messageTypes)
            {
                if (node == null)
                {
                    continue;
                }

                BasicEventStreamMessage streamNode;
                if (retainedNodes.TryGetValue(node, out streamNode))
                {
                    streamNode.SetUpStream(this);
                    continue;
                }

                var msg = CreateNode();
                msg.name = node;
                msg.SetUpStream(this);
                generatedNodes.Add(msg);
            }

            BasicEventStreamEditorSupport.GenerateNodes(this, generatedNodes);
        }

        /// <summary>
        ///   A factory function to create new message event sub-objects.
        /// </summary>
        /// <returns>the newly created message object</returns>
        protected virtual BasicEventStreamMessage CreateNode()
        {
            return CreateInstance<BasicEventStreamMessage>();
        }
#endif

        /// <summary>
        ///     Publishes the message if the message given is defined by this event stream.
        ///     Publishing is a reentrant operation and newly generated messages will be
        ///     processed in the order they are received. This method will not process more than
        ///     250 messages in a single cascade.  
        /// </summary>
        /// <param name="msg">the message being published</param>
        public void Publish(BasicEventStreamMessage msg)
        {
            if (msg.Stream != this)
            {
                return;
            }

            if (processingMessage)
            {
                if (queuedMessages.Count > 250)
                {
                    Debug.LogError("More than 250 messages in a single cascade. Aborting.");
                    return;
                }

                DebugLog("=== Queue message " + msg);
                queuedMessages.Enqueue(msg);
            }
            else
            {
                processingMessage = true;
                try
                {
                    DebugLog("=== Begin Publish message " + msg);
                    ReceivedEvent?.Invoke(msg);
                    DebugLog("=== End Publish message " + msg);

                    var cascadeCount = 0;
                    while (cascadeCount < 250 && queuedMessages.Count > 0)
                    {
                        var message = queuedMessages.Dequeue();
                        DebugLog("=== Begin Publish queued message " + message);
                        ReceivedEvent?.Invoke(message);
                        DebugLog("=== End Publish queued message " + message);
                        cascadeCount += 1;
                    }

                    queuedMessages.Clear();
                }
                finally
                {
                    processingMessage = false;
                }
            }
        }

        void DebugLog(string message)
        {
#if DEBUG
            if (debug)
            {
                Debug.Log(message, this);
            }
#endif
        }

        #region ReceivedEvent    

        /// <summary>
        ///     Unity cannot serialize or deserialize generic classes. So to use a generic class,
        ///     we have to derive a concrete (aka non-generic) subclass for it. Oh, well ..
        /// </summary>
        [UsedImplicitly]
        class EventStreamReceivedEvent : UnityEvent<BasicEventStreamMessage>
        {
        }

        [SerializeField] EventStreamReceivedEvent receivedEvent;

        /// <summary>
        ///     Subscribe to the ReceivedEvent event to get notified when a new message has been
        ///     published. 
        /// </summary>
        public UnityEvent<BasicEventStreamMessage> ReceivedEvent => receivedEvent;

        #endregion
    }
}