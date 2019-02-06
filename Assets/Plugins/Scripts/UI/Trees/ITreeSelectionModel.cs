using System;
using System.Collections.Generic;

namespace TutorialSystem.UI.Trees
{
    public interface ITreeSelectionModel<TNode> : IReadOnlyList<TreePath<TNode>>
    {
        SelectionMode SelectionMode { get; set; }
        TreePath<TNode> SelectionPath { get; set; }

        Func<TreePath<TNode>, int> RowMapper { get; set; }
        void SetPaths(TreePath<TNode>[] p);
        void Add(TreePath<TNode> p);
        void Remove(TreePath<TNode> p);
        bool Contains(TreePath<TNode> p);
        event EventHandler<EventArgs> SelectionChanged;
    }
}