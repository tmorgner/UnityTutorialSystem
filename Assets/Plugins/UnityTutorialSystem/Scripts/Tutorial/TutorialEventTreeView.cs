using UnityEngine;
using UnityTutorialSystem.UI;
using UnityTutorialSystem.UI.Trees;

namespace UnityTutorialSystem.Tutorial
{     
    public class TutorialEventTreeView : TreeView<EventStreamTreeModelData>
    {
        [SerializeField] TutorialEventTreeItemRenderer template;
        [SerializeField] bool showAllNodes;
        [SerializeField] bool hideCompletedSubTrees;
        protected override TreeItemRenderer<EventStreamTreeModelData> ItemRenderer => template;

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