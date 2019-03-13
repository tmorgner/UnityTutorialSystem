using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace UnityTutorialSystem.UI.Trees
{
    public static class TreePath
    {
        public static TreePath<TNode> Empty<TNode>()
        {
            return new TreePath<TNode>();
        }

        public static TreePath<TNode> From<TNode>(TNode n)
        {
            return new TreePath<TNode>(n);
        }

        public static TreePath<TNode> From<TNode>(params TNode[] n)
        {
            return new TreePath<TNode>(n);
        }
    }

    public class TreePath<TNode> : IReadOnlyList<TNode>, IEquatable<TreePath<TNode>>
    {
        readonly TNode[] backend;

        public TreePath()
        {
            backend = new TNode[0];
            Count = 0;
        }

        public TreePath(TNode component)
        {
            backend = new[] {component};
            Count = backend.Length;
        }

        public TreePath(params TNode[] component)
        {
            backend = (TNode[]) component.Clone();
            Count = backend.Length;
        }

        TreePath(TNode[] component, int count)
        {
            // not need for defensive cloning, as we trust the implementation.
            backend = component;
            Count = count;
        }

        public TreePath(TreePath<TNode> parent, TNode component)
        {
            var tmpArray = new TNode[parent.Count + 1];
            for (var index = 0; index < parent.Count; index++)
            {
                tmpArray[index] = parent[index];
            }

            tmpArray[parent.Count] = component;
            backend = tmpArray;
            Count = backend.Length;
        }

        public TreePath<TNode> Parent
        {
            get
            {
                if (Count == 0)
                {
                    throw new ArgumentException();
                }

                return new TreePath<TNode>(backend, Count - 1);
            }
        }

        public bool Equals(TreePath<TNode> other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            if (Count != other.Count)
            {
                return false;
            }

            if (ReferenceEquals(backend, other.backend))
            {
                return true;
            }

            return this.SequenceEqual(other);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        IEnumerator<TNode> IEnumerable<TNode>.GetEnumerator()
        {
            return GetEnumerator();
        }

        public int Count { get; }

        public TNode this[int index] => backend[index];

        public bool TryGetLastComponent(out TNode c)
        {
            if (Count == 0)
            {
                c = default(TNode);
                return false;
            }

            c = backend[Count - 1];
            return true;
        }

        public Enumerator GetEnumerator()
        {
            return new Enumerator(this);
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

            return Equals((TreePath<TNode>) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                // this isn't overly quick but will do for now.
                var hashCode = Count;
                for (var i = 0; i < Count; i++)
                {
                    object n = backend[i];
                    hashCode = (hashCode * 397) ^ (n?.GetHashCode() ?? 0);
                }

                return hashCode;
            }
        }

        public override string ToString()
        {
            var payload = string.Join("', '", backend);
            return $"['{payload}']";
        }

        public static bool operator ==(TreePath<TNode> left, TreePath<TNode> right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(TreePath<TNode> left, TreePath<TNode> right)
        {
            return !Equals(left, right);
        }

        public struct Enumerator : IEnumerator<TNode>
        {
            readonly TreePath<TNode> contents;

            int index;
            TNode current;

            internal Enumerator(TreePath<TNode> widget) : this()
            {
                contents = widget;
                index = -1;
                current = default(TNode);
            }

            public void Dispose()
            {
            }

            public bool MoveNext()
            {
                if (index + 1 < contents.Count)
                {
                    index += 1;
                    current = contents[index];
                    return true;
                }

                current = default(TNode);
                return false;
            }

            public void Reset()
            {
                index = -1;
                current = default(TNode);
            }

            object IEnumerator.Current => Current;

            public TNode Current
            {
                get
                {
                    if ((index < 0) || (index >= contents.Count))
                    {
                        throw new InvalidOperationException();
                    }

                    return current;
                }
            }
        }
    }
}