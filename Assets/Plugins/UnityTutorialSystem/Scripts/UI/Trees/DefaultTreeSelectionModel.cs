using System;
using System.Collections;
using System.Collections.Generic;

namespace TutorialSystem.UI.Trees
{
    public class DefaultTreeSelectionModel<TNode> : ITreeSelectionModel<TNode>
    {
        readonly List<TreePath<TNode>> backend;

        public DefaultTreeSelectionModel()
        {
            backend = new List<TreePath<TNode>>();
            SelectionMode = SelectionMode.SingleSelection;
        }

        public SelectionMode SelectionMode { get; set; }
        public Func<TreePath<TNode>, int> RowMapper { get; set; }

        public TreePath<TNode> SelectionPath
        {
            get
            {
                if (backend.Count > 0)
                {
                    return backend[0];
                }

                return null;
            }
            set
            {
                backend.Clear();
                if (value != null)
                {
                    backend.Add(value);
                }

                SelectionChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public bool Contains(TreePath<TNode> item)
        {
            return backend.Contains(item);
        }

        public void SetPaths(TreePath<TNode>[] p)
        {
            backend.Clear();
            if (p.Length != 0)
            {
                if (SelectionMode == SelectionMode.SingleSelection)
                {
                    backend.Add(p[0]);
                }
                else if (SelectionMode == SelectionMode.ContinuousSelection)
                {
                    if (IsContinuous(p))
                    {
                        backend.AddRange(p);
                    }
                    else
                    {
                        backend.Add(p[0]);
                    }
                }
                else
                {
                    backend.AddRange(p);
                }
            }

            SelectionChanged?.Invoke(this, EventArgs.Empty);
        }

        public void Add(TreePath<TNode> p)
        {
            var tmp = new List<TreePath<TNode>> {p};
            tmp.AddRange(backend);
            SetPaths(tmp.ToArray());
        }

        public void Remove(TreePath<TNode> p)
        {
            var tmp = new List<TreePath<TNode>>(backend);
            tmp.Remove(p);
            SetPaths(tmp.ToArray());
        }

        IEnumerator<TreePath<TNode>> IEnumerable<TreePath<TNode>>.GetEnumerator()
        {
            return GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public int Count => backend.Count;

        public TreePath<TNode> this[int index] => backend[index];

        public event EventHandler<EventArgs> SelectionChanged;

        bool IsContinuous(TreePath<TNode>[] treePaths)
        {
            if (treePaths.Length == 0)
            {
                return true;
            }

            var selectionByIndex = new int[treePaths.Length];
            for (var idx = 0; idx < selectionByIndex.Length; idx += 1)
            {
                var x = RowMapper?.Invoke(treePaths[idx]) ?? -1;
                selectionByIndex[idx] = x;
            }

            Array.Sort(selectionByIndex);
            var expected = selectionByIndex[0];
            if (expected == -1)
            {
                return false;
            }

            for (var idx = 1; idx < selectionByIndex.Length; idx += 1)
            {
                if (selectionByIndex[idx] != expected + 1)
                {
                    return false;
                }

                expected += 1;
            }

            return true;
        }

        public List<TreePath<TNode>>.Enumerator GetEnumerator()
        {
            return backend.GetEnumerator();
        }
    }
}