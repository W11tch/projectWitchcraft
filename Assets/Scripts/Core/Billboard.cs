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
        [SerializeField] private float groundingOffset;
        [SerializeField] private float forwardOffset;
        // Enable only on sprite children of placed objects, never on root GameObjects.
        // Keeps the depth/grounding offset in world space so it doesn't rotate with the parent.
        [SerializeField] private bool _worldSpacePositioning;

        private Transform _cameraTransform;
        private float _baseZ;

        private void Awake()
        {
            _baseZ = transform.localPosition.z;
        }

        private void Start()
        {
            if (Camera.main != null)
                _cameraTransform = Camera.main.transform;
        }

        private void LateUpdate()
        {
            if (_cameraTransform == null) return;
            transform.rotation = _cameraTransform.rotation;

            if (_worldSpacePositioning && transform.parent != null)
                transform.position = transform.parent.position
                    + new Vector3(0f, -groundingOffset, -(_baseZ + forwardOffset));
        }
    }
}