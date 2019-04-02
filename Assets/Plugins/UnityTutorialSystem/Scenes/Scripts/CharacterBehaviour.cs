using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Events;

namespace UnityTutorialSystem.Scenes.Scripts
{
    /// <summary>
    ///   A basic character controller that allows WASD movement and mouse look in a single script.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class CharacterBehaviour : MonoBehaviour
    {
        [BoxGroup("Movement")]
        [Tooltip("Modifier Key to enable strafing movement")]
        [SerializeField] KeyCode strafeKey;
        
        [BoxGroup("Movement")]
        [Tooltip("Alternative modifier Key to enable strafing movement")]
        [SerializeField] KeyCode strafeKey2;
        
        [BoxGroup("Movement")]
        [SerializeField] bool independentHeadMovement;

        [BoxGroup("Movement")]
        [SerializeField] bool invertHeadLook;
        [BoxGroup("Movement")]
        [SerializeField] bool mouseMovement;

        [BoxGroup("Movement Speed")]
        [SerializeField] float forwardMovementSpeed;
        [BoxGroup("Movement Speed")]
        [SerializeField] float reverseMovementSpeed;
        [BoxGroup("Movement Speed")]
        [SerializeField] float strafeSpeed;
        [BoxGroup("Movement Speed")]
        [SerializeField] float rotationSpeed;
        [BoxGroup("Movement Speed")]
        [Tooltip("At which velocity does the animation system show an movement animation?")]
        [SerializeField] float movementThreshold;
        
        [BoxGroup("Internal")]
        [SerializeField] Transform head;
        
        CharacterController characterController;
        [SerializeField] UnityEvent movementBlocked;
        [SerializeField] UnityEvent moving;

        public UnityEvent MovementBlocked => movementBlocked;

        public UnityEvent Moving => moving;

        public CollisionFlags MovementResult { get; private set; }

        void Reset()
        {
            strafeKey = KeyCode.LeftControl;
            strafeKey2 = KeyCode.RightControl;
            forwardMovementSpeed = 2;
            reverseMovementSpeed = 1;
            strafeSpeed = 1;
            movementThreshold = 0.1f;
            rotationSpeed = 5f;
        }

        void Awake()
        {
            characterController = GetComponent<CharacterController>();
        }

        void FixedUpdate()
        {
            var h = Input.GetAxis("Horizontal");
            if (mouseMovement && !independentHeadMovement)
            {
                h += Input.GetAxis("Mouse X");
            }

            var v = Input.GetAxis("Vertical");
            var velocity = Vector3.zero;
            if (v < 0)
            {
                velocity += transform.forward * v * reverseMovementSpeed * Time.fixedDeltaTime;
            }
            else
            {
                velocity += transform.forward * v * forwardMovementSpeed * Time.fixedDeltaTime;
            }

            if (Input.GetKey(strafeKey) || Input.GetKey(strafeKey2))
            {
                velocity += transform.right * h * strafeSpeed * Time.fixedDeltaTime;
            }
            else
            {
                transform.Rotate(Vector3.up, h * rotationSpeed * Time.fixedDeltaTime);
            }

            if (mouseMovement)
            {
                head.localRotation = HandleMouseLook();
            }

            MovementResult = characterController.Move(velocity);

            if (MovementResult != 0)
            {
                movementBlocked?.Invoke();
            }

            if (characterController.velocity.magnitude > movementThreshold)
            {
                moving?.Invoke();
            }
        }

        Quaternion HandleMouseLook()
        {
            var mouseLookAxisX = independentHeadMovement ? Input.GetAxis("Mouse X") : 0;
            var mouseLookAxisY = Input.GetAxis("Mouse Y") * (invertHeadLook ? 1 : -1);
            var angleY = mouseLookAxisX * Time.fixedDeltaTime * rotationSpeed;
            var angleX = mouseLookAxisY * Time.fixedDeltaTime * rotationSpeed;

            var eulers = head.localRotation.eulerAngles;
            eulers.y += angleY;
            eulers.x = Mathf.Clamp(Mathf.DeltaAngle(0, eulers.x + angleX), -45, 45);
            eulers.z = 0;
            return Quaternion.Euler(eulers);
        }
    }
}