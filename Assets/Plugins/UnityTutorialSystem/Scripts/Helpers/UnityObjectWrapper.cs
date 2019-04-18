using System;
using JetBrains.Annotations;

namespace UnityTutorialSystem.Helpers
{
    /// <summary>
    ///   An object wrapper for UnityObjects that compares objects based on
    ///   instance-id instead of low level Equals calls. The <see cref="UnityEngine.Object.Equals(object)"/>
    ///   implementation calls into the native code for no sane reason, which
    ///   can be performance critical. 
    /// </summary>
    /// <typeparam name="T"></typeparam>
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
            return Value.GetHashCode();
        }

        /// <summary>
        ///   Implicitly wraps the given UnityObject into an ObjectWrapper.
        /// </summary>
        /// <param name="w">the object to wrap</param>
        /// <returns>The UnityObjectWrapper for the given Unity-Object.</returns>
        public static implicit operator UnityObjectWrapper<T>(T w)
        {
            return new UnityObjectWrapper<T>(w);
        } 

        /// <summary>
        ///   Implicitly unwraps the wrapper into the contained Unity object.
        /// </summary>
        /// <param name="w">An object wrapper containing a non-null Unity object</param>
        /// <returns>the unwrapped object</returns>
        public static implicit operator UnityEngine.Object (UnityObjectWrapper<T> w)
        {
            return w.Value;
        }  
    }
}