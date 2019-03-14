using System.Collections.Generic;
using System.Linq;
using NaughtyAttributes;
using UnityEngine;
using UnityTutorialSystem.Aggregators;
using UnityTutorialSystem.Events;
using UnityTutorialSystem.UI.Trees;

namespace UnityTutorialSystem.Tutorial
{
    public class TutorialEventStreamManager : MonoBehaviour
    {
        readonly List<EventMessageState> buffer;

        [SerializeField] [ReorderableList] List<EventMessageAggregatorStatePublisher> tutorialStateTrackers;
        [SerializeField] bool debug;

        ListTreeModel<TutorialEventStateData> model;
        Dictionary<EventMessageAggregatorStatePublisher, TutorialEventStateData> nodeMapper;
        TutorialEventStateData rootNode;
        bool started;

        public TutorialEventStreamManager()
        {
            buffer = new List<EventMessageState>();
            model = new ListTreeModel<TutorialEventStateData>();
        }

        public ITreeModel<TutorialEventStateData> Model => model;

        void DebugLog(string message)
        {
#if DEBUG
            if (debug)
            {
                Debug.Log(name + ": " + message);
            }
#endif
        }

        void Start()
        {
            nodeMapper = new Dictionary<EventMessageAggregatorStatePublisher, TutorialEventStateData>();
            started = true;

            var data = CollectTreeData();
            PrintTree(data);
            rootNode = CreateTreeNodes(data, nodeMapper);
            DebugLog("Generated tree:\n" + rootNode.AsTextTree());
            model.Root = rootNode;

            foreach (var d in tutorialStateTrackers)
            {
                OnTrackerStateChanged(d);
            }
        }

        void OnEnable()
        {
            foreach (var d in tutorialStateTrackers)
            {
                d.StateChanged.AddListener(OnTrackerStateChanged);
            }
        }

        void OnDisable()
        {
            foreach (var d in tutorialStateTrackers)
            {
                d.StateChanged.RemoveListener(OnTrackerStateChanged);
            }
        }

        void OnTrackerStateChanged(EventMessageAggregatorStatePublisher source, BasicEventStreamMessage message = null)
        {
            if (!started)
            {
                return;
            }

            TutorialEventStateData data;
            if (!nodeMapper.TryGetValue(source, out data))
            {
                data = rootNode;
            }

            source.FetchEvents(buffer);

            for (var index = 0; index < buffer.Count; index++)
            {
                var b = buffer[index];
                var c = data[index];
                if (b.Message == c.SourceMessage)
                {
                    c.Completed = b.Completed;
                    c.ExpectedNext = b.ExpectedNext;
                }
                else
                {
                    Debug.LogWarning("Inconsistent data model from source " + source);
                }
            }

            // Special case handling: child-nodes of root won't ever receive complete events as
            // there is no aggregator listening for the completed notification. 

            foreach (var rootChild in rootNode)
            {
                if (rootChild == data)
                {
                    rootChild.Completed = source.MatchResult == EventMessageMatcherState.Success;
                }
            }

            model.FireStructureChanged();
        }

        List<ExpectedStates> CollectTreeData()
        {
            var data = new List<ExpectedStates>();
            foreach (var n in tutorialStateTrackers)
            {
                var messages = n.FetchEvents(buffer);
                data.Add(new ExpectedStates(n, n.SuccessMessage, messages, debug));
            }

            foreach (var self in data)
            {
                foreach (var n in data)
                {
                    if (self.MessageSource == n.MessageSource)
                    {
                        continue;
                    }

                    if (self.ExpectsMessage(n.SuccessMessage))
                    {
                        self.AddChild(n);
                    }
                }
            }

            return data.Where(d => !d.IsDependency).ToList();
        }

        TutorialEventStateData CreateTreeNodes(List<ExpectedStates> data,
                                               Dictionary<EventMessageAggregatorStatePublisher, TutorialEventStateData> nodeMapper)
        {
            var roots = data.Where(s => !s.IsDependency).ToList();
            var childStates = new List<TutorialEventStateData>();
            foreach (var s in roots)
            {
                var item = AddTree(s, nodeMapper);
                item.ExpectedNext = false;
                AddGeneratedChildItems(childStates, item);
            }

            return new TutorialEventStateData(null, false, true, childStates);
        }

        static void AddGeneratedChildItems(List<TutorialEventStateData> childStates, TutorialEventStateData item)
        {
            if (item.SourceMessage != null)
            {
                item.ExpectedNext = false;
                childStates.Add(item);
                return;
            }

            foreach (var child in item)
            {
                AddGeneratedChildItems(childStates, child);
            }
        }

        static TutorialEventMessage Convert(BasicEventStreamMessage msg)
        {
            var message = msg as TutorialEventMessage;
            if (message != null)
            {
                return message;
            }

            Debug.LogError("Old message format encountered");
            var m = ScriptableObject.CreateInstance<TutorialEventMessage>();
            m.CopyFrom(msg);
            return m;
        }

        static TutorialEventStateData AddTree(ExpectedStates state,
                                              Dictionary<EventMessageAggregatorStatePublisher, TutorialEventStateData> nodeMapper)
        {
            var childStates = new List<TutorialEventStateData>();
            foreach (var s in state.RequiredMessages)
            {
                ExpectedStates dependency;
                if (state.IsProvidedByDependency(s.Message, out dependency))
                {
                    var item = AddTree(dependency, nodeMapper);
                    item.ExpectedNext = s.ExpectedNext;
                    childStates.Add(item);
                }
                else
                {
                    var message = (s.Message);
                    childStates.Add(new TutorialEventStateData(message, s.Completed, s.ExpectedNext));
                }
            }

            var stateData = new TutorialEventStateData(state.SuccessMessage, state.Completed, false, childStates);
            nodeMapper[state.MessageSource] = stateData;
            return stateData;
        }

        void PrintTree(List<ExpectedStates> states)
        {
            if (!debug)
            {
                return;
            }

            DebugLog("---- START TREE DUMP ---");
            foreach (var state in states)
            {
                PrintTree(state, 0);
            }

            DebugLog("---- END TREE DUMP ---");
        }

        void PrintTree(ExpectedStates state, int indent)
        {
            DebugLog($"{$"{indent}".PadLeft(2).PadRight(indent * 4)}:{state.MessageSource}");
            foreach (var s in state.Dependencies)
            {
                PrintTree(s, indent + 1);
            }
        }

        class ExpectedStates
        {
            public readonly List<EventMessageState> RequiredMessages;
            public readonly EventMessageAggregatorStatePublisher MessageSource;
            public readonly BasicEventStreamMessage SuccessMessage;
            public readonly HashSet<ExpectedStates> Dependencies;
            readonly bool debug;

            public ExpectedStates(EventMessageAggregatorStatePublisher message,
                                  BasicEventStreamMessage successMessage,
                                  List<EventMessageState> requiredMessages,
                                  bool debug)
            {
                MessageSource = message;
                SuccessMessage = successMessage;
                RequiredMessages = new List<EventMessageState>(requiredMessages);
                Dependencies = new HashSet<ExpectedStates>();
                this.debug = debug;
                if (debug)
                {
                    Debug.Log("Expected state: " + SuccessMessage + " -> " + string.Join(",", RequiredMessages));
                }
            }

            public bool IsDependency { get; private set; }

            public bool Completed => MessageSource.MatchResult == EventMessageMatcherState.Success;

            public void AddChild(ExpectedStates nKey)
            {
                if (nKey.IsDependentOn(this))
                {
                    Debug.LogWarning($"Circular dependency in tutorial states between {SuccessMessage} and {nKey.SuccessMessage}.");
                    return;
                }

                nKey.IsDependency = true;
                Dependencies.Add(nKey);
            }

            bool IsDependentOn(ExpectedStates expectedStates)
            {
                foreach (var d in Dependencies)
                {
                    if (d == expectedStates)
                    {
                        return true;
                    }

                    if (d.IsDependentOn(expectedStates))
                    {
                        return true;
                    }
                }

                return false;
            }

            public bool IsProvidedByDependency(BasicEventStreamMessage msg, out ExpectedStates dependency)
            {
                foreach (var d in Dependencies)
                {
                    if (d.SuccessMessage == msg)
                    {
                        dependency = d;
                        return true;
                    }
                }

                dependency = default(ExpectedStates);
                return false;
            }

            public bool ExpectsMessage(BasicEventStreamMessage msg)
            {
                foreach (var r in RequiredMessages)
                {
                    if (r.Message == msg)
                    {
                        return true;
                    }
                }

                return false;
            }
        }
    }
}