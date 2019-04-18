using UnityEngine;
using UnityEngine.Events;

namespace UnityTutorialSystem.Helpers
{
    /// <summary>
    ///   A basic helper object that republishes incoming Unity event messages as
    ///   proper <see cref="UnityEvent"/>s.
    /// </summary>
    public class MouseEventPublisher : MonoBehaviour
    {
        /// <summary>
        ///   Republishes framework events received from clicks.
        /// </summary>
        public UnityEvent Clicked;
        /// <summary>
        ///   Republishes framework events received from MouseEntered events.
        /// </summary>
        public UnityEvent MouseEntered;
        /// <summary>
        ///   Republishes framework events received from MouseExited events.
        /// </summary>
        public UnityEvent MouseExited;

        void OnMouseUp()
        {
            Clicked?.Invoke();
        }

        void OnMouseEnter()
        {
            MouseEntered?.Invoke();
        }

        void OnMouseExit()
        {
            MouseExited?.Invoke();
        }
    }
}