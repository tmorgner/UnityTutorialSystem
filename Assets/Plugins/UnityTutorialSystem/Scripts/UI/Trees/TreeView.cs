using System.Collections.Generic;
using UnityEngine;

namespace UnityTutorialSystem.UI.Trees
{
    /// <summary>
    ///     <para>
    ///        A TreeView component. This class takes a Tree structure represented by the
    ///        <see cref="ITreeModel{TNode}"/> and unrolls it into a rendered structure.
    ///        This base class does not track the expanded state of nodes, if you want to
    ///        support this use the <see cref="IsPathVisible"/> method to influence whether
    ///        collapsed nodes are shown.
    ///     </para>
    ///     <para>
    ///        This class will generate a (pooled) copy of the a template TreeItemRenderer
    ///        game-object and behaviour and expects that the child nodes of the RectTransform
    ///        are laid out correctly by an independent LayoutElement behaviour attached to the
    ///        same GameObject.
    ///     </para>
    ///     <para>This class is abstract because it uses generics and unity does not like that.</para>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class TreeView<T> : MonoBehaviour
    {
        readonly List<TreeItemRenderer<T>> pool;
        [SerializeField] bool showRootNode;
        ITreeModel<T> model;

        /// <inheritdoc />
        protected TreeView()
        {
            pool = new List<TreeItemRenderer<T>>();
        }

        /// <summary>
        ///   Defines whether the root node of the model will be visible.
        ///   If set to false, this tree will look as if it is a list of
        ///   independent sub trees and nodes.
        /// </summary>
        public bool ShowRootNode
        {
            get { return showRootNode; }
            set
            {
                showRootNode = value;
                OnModelUpdated();
            }
        }

        /// <summary>
        ///   The TreeModel that provides the data for this component. The
        ///   tree will update in response to the events fired by the model.
        /// </summary>
        public ITreeModel<T> Model
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

        /// <summary>
        ///   A TreeItemRenderer. The renderer will be cloned via
        ///   <see cref="Object.Instantiate(UnityEngine.Object)"/>
        ///   for each node in the tree. This TreeView implementation here
        ///   expects that all calls to this property return the same instance
        ///   for the lifetime of this object.
        /// 
        ///   This property is abstract to work around Unity's Generics limitations.
        /// </summary>
        protected abstract TreeItemRenderer<T> ItemRenderer { get; }

        /// <summary>
        ///   Called when the script instance is loaded. This simply disables the
        ///   template item renderer game object.
        /// </summary>
        protected virtual void Awake()
        {
            if (ItemRenderer != null)
            {
                ItemRenderer.gameObject.SetActive(false);
            }
        }

        /// <summary>
        ///   Called when the tree model has changed.
        /// </summary>
        /// <param name="sender">The tree model that caused this event.</param>
        /// <param name="e">The tree model change event arguments.</param>
        protected virtual void OnStructureChanged(object sender, TreeModelEventArgs<T> e)
        {
            OnModelUpdated();
        }

        /// <summary>
        ///   Called when the tree model has changed.
        /// </summary>
        /// <param name="sender">The tree model that caused this event.</param>
        /// <param name="e">The tree model change event arguments.</param>
        protected virtual void OnNodesRemoved(object sender, TreeModelEventArgs<T> e)
        {
            OnModelUpdated();
        }

        /// <summary>
        ///   Called when the tree model has changed.
        /// </summary>
        /// <param name="sender">The tree model that caused this event.</param>
        /// <param name="e">The tree model change event arguments.</param>
        protected virtual void OnNodesInserted(object sender, TreeModelEventArgs<T> e)
        {
            OnModelUpdated();
        }

        /// <summary>
        ///   Called when the tree model has changed.
        /// </summary>
        /// <param name="sender">The tree model that caused this event.</param>
        /// <param name="e">The tree model change event arguments.</param>
        protected virtual void OnNodesChanged(object sender, TreeModelEventArgs<T> e)
        {
            OnModelUpdated();
        }

        /// <summary>
        ///   Callback handler that is invoked in response to data changes.
        /// </summary>
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

        /// <summary>
        ///   Internal method that is called as part of the rebuilding of render nodes after
        ///   data in the model has changed. Refreshes the tree.
        /// </summary>
        /// <param name="node">The node to refresh</param>
        /// <param name="nextNodeIndexInPool">The index of the next free node in the ItemRenderer pool</param>
        /// <param name="visible">a flag indicating whether the current node is visible.
        ///    Normally we would not process invisible nodes, but the root node can be
        ///    invisible and still contain visible child nodes if <see cref="ShowRootNode"/> is set to false.
        /// </param>
        /// <returns>A pointer to the next free node in the TreeItemRenderer pool.</returns>
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
                    nextNodeIndexInPool = RefreshNodes(node.Append(child), nextNodeIndexInPool, true);
                }
            }

            return nextNodeIndexInPool;
        }

        /// <summary>
        ///   Checks whether this tree-path should be visible in the rendered tree. 
        /// </summary>
        /// <param name="path">The TreePath to check</param>
        /// <returns>true, if the node should show, false otherwise.</returns>
        protected virtual bool IsPathVisible(TreePath<T> path)
        {
            return true;
        }

        /// <summary>
        ///   A helper method that instantiates a new ItemRenderer.  
        /// </summary>
        /// <returns></returns>
        protected virtual TreeItemRenderer<T> InstantiateRenderer()
        {
            var itemRenderer = Instantiate(ItemRenderer);
            var rtransform = itemRenderer.transform;
            rtransform.SetParent(transform, false);
            return itemRenderer;
        }
    }
}