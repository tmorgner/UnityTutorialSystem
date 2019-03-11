﻿using TutorialSystem.UI.Trees;
using UnityEngine;

namespace TutorialSystem.Helpers
{
    public class TutorialEventTreeView : TreeView<TutorialEventStateData>
    {
        [SerializeField] TutorialEventTreeItemRenderer template;
        [SerializeField] bool showAllNodes;
        [SerializeField] bool hideCompletedSubTrees;
        ListTreeModel<TutorialEventStateData> model;

        protected override TreeItemRenderer<TutorialEventStateData> ItemRenderer => template;

        public ListTreeModel<TutorialEventStateData> Model
        {
            get { return model; }
            set
            {
                model = value;
                InternalModel = value;
            }
        }

        protected override bool IsPathVisible(TreePath<TutorialEventStateData> path)
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
                TutorialEventStateData stateData;
                if (path.TryGetLastComponent(out stateData) && (stateData != null))
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