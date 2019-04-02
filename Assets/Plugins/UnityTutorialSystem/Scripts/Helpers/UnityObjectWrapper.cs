using System;
using JetBrains.Annotations;

namespace UnityTutorialSystem.Helpers
{
    public struct UnityObjectWrapper<T> where T: UnityEngine.Object
    {
        [NotNull] public readonly UnityEngine.Object Value;

        public UnityObjectWrapper(UnityEngine.Object value)
        {
            // ReSharper disable once JoinNullCheckWithUsage
            if (ReferenceEquals(value, null))
            {
                throw new ArgumentNullException();
            }
            Value = value;
        }

        public bool Equals(UnityObjectWrapper<T> other)
        {
            return Equals(Value.GetInstanceID(), other.Value.GetInstanceID());
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            return obj is UnityObjectWrapper<T> other && Equals(other);
        }

        public override int GetHashCode()
        {
            return (Value != null ? Value.GetHashCode() : 0);
        }
        
        public static implicit operator UnityEngine.Object (UnityObjectWrapper<T> w)
        {
            return w.Value;
        }  
    }
}