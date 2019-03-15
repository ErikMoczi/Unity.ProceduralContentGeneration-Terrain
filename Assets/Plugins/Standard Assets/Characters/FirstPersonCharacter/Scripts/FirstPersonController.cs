using UnityEngine;

namespace UnityStandardAssets.Characters.FirstPerson
{
    [RequireComponent(typeof(Rigidbody))]
    public sealed class FirstPersonController : MonoBehaviour
    {
#pragma warning disable 649     
        [SerializeField] private Camera cam;
        [SerializeField] private MovementSettings movementSettings = new MovementSettings();
        [SerializeField] private MouseLook mouseLook = new MouseLook();
#pragma warning restore 649

        private Rigidbody _rigidBody;
        private float _rotationY;
        private Vector3 _groundContactNormal;

        private void Start()
        {
            _rigidBody = GetComponent<Rigidbody>();
            mouseLook.Init(transform, cam.transform);
        }

        private void Update()
        {
            RotateView();
        }

        private void FixedUpdate()
        {
            var input = GetInput();
            var desiredMove = transform.forward * input.y + transform.right * input.x;
            if (Mathf.Abs(input.x) > float.Epsilon || Mathf.Abs(input.y) > float.Epsilon)
            {
                desiredMove.x = desiredMove.x * movementSettings.CurrentTargetSpeed;
                desiredMove.z = desiredMove.z * movementSettings.CurrentTargetSpeed;
                desiredMove.y = desiredMove.y * movementSettings.CurrentTargetSpeed;
                if (_rigidBody.velocity.sqrMagnitude <
                    movementSettings.CurrentTargetSpeed * movementSettings.CurrentTargetSpeed)
                {
                    _rigidBody.AddForce(desiredMove, ForceMode.Impulse);
                }
            }
        }

        private Vector2 GetInput()
        {
            var input = new Vector2
            {
                x = Input.GetAxis("Horizontal"),
                y = Input.GetAxis("Vertical")
            };
            movementSettings.UpdateDesiredTargetSpeed(input);

            return input;
        }

        private void RotateView()
        {
            if (Mathf.Abs(Time.timeScale) < float.Epsilon)
            {
                return;
            }

            mouseLook.LookRotation(transform, cam.transform);
        }
    }
}