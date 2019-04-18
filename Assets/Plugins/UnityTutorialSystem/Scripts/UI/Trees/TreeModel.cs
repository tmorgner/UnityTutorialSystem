using System;
using System.Collections.Generic;

namespace UnityTutorialSystem.UI.Trees
{
    /// <summary>
    ///   An abstract base class for tree model implementations. This class
    ///   provides default implementations for all events.
    /// </summary>
    /// <typeparam name="TNode"></typeparam>
    public abstract class TreeModel<TNode> : ITreeModel<TNode>
    {
        /// <inheritdoc />
        public abstract bool HasRoot { get; }

        /// <inheritdoc />
        public abstract TNode Root { get; set; }

        /// <inheritdoc />
        public abstract bool IsLeaf(TNode node);

        /// <inheritdoc />
        public abstract TNode GetChildAt(TNode parent, int index);

        /// <inheritdoc />
        public abstract int ChildCount(TNode parent);

        /// <inheritdoc />
        public virtual int GetIndexOfChild(TNode parent, TNode child)
        {
            if (IsLeaf(parent))
            {
                return -1;
            }

            var cmp = Comparer<TNode>.Default;
            var count = ChildCount(parent);
            for (var c = 0; c < count; c += 1)
            {
                if (cmp.Compare(child ,GetChildAt(parent, c)) == 0)
                {
                    return c;
                }
            }

            return -1;
        }

        /// <inheritdoc />
        public event EventHandler<TreeModelEventArgs<TNode>> StructureChanged;
        /// <inheritdoc />
        public event EventHandler<TreeModelEventArgs<TNode>> NodesChanged;
        /// <inheritdoc />
        public event EventHandler<TreeModelEventArgs<TNode>> NodesInserted;
        /// <inheritdoc />
        public event EventHandler<TreeModelEventArgs<TNode>> NodesRemoved;

        /// <summary>
        ///   Triggers the StructureChanged event for the root node to indicate that something somewhere inside the
        ///   tree has changed. This usually results in a full rebuild of any UI element or dependent data structure,
        ///   and thus this is the most expensive event that can be used to inform others of changes.
        /// </summary>
        public void FireStructureChanged()
        {
            StructureChanged?.Invoke(this, new TreeModelEventArgs<TNode>(new TreePath<TNode>(), new[] {0}, new[] {Root}));
        }

        /// <summary>
        ///   Indicates that the child node at the given tree path has changed. 
        /// </summary>
        /// <seealso cref="FireNodeChanged"/>
        /// <param name="parent">The path to the parent node</param>
        /// <param name="index">The index of the child node within the parent node</param>
        /// <param name="child">The child node that has changed</param>
        public void FireNodesChanged(TreePath<TNode> parent, int index, TNode child)
        {
            NodesChanged?.Invoke(this, new TreeModelEventArgs<TNode>(parent, index, child));
        }

        /// <summary>
        ///   Indicates that the structure at the given tree path has changed. 
        /// </summary>
        /// <seealso cref="FireStructureChanged(UnityTutorialSystem.UI.Trees.TreePath{TNode},TNode)"/>
        /// <param name="parent">The path to the parent node</param>
        /// <param name="index">The index of the child node within the parent node</param>
        /// <param name="child">The child node that has changed</param>
        public void FireStructureChanged(TreePath<TNode> parent, int index, TNode child)
        {
            StructureChanged?.Invoke(this, new TreeModelEventArgs<TNode>(parent, index, child));
        }

        /// <summary>
        ///   Indicates that a new node <see cref="child"/> has been inserted into the parent at the given
        ///   index positions.
        /// </summary>
        /// <seealso cref="FireStructureChanged(UnityTutorialSystem.UI.Trees.TreePath{TNode},TNode)"/>
        /// <param name="parent">The parent node</param>
        /// <param name="index">The index of the child node within the parent node</param>
        /// <param name="child">The child node that has changed</param>
        public void FireNodeInserted(TreePath<TNode> parent, int index, TNode child)
        {
            if (parent == null)
            {
                throw new ArgumentNullException(nameof(parent));
            }

            NodesInserted?.Invoke(this, new TreeModelEventArgs<TNode>(parent, index, child));
        }

        /// <summary>
        ///   Indicates that an existing node <see cref="child"/> has been removed from the parent at the given
        ///   index positions.
        /// </summary>
        /// <seealso cref="FireStructureChanged(UnityTutorialSystem.UI.Trees.TreePath{TNode},TNode)"/>
        /// <param name="parent">The parent node</param>
        /// <param name="index">The index of the child node within the parent node</param>
        /// <param name="child">The child node that has changed</param>
        public void FireNodeRemoved(TreePath<TNode> parent, int index, TNode child)
        {
            if (parent == null)
            {
                throw new ArgumentNullException(nameof(parent));
            }

            NodesRemoved?.Invoke(this, new TreeModelEventArgs<TNode>(parent, index, child));
        }

        /// <summary>
        ///   Indicates that the given node has changed.
        /// </summary>
        /// <param name="parentPath">The path to the parent node</param>
        /// <param name="child">The child node that has changed</param>
        public void FireNodeChanged(TreePath<TNode> parentPath, TNode child)
        {
            TNode parent;
            parentPath.TryGetLastComponent(out parent);
            var pos = GetIndexOfChild(parent, child);
            FireNodesChanged(parentPath, pos, child);
        }

        /// <summary>
        ///   Indicates that the structure at the given tree path has changed. 
        /// </summary>
        /// <seealso cref="FireStructureChanged(UnityTutorialSystem.UI.Trees.TreePath{TNode},int,TNode)"/>
        /// <param name="parentPath"></param>
        /// <param name="child"></param>
        public void FireStructureChanged(TreePath<TNode> parentPath, TNode child)
        {
            TNode parent;
            parentPath.TryGetLastComponent(out parent);
            var pos = GetIndexOfChild(parent, child);
            FireStructureChanged(parentPath, pos, child);
        }
    }
}
