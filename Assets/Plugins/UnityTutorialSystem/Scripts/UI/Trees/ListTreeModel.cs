using System.Collections.Generic;

namespace UnityTutorialSystem.UI.Trees
{
    public class ListTreeModel<TList> : TreeModel<TList> where TList : class, IReadOnlyList<TList>
    {
        public ListTreeModel()
        {
        }

        public ListTreeModel(TList root)
        {
            Root = root;
        }

        public override bool HasRoot => Root != null;
        public override TList Root { get; set; }

        public override bool IsLeaf(TList node)
        {
            return node.Count == 0;
        }

        public override TList GetChildAt(TList parent, int index)
        {
            return parent[index];
        }

        public override int ChildCount(TList parent)
        {
            return parent.Count;
        }

        public override int GetIndexOfChild(TList parent, TList child)
        {
            for (var i = 0; i < parent.Count; i++)
            {
                var c = parent[i];
                if (c == child)
                {
                    return i;
                }
            }

            return -1;
        }
    }
}