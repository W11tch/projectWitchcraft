// Located at: Assets/Scripts/Core/RotatableSprite.cs
using UnityEngine;

namespace ProjectWitchcraft.Core
{
    /// <summary>
    /// Manages a SpriteRenderer to display different sprites based on a rotation index.
    /// This allows an object to visually change its facing direction without rotating the actual transform.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class RotatableSprite : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("The sprites to cycle through. Order should be Front, Right, Back, Left.")]
        [SerializeField] private Sprite[] _directionalSprites = new Sprite[4];

        private SpriteRenderer _spriteRenderer;
        private int _rotationIndex = 0; // 0=Front, 1=Right, 2=Back, 3=Left

        // **FIX**: We add a public property to get and set the rotation index from other scripts.
        // This is crucial for saving the state and for applying it when an object is spawned.
        public int RotationIndex
        {
            get => _rotationIndex;
            set
            {
                // Ensure the value wraps around correctly if it's too high
                _rotationIndex = value % _directionalSprites.Length;
                UpdateSprite();
            }
        }

        private void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            // Set the initial sprite
            UpdateSprite();
        }

        /// <summary>
        /// Cycles to the next sprite in the array to simulate rotation.
        /// </summary>
        public void CycleSprite()
        {
            RotationIndex++; // Use the property to handle cycling and updating the sprite
        }

        /// <summary>
        /// Updates the SpriteRenderer to show the current sprite based on the rotation index.
        /// </summary>
        private void UpdateSprite()
        {
            if (_directionalSprites.Length > _rotationIndex && _directionalSprites[_rotationIndex] != null)
            {
                _spriteRenderer.sprite = _directionalSprites[_rotationIndex];
            }
        }
    }
}