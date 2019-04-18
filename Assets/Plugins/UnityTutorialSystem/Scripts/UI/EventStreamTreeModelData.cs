using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using UnityTutorialSystem.Events;

namespace UnityTutorialSystem.UI
{
    /// <summary>
    ///     A TreeModel data node that represents a
    ///     <see cref="BasicEventStreamMessage" /> and all messages that contribute
    ///     to state changes of that message.
    /// </summary>
    public class EventStreamTreeModelData : IReadOnlyList<EventStreamTreeModelData>,
                                            IEquatable<EventStreamTreeModelData>
    {
        readonly List<EventStreamTreeModelData> subTasks;

        /// <summary>
        ///     Creates a new EventStreamTreeModelData representing the given source
        ///     message and all its contributing sub messages.
        /// </summary>
        /// <param name="sourceMessage">
        ///     The source message represented by this tree node.
        /// </param>
        /// <param name="completed">
        ///     A flag indicating whether the message has been seen.
        /// </param>
        /// <param name="expectedNext">
        ///     A flag indicating whether this message will be seen soon.
        /// </param>
        /// <param name="data">
        ///     The messages contributing to the state of the source message.
        /// </param>
        public EventStreamTreeModelData(BasicEventStreamMessage sourceMessage,
                                        bool completed,
                                        bool expectedNext,
                                        IReadOnlyList<EventStreamTreeModelData> data)
        {
            SourceMessage = sourceMessage;
            Completed = completed;
            ExpectedNext = expectedNext;
            subTasks = new List<EventStreamTreeModelData>(data);
        }

        /// <summary>
        ///     A lazy constructor implementation to automatically wrap single
        ///     argument calls of the <paramref name="data" /> collection into a
        ///     <see cref="System.Collections.Generic.IReadOnlyList`1" /> .
        /// </summary>
        /// <param name="sourceMessage">
        ///     The source message represented by this tree node.
        /// </param>
        /// <param name="completed">
        ///     A flag indicating whether the message has been seen.
        /// </param>
        /// <param name="expectedNext">
        ///     A flag indicating whether this message will be seen soon.
        /// </param>
        /// <param name="data">
        ///     The messages contributing to the state of the source message.
        /// </param>
        public EventStreamTreeModelData(BasicEventStreamMessage sourceMessage,
                                        bool completed,
                                        bool expectedNext,
                                        params EventStreamTreeModelData[] data) :
            this(sourceMessage, completed, expectedNext, (IReadOnlyList<EventStreamTreeModelData>) data)
        {
        }

        /// <summary>
        ///   The source message represented by this node.
        /// </summary>
        public BasicEventStreamMessage SourceMessage { get; }

        /// <summary>
        ///  A flag indicating that the message has been received by the EventMessageAggregator.
        /// </summary>
        public bool Completed { get; set; }

        /// <summary>
        ///   A flag indicating that the system expects this message to be fired next.
        /// </summary>
        public bool ExpectedNext { get; set; }

        /// <summary>
        ///    Compares this tree node for structural equality. Required by the ListTreeModel implementation
        ///    and its GetIndexOf method.
        /// </summary>
        /// <param name="other">the other node to compare</param>
        /// <returns>true if both nodes represent the same message and submessage set.</returns>
        public bool Equals(EventStreamTreeModelData other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            if (!Equals(SourceMessage, other.SourceMessage))
            {
                return false;
            }

            if (Completed != other.Completed)
            {
                return false;
            }

            return subTasks.SequenceEqual(other.subTasks);
        }

        /// <summary>
        ///   Indexer access, part of the IList interface.
        /// </summary>
        /// <param name="idx">the index.</param>
        /// <returns>the element at the index given</returns>
        /// <exception cref="IndexOutOfRangeException">If the index is invalid.</exception>
        public EventStreamTreeModelData this[int idx] => subTasks[idx];

        /// <summary>
        ///  Return the number of sub-elements in this node.
        /// </summary>
        public int Count => subTasks.Count;

        IEnumerator<EventStreamTreeModelData> IEnumerable<EventStreamTreeModelData>.GetEnumerator()
        {
            return GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        ///   Returns an non-allocating memory efficient list enumerator.
        /// </summary>
        /// <returns>An efficient enumerator.</returns>
        public List<EventStreamTreeModelData>.Enumerator GetEnumerator()
        {
            return subTasks.GetEnumerator();
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != GetType())
            {
                return false;
            }

            return Equals((EventStreamTreeModelData) obj);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = 19;
                foreach (var s in subTasks)
                {
                    hashCode = (hashCode * 397) ^ (s != null ? s.GetHashCode() : 0);
                }

                hashCode = (hashCode * 397) ^ (SourceMessage != null ? SourceMessage.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ Completed.GetHashCode();
                return hashCode;
            }
        }

        /// <inheritdoc />
        [NotNull]
        public override string ToString()
        {
            if (SourceMessage == null)
            {
                return $"({nameof(SourceMessage)}: <NONE>, {nameof(Completed)}: {Completed})";
            }

            return $"({nameof(SourceMessage)}: {SourceMessage.name}, {nameof(Completed)}: {Completed})";
        }

        /// <summary>
        ///    A standard equality operator implementation.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns>true if both objects are structurally equal, false otherwise.</returns>
        public static bool operator ==(EventStreamTreeModelData left, EventStreamTreeModelData right)
        {
            return Equals(left, right);
        }

        /// <summary>
        ///    A standard inequality operator implementation.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns>true if both objects are structurally non-equal, false otherwise.</returns>
        public static bool operator !=(EventStreamTreeModelData left, EventStreamTreeModelData right)
        {
            return !Equals(left, right);
        }

        /// <summary>
        ///   Prints the tree as indented string, one element per line. Useful for debugging.
        /// </summary>
        /// <returns></returns>
        public string AsTextTree()
        {
            var b = new StringBuilder();
            AsTreeText(b, this, 0);
            return b.ToString();
        }

        static void AsTreeText(StringBuilder b, EventStreamTreeModelData data, int indent)
        {
            b.Append("".PadLeft(indent * 4));
            b.Append(data);
            b.Append("\n");
            foreach (var c in data)
            {
                AsTreeText(b, c, indent + 1);
            }
        }
    }
}
