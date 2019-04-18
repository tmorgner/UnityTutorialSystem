﻿using UnityEngine;
using UnityTutorialSystem.UI;
using UnityTutorialSystem.UI.Trees;

namespace UnityTutorialSystem.Tutorial
{     
    /// <summary>
    ///   A TreeView for EventStreamTreeModelData as produced by the
    ///   EventStreamTreeModelBuilder. This class only works well if it
    ///   is used in conjunction with a LayoutElement that can auto-arrange
    ///   the child nodes generated by the view.
    /// </summary>
    /// <inheritdoc/>
    public class TutorialEventTreeView : TreeView<EventStreamTreeModelData>
    {
        [SerializeField] TutorialEventTreeItemRenderer template;
        [SerializeField] bool showAllNodes;
        [SerializeField] bool hideCompletedSubTrees;


        /// <inheritdoc />
        protected override TreeItemRenderer<EventStreamTreeModelData> ItemRenderer => template;

        /// <summary>
        ///   Controls the visibility of nodes. This method is depends on the showAllNodes and
        ///   hideCompletedSubTrees injected variables.
        ///   If hideCompletedSubTrees is true, it will collapse branches where all nodes have
        ///   been completed. If this test would make the node visible, a second test based on
        ///   showAllNodes will selectively only show nodes that are either completed or expected
        ///   next.
        /// </summary>
        /// <param name="path">The tree path to check</param>
        /// <returns>true to show the corresponding node, false otherwise.</returns>
        protected override bool IsPathVisible(TreePath<EventStreamTreeModelData> path)
        {
            if (hideCompletedSubTrees)
            {
                if (path.Count > 1)
                {
                    var parentNode = path[path.Count - 2];
                    if (parentNode.Completed)
                    {
                        return false;
                    }
                }
            }

            if (!showAllNodes)
            {
                if (path.TryGetLastComponent(out var stateData) && (stateData != null))
                {
                    // Debug.Log("Maybe Hiding node " + path);
                    return stateData.Completed || stateData.ExpectedNext;
                }

                // Debug.Log("Hiding node " + path);
                return false;
            }

            return true;
        }
    }
}