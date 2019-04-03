using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityTutorialSystem.Events;

namespace UnityTutorialSystem.UI
{
    public class EventStreamTreeModelData : IReadOnlyList<EventStreamTreeModelData>,
                                          IEquatable<EventStreamTreeModelData>
    {
        readonly List<EventStreamTreeModelData> subTasks;

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

        public EventStreamTreeModelData(BasicEventStreamMessage sourceMessage,
                                      bool completed,
                                      bool expectedNext,
                                      params EventStreamTreeModelData[] data) :
            this(sourceMessage, completed, expectedNext, (IReadOnlyList<EventStreamTreeModelData>)data)
        {
        }

        public BasicEventStreamMessage SourceMessage { get; }

        public bool Completed { get; set; }
        public bool ExpectedNext { get; set; }

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

        public EventStreamTreeModelData this[int idx] => subTasks[idx];
        public int Count => subTasks.Count;

        IEnumerator<EventStreamTreeModelData> IEnumerable<EventStreamTreeModelData>.GetEnumerator()
        {
            return GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public List<EventStreamTreeModelData>.Enumerator GetEnumerator()
        {
            return subTasks.GetEnumerator();
        }

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

            return Equals((EventStreamTreeModelData)obj);
        }

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

        public override string ToString()
        {
            if (SourceMessage == null)
            {
                return $"({nameof(SourceMessage)}: <NONE>, {nameof(Completed)}: {Completed})";
            }

            return $"({nameof(SourceMessage)}: {SourceMessage.name}, {nameof(Completed)}: {Completed})";
        }

        public static bool operator ==(EventStreamTreeModelData left, EventStreamTreeModelData right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(EventStreamTreeModelData left, EventStreamTreeModelData right)
        {
            return !Equals(left, right);
        }

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