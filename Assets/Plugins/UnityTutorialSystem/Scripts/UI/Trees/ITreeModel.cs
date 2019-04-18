using System;

namespace UnityTutorialSystem.UI.Trees
{
    /// <summary>
    ///  A Tree model <see langword="interface"/>. This model abstracts away the
    ///  underlying data structure by forming a facade with all required
    ///  operations over the data nodes. This <see langword="interface"/> is
    ///  roughly modelled after the excellent design found in Java Swing.
    /// </summary>
    /// <typeparam name="TNode">The underlying node data type</typeparam>
    public interface ITreeModel<TNode>
    {
        /// <summary>
        ///  Checks whether the model has a tree node.
        /// </summary>
        bool HasRoot { get; }

        /// <summary>
        ///  If the model has a root tree node, return it. 
        /// </summary>
        /// <seealso cref="HasRoot"/>
        TNode Root { get; }

        /// <summary>
        /// <para>
        /// Tests whether the given <paramref name="node"/> is a leaf node. A
        /// leaf <paramref name="node"/> is any <paramref name="node"/> that
        /// cannot possibly have children. For example, a file is a leaf
        /// <paramref name="node"/> in a file system tree, but an empty
        /// directory is not. Even though an empty directory currently has no
        /// child nodes, there are circumstances that directory can have child
        /// nodes in the future.
        /// </para>
        /// <para>
        /// Implementation note: This test is currently only used for
        /// presentation purposes.
        /// </para>
        /// </summary>
        /// <param name="node">The node to test</param>
        /// <returns>
        /// <see langword="true" /> if the <paramref name="node"/> cannot have
        /// children, <see langword="false"/> otherwise.
        /// </returns>
        bool IsLeaf(TNode node);

        /// <summary>
        ///   Return the child node at the given index. 
        /// </summary>
        /// <param name="parent">the parent node</param>
        /// <param name="index">the index of the child node in the parent node</param>
        /// <returns>The node at the given index, never null.</returns>
        /// <exception cref="IndexOutOfRangeException">if the node given does not have a child at that position.</exception>
        /// <seealso cref="ChildCount"/>
        /// <seealso cref="GetIndexOfChild"/>
        TNode GetChildAt(TNode parent, int index);

        /// <summary>
        /// Returns the number of child nodes this <paramref name="parent"/>
        /// node contains.
        /// </summary>
        /// <param name="parent">The parent node</param>
        /// <returns>
        /// a positive integer
        /// </returns>
        int ChildCount(TNode parent);

        /// <summary>
        /// Returns the index of the given <paramref name="child"/> node in the
        /// <paramref name="parent"/> node or -1 if this potential
        /// <paramref name="child"/> node is not a <paramref name="child"/> of
        /// the parent.
        /// </summary>
        /// <param name="parent">The parent node</param>
        /// <param name="child">The child node</param>
        /// <returns>
        /// the positive index of the <paramref name="child"/> node in the
        /// parent, or -1 if the node given as <paramref name="child"/> is not a
        /// <paramref name="child"/> of that parent.
        /// </returns>
        /// <seealso cref="ChildCount"/>
        /// <seealso cref="GetChildAt"/>
        int GetIndexOfChild(TNode parent, TNode child);

        /// <summary>
        ///   Informs all listeners that the structure of the tree at the given tree path has
        ///   changed. A structural change indicates that nodes have been added or removed from
        ///   the tree. If an empty tree path is given it indicates that the root node itself has changed.
        /// </summary>
        event EventHandler<TreeModelEventArgs<TNode>> StructureChanged;

        /// <summary>
        ///   Informs all listeners that the data inside a tree node has changed. Firing this event
        ///   does not indicate that nodes have been added or removed, it merely means that one or
        ///   more properties of the TNode object representing the node have changed in some way.
        /// </summary>
        event EventHandler<TreeModelEventArgs<TNode>> NodesChanged;

        /// <summary>
        ///  A specialised structural change event that indicates that new nodes have been added
        ///  at the given location.
        /// </summary>
        event EventHandler<TreeModelEventArgs<TNode>> NodesInserted;

        /// <summary>
        ///  A specialised structural change event that indicates that old nodes have been removed
        ///  at the given location.
        /// </summary>
        event EventHandler<TreeModelEventArgs<TNode>> NodesRemoved;
    }
}
