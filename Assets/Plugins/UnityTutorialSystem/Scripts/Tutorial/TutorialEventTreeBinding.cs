using UnityEngine;

namespace UnityTutorialSystem.Tutorial
{
    public class TutorialEventTreeBinding : MonoBehaviour
    {
        [SerializeField] TutorialEventStreamManager modelSource;
        [SerializeField] TutorialEventTreeView treeView;

        void Awake()
        {
            treeView.Model = modelSource.Model;
        }
    }
}