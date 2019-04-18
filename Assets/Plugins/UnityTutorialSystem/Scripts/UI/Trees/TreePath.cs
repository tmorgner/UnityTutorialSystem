using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace UnityTutorialSystem.UI.Trees
{
    /// <summary>
    ///   Factory methods for the TreePath data type.
    /// </summary>
    public static class TreePath
    {
        public static TreePath<TNode> Empty<TNode>()
        {
            return new TreePath<TNode>();
        }
        
        /// <summary>
        ///  Creates a new, single level tree path from the given node.
        /// </summary>
        /// <param name="n">the node data</param>
        /// <typeparam name="TNode">The TreeNode type</typeparam>
        /// <returns>A treepath containing the given node as only element.</returns>
        public static TreePath<TNode> From<TNode>(TNode n)
        {
            return new TreePath<TNode>(n);
        }

        /// <summary>
        ///   Creates a TreePath from the ordered set ot nodes.
        /// </summary>
        /// <param name="n">The set of nodes making up the path.</param>
        /// <typeparam name="TNode">The TreeNode type</typeparam>
        /// <returns>the created TreePath</returns>
        public static TreePath<TNode> From<TNode>(params TNode[] n)
        {
            var a = (TNode[]) n.Clone();
            return new TreePath<TNode>(a, n.Length);
        }
    }

    /// <summary>
    ///   A TreePath represents a hierarchical address in a tree data structure.
    ///   Each element represents a node in the tree at the given level so that
    ///   the element at N+1 is a child of the tree node found at level N.
    /// </summary>
    /// <typeparam name="TNode">The TreeNode content type</typeparam>
    public class TreePath<TNode> : IReadOnlyList<TNode>, IEquatable<TreePath<TNode>>
    {
        readonly TNode[] backend;

        internal TreePath()
        {
            backend = new TNode[0];
            Count = 0;
        }

        internal TreePath(TNode component)
        {
            backend = new[] {component};
            Count = backend.Length;
        }

        internal TreePath(TNode[] component, int count)
        {
            // not need for defensive cloning, as we trust the implementation.
            backend = component;
            Count = count;
        }

        /// <summary>
        ///   Appends the given component to this tree-path creating a new path from it.
        /// </summary>
        /// <param name="component">The new child component.</param>
        /// <returns>The new tree path</returns>
        public TreePath<TNode> Append(TNode component)
        {
            var tmpArray = new TNode[Count + 1];
            for (var index = 0; index < Count; index++)
            {
                tmpArray[index] = this[index];
            }

            tmpArray[Count] = component;
            return new TreePath<TNode>(backend, Count);
        }

        /// <summary>
        ///   Returns the parent path of this path, which is the path with
        ///   the last element removed. This method will fail with an
        ///   ArgumentException if the path is already empty.
        /// </summary>
        /// <exception cref="ArgumentException">if the Path is empty</exception>
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

        /// <inheritdoc />
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

        /// <summary>
        ///  Returns the number of segments in this path. Returns zero for the empty path. 
        /// </summary>
        public int Count { get; }

        /// <summary>
        ///   Accesses the path segment at the given index.
        /// </summary>
        /// <param name="index">a zero based index.</param>
        public TNode this[int index] => backend[index];

        /// <summary>
        ///   Attempts to retrieve the last component of the path. 
        /// </summary>
        /// <param name="c">The last component</param>
        /// <returns>true if the path is non empty, false otherwise.</returns>
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

        /// <summary>
        ///   Returns an struct-based enumerator to support more efficient
        ///   foreach loops.
        /// </summary>
        /// <returns></returns>
        public Enumerator GetEnumerator()
        {
            return new Enumerator(this);
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

            return Equals((TreePath<TNode>) obj);
        }

        /// <inheritdoc />
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

        /// <inheritdoc />
        [NotNull]
        public override string ToString()
        {
            var payload = string.Join("', '", backend);
            return $"['{payload}']";
        }

        /// <summary>
        ///   Compares the two tree paths for structural equality using the Equals operator.
        /// </summary>
        /// <param name="left">The left path</param>
        /// <param name="right">The right path</param>
        /// <returns>true if both paths are structurally equal, false otherwise.</returns>
        public static bool operator ==(TreePath<TNode> left, TreePath<TNode> right)
        {
            return Equals(left, right);
        }

        /// <summary>
        ///   Compares the two tree paths for structural inequality using the Equals operator.
        /// </summary>
        /// <param name="left">The left path</param>
        /// <param name="right">The right path</param>
        /// <returns>true if both paths are structurally unequal, false otherwise.</returns>
        public static bool operator !=(TreePath<TNode> left, TreePath<TNode> right)
        {
            return !Equals(left, right);
        }

        /// <summary>
        ///  A enumerator that does not cause heap allocations when used correctly.
        /// </summary>
        public struct Enumerator : IEnumerator<TNode>
        {
            readonly TreePath<TNode> contents;

            int index;
            TNode current;

            /// <inheritdoc />
            internal Enumerator(TreePath<TNode> widget) : this()
            {
                contents = widget;
                index = -1;
                current = default(TNode);
            }

            /// <inheritdoc />
            public void Dispose()
            {
            }

            /// <inheritdoc />
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

            /// <inheritdoc />
            public void Reset()
            {
                index = -1;
                current = default(TNode);
            }

            object IEnumerator.Current => Current;

            /// <inheritdoc />
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