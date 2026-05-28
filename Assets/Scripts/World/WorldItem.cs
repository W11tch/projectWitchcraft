// Located at: Assets/Scripts/World/WorldItem.cs
using UnityEngine;
using ProjectWitchcraft.Core;
using ProjectWitchcraft.Managers;
using System.Collections;

namespace ProjectWitchcraft.World
{
    [RequireComponent(typeof(SphereCollider))]
    public class WorldItem : MonoBehaviour
    {
        private enum ItemState { Animating, Idle, Attracted }

        [Header("Dependencies")]
        [SerializeField] private SpriteRenderer _iconSpriteRenderer;
        [SerializeField] private Transform _iconTransform;
        [SerializeField] private SpriteRenderer _shadowSpriteRenderer;

        [Header("Configuration")]
        [SerializeField] private float _pickupDistance = 0.5f;
        // **FIX**: This new field will control the final size of ALL dropped items.
        [Tooltip("The desired size (width) of the dropped item's sprite in world units.")]
        [SerializeField] private float _droppedItemWorldSize = 0.5f;

        [Header("Ground Detection")]
        [SerializeField] private LayerMask _groundLayer;
        [SerializeField] private float _raycastDistance = 5f;

        [Header("Bounce Animation")]
        [SerializeField] private float _dropDuration = 0.2f;
        [SerializeField] private float _bounceHeight = 0.5f;
        [SerializeField] private float _bounceDuration = 0.5f;
        [SerializeField] private float _floatHeight = 0.25f;

        [Header("Attraction")]
        [SerializeField] private float _attractionSpeed = 8f;

        // **FIX**: This is the new baseline for scaling. We assume 64 PPU is the "normal" size.
        private const float BASE_PIXELS_PER_UNIT = 32f;

        public ItemData ItemData { get; private set; }
        public int Quantity { get; private set; }

        private ItemState _currentState;
        private Transform _playerTransform;

        private void Awake()
        {
            GetComponent<SphereCollider>().isTrigger = true;
        }

        private void OnEnable()
        {
            // Reset scale on enable to ensure pooled objects don't keep old scales.
            if (_iconTransform != null) _iconTransform.localScale = Vector3.one;
            StartCoroutine(AnimateLifecycle());
        }

        public void Initialize(ItemData itemData, int quantity)
        {
            ItemData = itemData;
            Quantity = quantity;

            if (_iconSpriteRenderer != null)
            {
                _iconSpriteRenderer.sprite = itemData.Icon;

                // **THE DEFINITIVE FIX**: This new logic normalizes the scale of all sprites.
                if (itemData.Icon != null && itemData.Icon.pixelsPerUnit > 0)
                {
                    // 1. Calculate the sprite's original size in world units.
                    //    (e.g., a 32px sprite with 32 PPU is 1 unit wide).
                    //    (e.g., a 64px sprite with 64 PPU is 1 unit wide).
                    float originalWorldWidth = itemData.Icon.rect.width / itemData.Icon.pixelsPerUnit;

                    // 2. Calculate the scale needed to make it match our target size.
                    //    (e.g., for both sprites above, this would be 0.5 / 1.0 = 0.5).
                    float scaleMultiplier = _droppedItemWorldSize / originalWorldWidth;

                    // 3. Apply the final, normalized scale.
                    if (_iconTransform != null)
                    {
                        _iconTransform.localScale = Vector3.one * scaleMultiplier;
                    }
                }
            }


            if (_playerTransform == null)
            {
                var playerObject = GameObject.FindGameObjectWithTag("Player");
                if (playerObject != null)
                    _playerTransform = playerObject.transform;
            }
        }

        private IEnumerator AnimateLifecycle()
        {
            _currentState = ItemState.Animating;

            if (_iconSpriteRenderer != null) _iconSpriteRenderer.enabled = false;
            if (_shadowSpriteRenderer != null) _shadowSpriteRenderer.enabled = false;

            yield return null;

            Vector3 spawnPosition = transform.position;
            Vector3 groundPosition = spawnPosition;

            if (Physics.Raycast(spawnPosition, Vector3.down, out RaycastHit hit, _raycastDistance, _groundLayer))
            {
                groundPosition = hit.point;
            }

            if (_iconSpriteRenderer != null) _iconSpriteRenderer.enabled = true;
            if (_shadowSpriteRenderer != null) _shadowSpriteRenderer.enabled = true;

            float elapsedTime = 0f;
            while (elapsedTime < _dropDuration)
            {
                transform.position = Vector3.Lerp(spawnPosition, groundPosition, elapsedTime / _dropDuration);
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            transform.position = groundPosition;

            elapsedTime = 0f;
            while (elapsedTime < _bounceDuration)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / _bounceDuration;

                float height = _floatHeight + (Mathf.Sin(t * Mathf.PI) * _bounceHeight);
                if (_iconTransform != null)
                {
                    _iconTransform.localPosition = new Vector3(0, height, 0);
                }

                yield return null;
            }

            if (_iconTransform != null)
            {
                _iconTransform.localPosition = new Vector3(0, _floatHeight, 0);
            }
            _currentState = ItemState.Idle;
        }

        private void Update()
        {
            if (_currentState != ItemState.Attracted || _playerTransform == null) return;

            Vector3 directionToPlayer = (_playerTransform.position - transform.position).normalized;
            transform.position += directionToPlayer * _attractionSpeed * Time.deltaTime;

            if (Vector3.Distance(transform.position, _playerTransform.position) < _pickupDistance)
            {
                TryPickup();
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (_currentState == ItemState.Idle && other.CompareTag("Player"))
            {
                _currentState = ItemState.Attracted;
            }
        }

        private void TryPickup()
        {
            if (ItemData == null) return;

            int remainingQuantity = InventoryManager.Instance.AddItem(ItemData, Quantity);

            if (remainingQuantity == 0)
            {
                gameObject.SetActive(false);
            }
            else
            {
                Quantity = remainingQuantity;
                _currentState = ItemState.Idle;
            }
        }
    }
}