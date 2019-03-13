using System;

namespace UnityTutorialSystem.UI.Trees
{
    public abstract class TreeModel<TNode> : ITreeModel<TNode>
    {
        public abstract bool HasRoot { get; }

        public abstract TNode Root { get; set; }

        public abstract bool IsLeaf(TNode node);

        public abstract TNode GetChildAt(TNode parent, int index);

        public abstract int ChildCount(TNode parent);

        public abstract int GetIndexOfChild(TNode parent, TNode child);

        public event EventHandler<TreeModelEventArgs<TNode>> StructureChanged;
        public event EventHandler<TreeModelEventArgs<TNode>> NodesChanged;
        public event EventHandler<TreeModelEventArgs<TNode>> NodesInserted;
        public event EventHandler<TreeModelEventArgs<TNode>> NodesRemoved;

        public void FireStructureChanged()
        {
            StructureChanged?.Invoke(this, new TreeModelEventArgs<TNode>(new TreePath<TNode>(), new[] {0}, new[] {Root}));
        }

        public void FireNodesChanged(TreePath<TNode> parent, int index, TNode child)
        {
            NodesChanged?.Invoke(this, new TreeModelEventArgs<TNode>(parent, index, child));
        }

        public void FireStructureChanged(TreePath<TNode> parent, int index, TNode child)
        {
            StructureChanged?.Invoke(this, new TreeModelEventArgs<TNode>(parent, index, child));
        }

        public void FireNodeInserted(TreePath<TNode> parent, int index, TNode child)
        {
            if (parent == null)
            {
                throw new ArgumentNullException(nameof(parent));
            }

            NodesInserted?.Invoke(this, new TreeModelEventArgs<TNode>(parent, index, child));
        }

        public void FireNodeRemoved(TreePath<TNode> parent, int index, TNode child)
        {
            if (parent == null)
            {
                throw new ArgumentNullException(nameof(parent));
            }

            NodesRemoved?.Invoke(this, new TreeModelEventArgs<TNode>(parent, index, child));
        }

        public void FireNodeChanged(TreePath<TNode> parentPath, TNode child)
        {
            TNode parent;
            parentPath.TryGetLastComponent(out parent);
            var pos = GetIndexOfChild(parent, child);
            FireNodesChanged(parentPath, pos, child);
        }

        public void FireStructureChanged(TreePath<TNode> parentPath, TNode child)
        {
            TNode parent;
            parentPath.TryGetLastComponent(out parent);
            var pos = GetIndexOfChild(parent, child);
            FireStructureChanged(parentPath, pos, child);
        }
    }
}