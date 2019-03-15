using System;
using UnityEngine;

namespace UnityStandardAssets.Characters.FirstPerson
{
    [Serializable]
    public sealed class MovementSettings
    {
#pragma warning disable 649   
        [SerializeField] private float forwardSpeed = 8.0f;
        [SerializeField] private float backwardSpeed = 4.0f;
        [SerializeField] private float strafeSpeed = 4.0f;
        [SerializeField] private float runMultiplier = 2.0f;
        [SerializeField] private KeyCode runKey = KeyCode.LeftShift;
#pragma warning restore 649

        private bool _running;
        public float CurrentTargetSpeed { get; private set; } = 8f;

        public void UpdateDesiredTargetSpeed(Vector2 input)
        {
            if (input == Vector2.zero)
            {
                return;
            }

            if (input.x > 0 || input.x < 0)
            {
                CurrentTargetSpeed = strafeSpeed;
            }

            if (input.y < 0)
            {
                CurrentTargetSpeed = backwardSpeed;
            }

            if (input.y > 0)
            {
                CurrentTargetSpeed = forwardSpeed;
            }

            if (Input.GetKey(runKey))
            {
                CurrentTargetSpeed *= runMultiplier;
            }
        }
    }
}