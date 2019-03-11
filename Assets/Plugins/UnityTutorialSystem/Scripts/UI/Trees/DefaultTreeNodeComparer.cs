using System;
using System.Collections.Generic;

namespace TutorialSystem.UI.Trees
{
    public class DefaultTreeNodeComparer<T> : IComparer<DefaultTreeNode>
    {
        readonly IComparer<T> valueComparer;

        public DefaultTreeNodeComparer(IComparer<T> valueComparer)
        {
            if (valueComparer == null)
            {
                throw new ArgumentNullException(nameof(valueComparer));
            }

            this.valueComparer = valueComparer;
        }

        public int Compare(DefaultTreeNode x, DefaultTreeNode y)
        {
            if ((x == null) && (y == null))
            {
                return 0;
            }

            if (x == null)
            {
                return -1;
            }

            if (y == null)
            {
                return 1;
            }

            var xT = x.Value is T;
            var yT = y.Value is T;
            if (!xT && !yT)
            {
                return 0;
            }

            if (!xT)
            {
                return -1;
            }

            if (!yT)
            {
                return 1;
            }

            return valueComparer.Compare((T) x.Value, (T) y.Value);
        }
    }
}