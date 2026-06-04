// Located at: Assets/Scripts/Core/FacingSprite.cs
using UnityEngine;

namespace ProjectWitchcraft.Core
{
    // Selects a directional sprite based on a facing transform's XZ forward direction.
    // Array order: 0=Front(South), 1=Right(East), 2=Back(North), 3=Left(West).
    // For 8 directions add: 0=S, 1=SE, 2=E, 3=NE, 4=N, 5=NW, 6=W, 7=SW — no code change needed.
    [RequireComponent(typeof(SpriteRenderer))]
    public class FacingSprite : MonoBehaviour
    {
        [SerializeField] private Transform _facingSource;
        [SerializeField] private Sprite[] _directionSprites = new Sprite[4];

        private SpriteRenderer _spriteRenderer;
        private int _lastIndex = -1;

        private void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
        }

        private void LateUpdate()
        {
            if (_facingSource == null || _directionSprites.Length == 0) return;

            int index = GetDirectionIndex(_facingSource.forward);
            if (index == _lastIndex) return;

            _lastIndex = index;
            if (_directionSprites[index] != null)
                _spriteRenderer.sprite = _directionSprites[index];
        }

        private int GetDirectionIndex(Vector3 forward)
        {
            // Atan2(x, -z) gives 0° = south (front), increasing clockwise.
            float angle = Mathf.Atan2(forward.x, -forward.z) * Mathf.Rad2Deg;
            float normalized = (angle + 360f) % 360f;
            int count = _directionSprites.Length;
            float sectorSize = 360f / count;
            return Mathf.FloorToInt(((normalized + sectorSize * 0.5f) % 360f) / sectorSize);
        }
    }
}
