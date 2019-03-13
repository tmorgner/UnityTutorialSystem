using UnityEngine;

namespace UnityTutorialSystem.UI
{
    /// <summary>
    ///     A small helper that deactivates template elements when not in the editor.
    ///     Template elements are instantiated via a script, which also takes care of
    ///     setting the newly instantiated game object active.
    /// </summary>
    public class VisibleDuringEdit : MonoBehaviour
    {
        void Awake()
        {
            if (Application.isPlaying)
            {
                Debug.Log("Hiding " + name);
                gameObject.SetActive(false);
            }
        }
    }
}