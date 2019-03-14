using System.Collections.Generic;
using UnityEngine;

namespace UnityTutorialSystem.UI.Trees
{
    /// <summary>
    ///     abstract because of generics and unity.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class TreeView<T> : MonoBehaviour
    {
        readonly List<TreeItemRenderer<T>> pool;
        [SerializeField] bool showRootNode;
        ITreeModel<T> model;

        protected TreeView()
        {
            pool = new List<TreeItemRenderer<T>>();
        }

        public bool ShowRootNode
        {
            get { return showRootNode; }
            set
            {
                showRootNode = value;
                OnModelUpdated();
            }
        }

        protected ITreeModel<T> InternalModel
        {
            get { return model; }
            set
            {
                if (model != null)
                {
                    model.NodesChanged -= OnNodesChanged;
                    model.NodesInserted -= OnNodesInserted;
                    model.NodesRemoved -= OnNodesRemoved;
                    model.StructureChanged -= OnStructureChanged;
                }

                model = value;
                if (model != null)
                {
                    model.NodesChanged += OnNodesChanged;
                    model.NodesInserted += OnNodesInserted;
                    model.NodesRemoved += OnNodesRemoved;
                    model.StructureChanged += OnStructureChanged;
                }

                OnModelUpdated();
            }
        }

        protected abstract TreeItemRenderer<T> ItemRenderer { get; }

        protected virtual void Awake()
        {
            if (ItemRenderer != null)
            {
                ItemRenderer.gameObject.SetActive(false);
            }
        }

        protected virtual void OnStructureChanged(object sender, TreeModelEventArgs<T> e)
        {
            OnModelUpdated();
        }

        protected virtual void OnNodesRemoved(object sender, TreeModelEventArgs<T> e)
        {
            OnModelUpdated();
        }

        protected virtual void OnNodesInserted(object sender, TreeModelEventArgs<T> e)
        {
            OnModelUpdated();
        }

        protected virtual void OnNodesChanged(object sender, TreeModelEventArgs<T> e)
        {
            OnModelUpdated();
        }

        protected virtual void OnModelUpdated()
        {
            if ((model == null) || (model.HasRoot == false))
            {
                foreach (var p in pool)
                {
                    p.Path = null;
                    p.gameObject.SetActive(false);
                    //p.transform.SetParent(null, false);
                }

                return;
            }

            var r = model.Root;
            var usedNodes = RefreshNodes(TreePath.From(r), 0, ShowRootNode);
            for (var i = usedNodes; i < pool.Count; i += 1)
            {
                var p = pool[i];
                p.Path = null;
                p.gameObject.SetActive(false);
                //p.transform.SetParent(null, false);
            }
        }

        protected virtual int RefreshNodes(TreePath<T> node, int nextNodeIndexInPool, bool visible)
        {
            while (pool.Count <= nextNodeIndexInPool)
            {
                pool.Add(InstantiateRenderer());
            }

            if (!IsPathVisible(node))
            {
                return nextNodeIndexInPool;
            }

            if (visible)
            {
                var r = pool[nextNodeIndexInPool];
                //r.transform.SetParent(transform, false);
                r.Path = node;
                r.gameObject.SetActive(true);
                nextNodeIndexInPool += 1;
            }

            T t;
            if (node.TryGetLastComponent(out t))
            {
                var count = model.ChildCount(t);
                for (var i = 0; i < count; i += 1)
                {
                    var child = model.GetChildAt(t, i);
                    nextNodeIndexInPool = RefreshNodes(new TreePath<T>(node, child), nextNodeIndexInPool, true);
                }
            }

            return nextNodeIndexInPool;
        }

        protected virtual bool IsPathVisible(TreePath<T> path)
        {
            return true;
        }

        protected virtual TreeItemRenderer<T> InstantiateRenderer()
        {
            var itemRenderer = Instantiate(ItemRenderer);
            var rtransform = itemRenderer.transform;
            rtransform.SetParent(transform, false);
            return itemRenderer;
        }
    }
}