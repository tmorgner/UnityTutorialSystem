using System;

namespace TutorialSystem.UI.Trees
{
    [Serializable]
    public class DefaultTreeModel : TreeModel<DefaultTreeNode>
    {
        DefaultTreeNode root;

        public DefaultTreeModel()
        {
            Root = new DefaultTreeNode();
        }

        public DefaultTreeModel(DefaultTreeNode root)
        {
            Root = root;
        }

        public override bool HasRoot => root != null;

        public override DefaultTreeNode Root
        {
            get { return root; }
            set
            {
                var old = root;
                root = value;
                if (!ReferenceEquals(old, root))
                {
                    FireStructureChanged();
                }
            }
        }

        public override bool IsLeaf(DefaultTreeNode node)
        {
            return node.IsLeaf;
        }

        public override DefaultTreeNode GetChildAt(DefaultTreeNode parent, int index)
        {
            return parent[index];
        }

        public override int ChildCount(DefaultTreeNode parent)
        {
            return parent.Count;
        }

        public override int GetIndexOfChild(DefaultTreeNode parent, DefaultTreeNode child)
        {
            return parent.IndexOf(child);
        }

        public void InsertChild(DefaultTreeNode parent, DefaultTreeNode child)
        {
            child.Parent?.Remove(child);

            var index = parent.Count;
            parent.InsertAt(child, index);
            FireNodeInserted(parent.Path, index, child);
        }

        public void InsertChild(DefaultTreeNode parent, DefaultTreeNode child, int index)
        {
            parent.InsertAt(child, index);
            FireNodeInserted(parent.Path, index, child);
        }

        public void RemoveChild(DefaultTreeNode parent, DefaultTreeNode child)
        {
            var idx = parent.IndexOf(child);
            if (idx == -1)
            {
                throw new ArgumentException();
            }

            parent.Remove(child);
            FireNodeRemoved(parent.Path, idx, child);
        }
    }
}