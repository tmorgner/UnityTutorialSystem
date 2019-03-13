using System;

namespace UnityTutorialSystem.UI.Trees
{
    public class TreeModelEventArgs<TNode> : EventArgs
    {
        public TreeModelEventArgs(TreePath<TNode> path)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            Path = path;
            Children = new TNode[0];
            ChildIndices = new int[0];
        }

        public TreeModelEventArgs(TreePath<TNode> path, int childIndex, TNode child) : this(path, new[] {childIndex}, new[] {child})
        {
        }

        public TreeModelEventArgs(TreePath<TNode> path, int[] childIndices, TNode[] children)
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

        public int[] ChildIndices { get; }
        public TNode[] Children { get; }
        public TreePath<TNode> Path { get; }

        public override string ToString()
        {
            return $"{nameof(Path)}: {Path}, {nameof(ChildIndices)}: {ChildIndices}, {nameof(Children)}: {Children}";
        }
    }
}