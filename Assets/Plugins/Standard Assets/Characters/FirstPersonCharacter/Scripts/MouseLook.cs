using System;
using UnityEngine;

namespace UnityStandardAssets.Characters.FirstPerson
{
    [Serializable]
    public sealed class MouseLook
    {
#pragma warning disable 649       
        [SerializeField] private float xSensitivity = 2f;
        [SerializeField] private float ySensitivity = 2f;
        [SerializeField] private bool clampVerticalRotation = true;
        [SerializeField] private float minimumX = -90F;
        [SerializeField] private float maximumX = 90F;
        [SerializeField] private bool smooth;
        [SerializeField] private float smoothTime = 5f;
        [SerializeField] private bool lockCursor = true;
#pragma warning restore 649

        private Quaternion _characterTargetRot;
        private Quaternion _cameraTargetRot;
        private bool _cursorIsLocked = true;

        public void Init(Transform character, Transform camera)
        {
            _characterTargetRot = character.localRotation;
            _cameraTargetRot = camera.localRotation;
        }

        public void LookRotation(Transform character, Transform camera)
        {
            UpdateCursorLock();

            if (lockCursor && !_cursorIsLocked)
            {
                return;
            }

            var yRot = Input.GetAxis("Mouse X") * xSensitivity;
            var xRot = Input.GetAxis("Mouse Y") * ySensitivity;

            _characterTargetRot *= Quaternion.Euler(0f, yRot, 0f);
            _cameraTargetRot *= Quaternion.Euler(-xRot, 0f, 0f);

            if (clampVerticalRotation)
            {
                _cameraTargetRot = ClampRotationAroundXAxis(_cameraTargetRot);
            }

            if (smooth)
            {
                character.localRotation = Quaternion.Slerp(character.localRotation, _characterTargetRot,
                    smoothTime * Time.deltaTime);
                camera.localRotation =
                    Quaternion.Slerp(camera.localRotation, _cameraTargetRot, smoothTime * Time.deltaTime);
            }
            else
            {
                character.localRotation = _characterTargetRot;
                camera.localRotation = _cameraTargetRot;
            }
        }

        private void UpdateCursorLock()
        {
            if (lockCursor)
            {
                InternalLockUpdate();
            }
        }

        private void InternalLockUpdate()
        {
            if (Input.GetKeyUp(KeyCode.Escape))
            {
                _cursorIsLocked = !_cursorIsLocked;
            }

            if (_cursorIsLocked)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
            else if (!_cursorIsLocked)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }

        private Quaternion ClampRotationAroundXAxis(Quaternion q)
        {
            q.x /= q.w;
            q.y /= q.w;
            q.z /= q.w;
            q.w = 1.0f;

            var angleX = 2.0f * Mathf.Rad2Deg * Mathf.Atan(q.x);
            angleX = Mathf.Clamp(angleX, minimumX, maximumX);
            q.x = Mathf.Tan(0.5f * Mathf.Deg2Rad * angleX);
            return q;
        }
    }
}