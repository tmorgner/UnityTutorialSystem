using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace TutorialSystem.UI.Trees
{
    [Serializable]
    public class DefaultTreeNode : IReadOnlyList<DefaultTreeNode>, IEquatable<DefaultTreeNode>
    {
        // ReSharper disable once CollectionNeverUpdated.Local
        static readonly List<DefaultTreeNode> emptyNodes = new List<DefaultTreeNode>();

        List<DefaultTreeNode> backend;
        object value;
        TreePath<DefaultTreeNode> path;
        DefaultTreeNode parent;

        public DefaultTreeNode(bool asLeaf = false, object value = null)
        {
            IsLeaf = asLeaf;
            this.value = value;
        }

        public TreePath<DefaultTreeNode> Path
        {
            get
            {
                if (path != null)
                {
                    return path;
                }

                if (Parent == null)
                {
                    path = new TreePath<DefaultTreeNode>(this);
                }
                else
                {
                    path = new TreePath<DefaultTreeNode>(Parent.Path, this);
                }

                return path;
            }
        }

        public object Value
        {
            get { return value; }
            set { this.value = value; }
        }

        public DefaultTreeNode Parent
        {
            get { return parent; }
            private set
            {
                parent = value;
                path = null;
            }
        }

        public bool IsLeaf { get; }

        public bool Equals(DefaultTreeNode other)
        {
            return ReferenceEquals(this, other);
        }

        public DefaultTreeNode this[int index]
        {
            get
            {
                if (backend == null)
                {
                    throw new IndexOutOfRangeException();
                }

                return backend[index];
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        IEnumerator<DefaultTreeNode> IEnumerable<DefaultTreeNode>.GetEnumerator()
        {
            return GetEnumerator();
        }

        public int Count => backend?.Count ?? 0;

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

            return Equals((DefaultTreeNode) obj);
        }

        public override int GetHashCode()
        {
            return RuntimeHelpers.GetHashCode(this);
        }

        public static bool operator ==(DefaultTreeNode left, DefaultTreeNode right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(DefaultTreeNode left, DefaultTreeNode right)
        {
            return !Equals(left, right);
        }

        public void Add(DefaultTreeNode child)
        {
            var oldParent = child.Parent;
            if ((oldParent != null) && !oldParent.Remove(child))
            {
                throw new ArgumentException();
            }

            if (backend == null)
            {
                backend = new List<DefaultTreeNode>();
            }

            backend.Add(child);
            child.Parent = this;
        }

        public override string ToString()
        {
            return $"{Value}";
        }

        public List<DefaultTreeNode>.Enumerator GetEnumerator()
        {
            return backend?.GetEnumerator() ?? emptyNodes.GetEnumerator();
        }

        public bool Remove(DefaultTreeNode child)
        {
            if (backend == null)
            {
                return false;
            }

            if (backend.Remove(child))
            {
                child.Parent = null;
                return true;
            }

            return false;
        }

        public int IndexOf(DefaultTreeNode child)
        {
            if (backend == null)
            {
                return -1;
            }

            return backend.IndexOf(child);
        }

        public void InsertAt(DefaultTreeNode child, int index)
        {
            var oldParent = child.Parent;
            if ((oldParent != null) && !oldParent.Remove(child))
            {
                throw new ArgumentException();
            }

            if (backend == null)
            {
                backend = new List<DefaultTreeNode>();
            }

            backend.Insert(index, child);
            child.Parent = this;
        }
    }
}