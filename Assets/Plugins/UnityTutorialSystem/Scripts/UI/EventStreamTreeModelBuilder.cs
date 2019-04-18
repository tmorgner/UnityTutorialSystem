using System.Collections.Generic;
using System.Linq;
using NaughtyAttributes;
using UnityEngine;
using UnityTutorialSystem.Aggregators;
using UnityTutorialSystem.Events;
using UnityTutorialSystem.Helpers;
using UnityTutorialSystem.UI.Trees;

namespace UnityTutorialSystem.UI
{
    /// <summary>
    ///     A tree model builder that creates a tree of
    ///     <see cref="BasicEventStreamMessage" /> objects based on the
    ///     <see cref="EventMessageAggregatorStatePublisher" /> instances given.
    ///     Each <see cref="EventMessageAggregatorStatePublisher" /> is assumed
    ///     to be associated with a single <see cref="EventMessageAggregator" />
    ///     .
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This class attempts to detect hierarchical relationships between
    ///         the
    ///         <see cref="EventMessageAggregatorStatePublisher" /> given by
    ///         trying to match the success message of each
    ///         <see cref="EventMessageAggregatorStatePublisher" /> with a
    ///         produced message of any of the associated
    ///         <see cref="EventMessageAggregator" />instances.
    ///     </para>
    ///     <para>
    ///         This TreeModelBuilder manages a single tree model that can be
    ///         shared between multiple tree view instances. The model will be
    ///         kept up to date for as long as the associated
    ///         <see cref="EventMessageAggregator" />instances are valid. The
    ///         class will delay all work until
    ///         <see cref="Start" />has been called.
    ///     </para>
    /// </remarks>
    public class EventStreamTreeModelBuilder : MonoBehaviour
    {
        readonly List<EventMessageState> buffer;
        readonly ListTreeModel<EventStreamTreeModelData> model;

        [SerializeField] [ReorderableList] List<EventMessageAggregatorStatePublisher> tutorialStateTrackers;
        [SerializeField] bool debug;

        Dictionary<EventMessageAggregatorStatePublisher, EventStreamTreeModelData> nodeMapper;
        EventStreamTreeModelData rootNode;
        bool started;

        /// <summary>
        ///     Default constructor.
        /// </summary>
        public EventStreamTreeModelBuilder()
        {
            buffer = new List<EventMessageState>();
            model = new ListTreeModel<EventStreamTreeModelData>();
        }

        /// <summary>
        ///     The model produced by this builder.
        /// </summary>
        public ITreeModel<EventStreamTreeModelData> Model => model;

        /// <summary>
        ///     Diagnostic method that conditionally logs messages to the debug log.
        /// </summary>
        /// <param name="message"></param>
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
            started = true;

            var data = CollectTreeData();
            PrintTree(data);
            (rootNode, nodeMapper) = CreateTreeNodes(data);
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

            if (started)
            {
                foreach (var d in tutorialStateTrackers)
                {
                    OnTrackerStateChanged(d);
                }
            }
        }

        void OnDisable()
        {
            foreach (var d in tutorialStateTrackers)
            {
                d.StateChanged.RemoveListener(OnTrackerStateChanged);
            }
        }

        /// <summary>
        ///     Called when the state of the associated EventMessageAggregator has changed. This
        ///     updates the
        /// </summary>
        /// <param name="source"></param>
        /// <param name="message"></param>
        void OnTrackerStateChanged(EventMessageAggregatorStatePublisher source, BasicEventStreamMessage message = null)
        {
            if (!started)
            {
                return;
            }

            // Attempt to find the parent EventMessageAggregator that handles the
            // success message of that source. The messages tracked by the source 
            // are represented by child nodes within that parent.
            EventStreamTreeModelData data;
            if (!nodeMapper.TryGetValue(source, out data))
            {
                // only the root itself can be without parent.
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

            // Special case handling: child-nodes of root won't ever receive completion events as
            // there is no aggregator listening for the completed notification. Therefore we
            // have to manually poll that state.
            foreach (var rootChild in rootNode)
            {
                if (rootChild == data)
                {
                    rootChild.Completed = source.MatchResult == EventMessageMatcherState.Success;
                }
            }

            // Nuke the treeview. Right now that is cheaper to write than proper
            // targeted event handling.
            model.FireStructureChanged();
        }

        /// <summary>
        ///     Creates an list of expected states for the tree. This method blindly
        ///     gathers all events produced by each
        ///     <see cref="EventMessageAggregator" /> and - in a second step -
        ///     attempts to build a hierarchy by checking the success message of the
        ///     EventMessageAggratorStatePublisher with all collected messages.
        /// </summary>
        /// <remarks>
        ///     This method has O(N^2) complexity, but hopefully you won't ever have
        ///     so many activities that this actually matters.
        /// </remarks>
        /// <returns>
        ///     The list of aggregator message states that have no parent. All other
        ///     states can be reached from there.
        /// </returns>
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

        /// <summary>
        ///     Creates an artificial root node not backed by any success message. Any actual event aggregators
        ///     that would act as root nodes (because they have no success message on their own) will be flattened
        ///     into this root node.
        /// </summary>
        /// <param name="data">The list of potential root nodes</param>
        /// <returns>The created tree node and a mapping of the created node and all its child nodes and their respective sources.</returns>
        static (EventStreamTreeModelData, Dictionary<EventMessageAggregatorStatePublisher, EventStreamTreeModelData>) CreateTreeNodes(List<ExpectedStates> data)
        {
            var roots = data.Where(s => !s.IsDependency).ToList();
            var nodes = new Dictionary<EventMessageAggregatorStatePublisher, EventStreamTreeModelData>();
            var childStates = new List<EventStreamTreeModelData>();
            foreach (var s in roots)
            {
                var (item, subNodes) = AddTree(s);
                nodes.AddRange(subNodes);
                item.ExpectedNext = false;
                AddGeneratedChildItems(childStates, item);
            }

            var treeModelData = new EventStreamTreeModelData(null, false, true, childStates);
            return (treeModelData, nodes);
        }

        /// <summary>
        ///     Adds all nodes represented by the EventStreamTreeModelData given as item to the list of child states.
        ///     This method flattens all event message aggregators that generate no success message.
        /// </summary>
        /// <param name="childStates">The collected child nodes</param>
        /// <param name="item">The item to process</param>
        static void AddGeneratedChildItems(List<EventStreamTreeModelData> childStates, EventStreamTreeModelData item)
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

        /// <summary>
        ///     Generates a sub-tree for the given ExpectedStates instance that
        ///     replicates the structure of all known dependencies for that state.
        /// </summary>
        /// <param name="state">The state that is processed.</param>
        /// <returns>
        ///     The created tree node and a mapping of the created node and all its
        ///     child nodes and their respective sources.
        /// </returns>
        static (EventStreamTreeModelData, Dictionary<EventMessageAggregatorStatePublisher, EventStreamTreeModelData>) AddTree(ExpectedStates state)
        {
            var childStates = new List<EventStreamTreeModelData>();
            var nodeMapper = new Dictionary<EventMessageAggregatorStatePublisher, EventStreamTreeModelData>();
            foreach (var s in state.RequiredMessages)
            {
                ExpectedStates dependency;
                if (state.IsProvidedByDependency(s.Message, out dependency))
                {
                    var (item, subNodes) = AddTree(dependency);
                    nodeMapper.AddRange(subNodes);
                    item.ExpectedNext = s.ExpectedNext;
                    childStates.Add(item);
                }
                else
                {
                    var message = s.Message;
                    childStates.Add(new EventStreamTreeModelData(message, s.Completed, s.ExpectedNext));
                }
            }

            var stateData = new EventStreamTreeModelData(state.SuccessMessage, state.Completed, false, childStates);
            nodeMapper[state.MessageSource] = stateData;
            return (stateData, nodeMapper);
        }

        /// <summary>
        ///    A debug helper function that prints the resulting tree into the log.
        /// </summary>
        /// <param name="states"></param>
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

        /// <summary>
        ///   An internal helper data structure used during the initial tree construction during the Start event.
        /// </summary>
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

                dependency = default;
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
