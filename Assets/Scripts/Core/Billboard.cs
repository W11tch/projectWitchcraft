// Located at: Assets/Scripts/Core/Billboard.cs
using UnityEngine;

namespace ProjectWitchcraft.Core
{
    /// <summary>
    /// This component makes the GameObject it's attached to always face the main camera.
    /// For a top-down view, it works by matching the camera's X-axis rotation.
    /// </summary>
    public class Billboard : MonoBehaviour
    {
        [Tooltip("Adjust this value in Play Mode to move the sprite down on the screen until it's grounded.")]
        [SerializeField] private float groundingOffset = 0.2f;
        [Tooltip("Adjust this value in Play Mode to move the sprite down on the screen until it's grounded.")]
        [SerializeField] private float forwardOffset = 0.2f;

        private Transform _cameraTransform;
        private Transform _parentTransform;

        private void Start()
        {
            if (Camera.main != null)
            {
                _cameraTransform = Camera.main.transform;
                // Get the parent's transform to use as the logical position anchor.
                _parentTransform = transform.parent;
            }
        }

        private void LateUpdate()
        {
            if (_cameraTransform == null || _parentTransform == null) return;

            // Step 1: Rotate ONLY this object (the visuals) to match the camera.
            transform.rotation = _cameraTransform.rotation;

            // Step 2: Position this object based on the PARENT's position, then apply the offset.
            // The offset is moved along the camera's "up" vector to look correct on screen.
            transform.position = _parentTransform.position - (_cameraTransform.up * groundingOffset) - (_cameraTransform.forward * forwardOffset); ;
        }
    }
}