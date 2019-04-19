using UnityEngine;
using UnityTutorialSystem.UI;

namespace UnityTutorialSystem.Tutorial
{
    /// <summary>
    ///   A basic binding component to decouple presentation (TreeView) from
    ///   data source (EventStreamTreeModelBuilder). This class simply connects the
    ///   model created by the EventStreamTreeModelBuilder to the TreeView.
    /// </summary>
    public class TutorialEventTreeBinding : MonoBehaviour
    {
        [SerializeField] EventStreamTreeModelBuilder modelSource;
        [SerializeField] TutorialEventTreeView treeView;

        void Start()
        {
            treeView.Model = modelSource.Model;
        }
    }
}