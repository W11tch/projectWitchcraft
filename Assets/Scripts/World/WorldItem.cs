// Located at: Assets/Scripts/World/WorldItem.cs
using UnityEngine;
using ProjectWitchcraft.Core;
using ProjectWitchcraft.Managers;
using System.Collections;

namespace ProjectWitchcraft.World
{
    [RequireComponent(typeof(SphereCollider))]
    public class WorldItem : MonoBehaviour, IPoolableObject
    {
        private enum ItemState { Animating, Idle, Attracted }

        [Header("Pool")]
        [SerializeField] private string _poolTag;
        public string PoolTag => _poolTag;

        [Header("Dependencies")]
        [SerializeField] private SpriteRenderer _iconSpriteRenderer;
        [SerializeField] private Transform _iconTransform;
        [SerializeField] private SpriteRenderer _shadowSpriteRenderer;

        [Header("Configuration")]
        [SerializeField] private float _pickupDistance = 0.5f;
        [Tooltip("The desired size (width) of the dropped item's sprite in world units.")]
        [SerializeField] private float _droppedItemWorldSize = 0.5f;

        [Header("Ground Detection")]
        [SerializeField] private LayerMask _groundLayer;
        [SerializeField] private float _raycastDistance = 5f;

        [Header("Drop Animation")]
        [Tooltip("Height (in local units) the icon starts at when dropped by the player.")]
        [SerializeField] private float _dropHeight = 1.5f;
        [SerializeField] private float _dropDuration = 0.2f;

        [Header("Bounce Animation")]
        [SerializeField] private float _bounceHeight = 0.5f;
        [SerializeField] private float _bounceDuration = 0.5f;
        [SerializeField] private float _floatHeight = 0.25f;

        [Header("Attraction")]
        [SerializeField] private float _attractionSpeed = 8f;

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
            // Reset visual state only — animation is started by Initialize / InitializeFromSave.
            if (_iconTransform != null) _iconTransform.localScale = Vector3.one;
        }

        private void OnDisable()
        {
            StopAllCoroutines();
        }

        // Called by WorldItemSpawner for normal player drops.
        public void Initialize(ItemData itemData, int quantity, bool playerDrop = false)
        {
            SetupVisuals(itemData, quantity);
            _currentState = ItemState.Animating;
            StopAllCoroutines();
            StartCoroutine(playerDrop ? AnimateDropFromAbove() : AnimatePopFromGround());
        }

        // Called by WorldItemSpawner when restoring items from a save file.
        public void InitializeFromSave(ItemData itemData, int quantity)
        {
            SetupVisuals(itemData, quantity);
            if (_iconSpriteRenderer != null) _iconSpriteRenderer.enabled = true;
            if (_shadowSpriteRenderer != null) _shadowSpriteRenderer.enabled = true;
            if (_iconTransform != null) _iconTransform.localPosition = new Vector3(0, _floatHeight, 0);
            _currentState = ItemState.Idle;
        }

        private void SetupVisuals(ItemData itemData, int quantity)
        {
            ItemData = itemData;
            Quantity = quantity;

            if (_playerTransform == null)
                _playerTransform = GameReferences.Instance.PlayerTransform;

            if (_iconSpriteRenderer != null)
            {
                _iconSpriteRenderer.sprite = itemData.Icon;

                if (itemData.Icon != null && itemData.Icon.pixelsPerUnit > 0)
                {
                    float originalWorldWidth = itemData.Icon.rect.width / itemData.Icon.pixelsPerUnit;
                    float scaleMultiplier = _droppedItemWorldSize / originalWorldWidth;
                    if (_iconTransform != null)
                        _iconTransform.localScale = Vector3.one * scaleMultiplier;
                }
            }
        }

        // Player-drop animation: icon falls from above, root stays at ground position.
        private IEnumerator AnimateDropFromAbove()
        {
            if (_iconSpriteRenderer != null) _iconSpriteRenderer.enabled = false;
            if (_shadowSpriteRenderer != null) _shadowSpriteRenderer.enabled = false;

            yield return null;

            if (_iconSpriteRenderer != null) _iconSpriteRenderer.enabled = true;
            if (_shadowSpriteRenderer != null) _shadowSpriteRenderer.enabled = true;

            // Fall: icon drops from _dropHeight to _floatHeight with a gravity ease-in.
            float elapsed = 0f;
            while (elapsed < _dropDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / _dropDuration);
                float height = Mathf.Lerp(_dropHeight, _floatHeight, t * t);
                if (_iconTransform != null)
                    _iconTransform.localPosition = new Vector3(0, height, 0);
                yield return null;
            }

            // Bounce.
            elapsed = 0f;
            while (elapsed < _bounceDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / _bounceDuration;
                float height = _floatHeight + Mathf.Sin(t * Mathf.PI) * _bounceHeight;
                if (_iconTransform != null)
                    _iconTransform.localPosition = new Vector3(0, height, 0);
                yield return null;
            }

            if (_iconTransform != null)
                _iconTransform.localPosition = new Vector3(0, _floatHeight, 0);
            _currentState = ItemState.Idle;
        }

        // Preserved for future use (e.g. loot spawning from enemies).
        // Root object moves from spawn position down to the ground surface, then bounces.
        private IEnumerator AnimatePopFromGround()
        {
            _currentState = ItemState.Animating;

            if (_iconSpriteRenderer != null) _iconSpriteRenderer.enabled = false;
            if (_shadowSpriteRenderer != null) _shadowSpriteRenderer.enabled = false;

            yield return null;

            Vector3 spawnPosition = transform.position;
            Vector3 groundPosition = spawnPosition;

            if (Physics.Raycast(spawnPosition, Vector3.down, out RaycastHit hit, _raycastDistance, _groundLayer))
                groundPosition = hit.point;

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
                float height = _floatHeight + Mathf.Sin(t * Mathf.PI) * _bounceHeight;
                if (_iconTransform != null)
                    _iconTransform.localPosition = new Vector3(0, height, 0);
                yield return null;
            }

            if (_iconTransform != null)
                _iconTransform.localPosition = new Vector3(0, _floatHeight, 0);
            _currentState = ItemState.Idle;
        }

        private void Update()
        {
            if (_currentState != ItemState.Attracted || _playerTransform == null) return;

            Vector3 directionToPlayer = (_playerTransform.position - transform.position).normalized;
            transform.position += directionToPlayer * _attractionSpeed * Time.deltaTime;

            if (Vector3.Distance(transform.position, _playerTransform.position) < _pickupDistance)
                TryPickup();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (_currentState == ItemState.Idle && other.CompareTag("Player"))
                _currentState = ItemState.Attracted;
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
