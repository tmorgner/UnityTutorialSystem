using System;
using TutorialSystem.Events;

namespace TutorialSystem.UI
{
    public class EventStreamState : IEquatable<EventStreamState>
    {
        public EventStreamState(BasicEventStreamMessage sourceMessage, bool completed)
        {
            if (sourceMessage == null)
            {
                throw new ArgumentNullException(nameof(sourceMessage));
            }

            SourceMessage = sourceMessage;
            Completed = completed;
        }

        public BasicEventStreamMessage SourceMessage { get; }

        public bool Completed { get; }

        public bool Equals(EventStreamState other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return Equals(SourceMessage, other.SourceMessage)
                   && (Completed == other.Completed);
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

            return Equals((EventStreamState) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((SourceMessage != null ? SourceMessage.GetHashCode() : 0) * 397) ^ Completed.GetHashCode();
            }
        }

        public static bool operator ==(EventStreamState left, EventStreamState right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(EventStreamState left, EventStreamState right)
        {
            return !Equals(left, right);
        }
    }
}