using UnityEngine;
using UnityEngine.Events;

namespace UnityTutorialSystem.Helpers
{
    public class MouseEventPublisher : MonoBehaviour
    {
        public UnityEvent Clicked;
        public UnityEvent MouseEntered;
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