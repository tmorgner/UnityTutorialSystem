using System.Collections.Generic;

namespace UnityTutorialSystem.UI.Trees
{
    /// <summary>
    ///   A standard implementation of a tree model for list based tree nodes that maps tree
    ///   operations to their corresponding list operations on the nodes themselves.
    /// </summary>
    /// <typeparam name="TList">A list based data type is required for this class.</typeparam>
    public class ListTreeModel<TList> : TreeModel<TList> where TList : class, IReadOnlyList<TList>
    {
        /// <summary>
        ///   Creates an empty tree model.
        /// </summary>
        public ListTreeModel()
        {
        }

        /// <summary>
        ///   Initializes the tree model with the given root node.
        /// </summary>
        /// <param name="root"></param>
        public ListTreeModel(TList root)
        {
            Root = root;
        }

        /// <inheritdoc />
        public sealed override bool HasRoot => Root != null;

        /// <inheritdoc />
        public sealed override TList Root { get; set; }

        /// <inheritdoc />
        public override bool IsLeaf(TList node)
        {
            return node.Count == 0;
        }

        /// <inheritdoc />
        public override TList GetChildAt(TList parent, int index)
        {
            return parent[index];
        }

        /// <inheritdoc />
        public override int ChildCount(TList parent)
        {
            return parent.Count;
        }
    }
}