using UnityEngine;

namespace ProjectWitchcraft.Player
{
    public class CameraMovement : MonoBehaviour
    {
        [Header("Target")]
        [Tooltip("The player model's transform to follow. DRAG THE 'ModelePlayer' OBJECT HERE.")]
        [SerializeField] private Transform playerModel;

        [Header("Settings")]
        [Tooltip("The offset from the player's position.")]
        [SerializeField] private Vector3 offset;

        [Tooltip("Controls how quickly the camera follows. A value like 5 is responsive.")]
        [SerializeField] private float followSpeed = 5f;

        // LateUpdate is the best place for camera logic.
        void LateUpdate()
        {
            if (playerModel == null) return;

            // Calculate the desired position for the camera.
            Vector3 targetPosition = playerModel.position + offset;

            // Use Lerp for simple, predictable following.
            transform.position = Vector3.Lerp(transform.position, targetPosition, followSpeed * Time.deltaTime);
        }
    }
}