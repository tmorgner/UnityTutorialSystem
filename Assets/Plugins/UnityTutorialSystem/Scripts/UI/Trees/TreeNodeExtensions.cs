using System.Collections.Generic;

namespace TutorialSystem.UI.Trees
{
    public static class TreeNodeExtensions
    {
        public static int InsertSorted(this DefaultTreeNode parent, DefaultTreeNode node, IComparer<DefaultTreeNode> sorter)
        {
            var c = parent.Count;
            for (var idx = 0; idx < c; idx += 1)
            {
                var current = parent[idx];
                if (sorter.Compare(current, node) > 0)
                {
                    parent.InsertAt(node, idx);
                    return idx;
                }
            }

            parent.Add(node);
            return c;
        }
    }
}