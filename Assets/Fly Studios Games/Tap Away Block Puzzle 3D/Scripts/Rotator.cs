using UnityEngine;

namespace Tap_Away_Block_Puzzle_3D
{
    /// <summary>
    /// Simple rotation helper: rotates the transform each frame.
    /// </summary>
    public class Rotator : MonoBehaviour
    {
        [Header("Rotation Settings")]
        [Tooltip("Rotation speed in degrees per second.")]
        public float rotationSpeed = 5f;

        [Tooltip("Rotation axis. Default is -Z (Vector3.back).")]
        public Vector3 rotationAxis = Vector3.back;

        private void Update()
        {
            // Rotate by (axis * speed * deltaTime). Keeps original behavior (default -Z).
            transform.Rotate(rotationAxis * rotationSpeed * Time.deltaTime);
        }
    }
}