using System;

namespace UnityTutorialSystem.UI.Trees
{
    public interface ITreeModel<TNode>
    {
        bool HasRoot { get; }
        TNode Root { get; }
        bool IsLeaf(TNode node);
        TNode GetChildAt(TNode parent, int index);
        int ChildCount(TNode parent);
        int GetIndexOfChild(TNode parent, TNode child);

        event EventHandler<TreeModelEventArgs<TNode>> StructureChanged;
        event EventHandler<TreeModelEventArgs<TNode>> NodesChanged;
        event EventHandler<TreeModelEventArgs<TNode>> NodesInserted;
        event EventHandler<TreeModelEventArgs<TNode>> NodesRemoved;
    }
}