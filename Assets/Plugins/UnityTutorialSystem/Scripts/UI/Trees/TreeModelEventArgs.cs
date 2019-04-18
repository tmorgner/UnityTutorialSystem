using System;
using JetBrains.Annotations;

namespace UnityTutorialSystem.UI.Trees
{
    /// <summary>
    ///   An event argument class for tree modification events.
    /// </summary>
    /// <typeparam name="TNode"></typeparam>
    public class TreeModelEventArgs<TNode> : EventArgs
    {
        /// <summary>
        ///   Constructs an event argument with the given path as context path and
        ///   no information about changed child nodes.
        /// </summary>
        /// <param name="path">The context path</param>
        /// <exception cref="ArgumentNullException">If the given path is null</exception>
        public TreeModelEventArgs([NotNull] TreePath<TNode> path)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            Path = path;
            Children = new TNode[0];
            ChildIndices = new int[0];
        }

        /// <summary>
        ///   Constructs an event argument for tree modification events caused by a
        ///   single tree node. Use this constructor for insertion and deletion events.
        /// </summary>
        /// <param name="path">The context path.</param>
        /// <param name="childIndex">The index of the child within the parent.</param>
        /// <param name="child">The child node.</param>
        /// <exception cref="ArgumentNullException">if any of the parameters is null</exception>
        public TreeModelEventArgs([NotNull] TreePath<TNode> path, int childIndex, [NotNull] TNode child) : this(path, new[] {childIndex}, new[] {child})
        {
        }

        /// <summary>
        ///   Constructs an event argument for tree modification events caused by a
        ///   multiple tree nodes within the same parent.
        /// </summary>
        /// <param name="path">The context path.</param>
        /// <param name="childIndex">The indices of the child within the parent.</param>
        /// <param name="child">The child nodes.</param>
        /// <exception cref="ArgumentNullException">if any of the parameters is null</exception>
        public TreeModelEventArgs([NotNull] TreePath<TNode> path, [NotNull] int[] childIndices, [NotNull] TNode[] children)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            if (childIndices == null)
            {
                throw new ArgumentNullException(nameof(childIndices));
            }

            if (children == null)
            {
                throw new ArgumentNullException(nameof(children));
            }

            ChildIndices = childIndices;
            Children = children;
            Path = path;
        }

        /// <summary>
        ///   The indices of the position of the child nodes in the node described by the context path.
        /// </summary>
        [NotNull] public int[] ChildIndices { get; }
        
        /// <summary>
        ///  The children.
        /// </summary>
        [NotNull] public TNode[] Children { get; }

        /// <summary>
        ///  The context path.
        /// </summary>
        [NotNull] public TreePath<TNode> Path { get; }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{nameof(Path)}: {Path}, {nameof(ChildIndices)}: {ChildIndices}, {nameof(Children)}: {Children}";
        }
    }
}