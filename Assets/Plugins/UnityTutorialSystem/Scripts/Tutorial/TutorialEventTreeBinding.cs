using UnityEngine;
using UnityTutorialSystem.UI;

namespace UnityTutorialSystem.Tutorial
{
    public class TutorialEventTreeBinding : MonoBehaviour
    {
        [SerializeField] EventStreamTreeModelBuilder modelSource;
        [SerializeField] TutorialEventTreeView treeView;

        void Awake()
        {
            treeView.Model = modelSource.Model;
        }
    }
}