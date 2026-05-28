// Located at: Assets/Scripts/BuildingSystem/PlaceableObject.cs
using System.Collections;
using UnityEngine;
using ProjectWitchcraft.Core;

namespace ProjectWitchcraft.BuildingSystem
{
    public class PlaceableObject : MonoBehaviour, IPoolableObject
    {
        [SerializeField] private PlaceableItemData itemData;
        public Vector3Int Size { get; private set; }
        public bool Placed { get; set; }
        public string UniqueID { get; set; }
        public PlaceableItemData ItemData => itemData; // Changed from BuildingData

        // --- IPoolableObject Implementation ---
        /// <summary>
        /// Provides a reliable "nametag" for the ObjectPooler by returning the name
        /// of the prefab asset, which is used as the pool tag.
        /// </summary>
        public string PoolTag => itemData.placedPrefab.name;

        // **NEW**: A reference to the optional RotatableSprite component.
        private RotatableSprite _rotatableSprite;

        /// <summary>
        /// A public property that acts as a "bridge" to the RotatableSprite's index.
        /// This allows other scripts (like the PlacementManager and WorldGridManager) to
        /// get/set the visual state without needing to know about the RotatableSprite component itself.
        /// </summary>
        public int VisualRotationIndex
        {
            get => _rotatableSprite != null ? _rotatableSprite.RotationIndex : 0;
            set
            {
                if (_rotatableSprite != null)
                {
                    _rotatableSprite.RotationIndex = value;
                }
            }
        }

        private Collider _collider;

        private void Awake()
        {
            _rotatableSprite = GetComponentInChildren<RotatableSprite>();
            _collider = GetComponent<Collider>();
            if (_collider != null)
            {
                Vector3 size = _collider.bounds.size;
                Size = new Vector3Int(Mathf.RoundToInt(size.x), Mathf.RoundToInt(size.y), Mathf.Max(1, Mathf.RoundToInt(size.z)));
            }
        }

        // Disables the collider and re-enables it only once the CharacterController is
        // no longer intersecting the placed object's footprint. This prevents the
        // CharacterController's depenetration from ejecting the player when an object
        // spawns on or adjacent to them.
        public void ActivateColliderSafely()
        {
            if (_collider != null)
                StartCoroutine(EnableWhenPlayerClears());
        }

        private IEnumerator EnableWhenPlayerClears()
        {
            // Cache bounds before disabling — disabled colliders don't update bounds reliably.
            Bounds placedBounds = _collider.bounds;
            _collider.enabled = false;

            var playerCC = GameReferences.Instance.PlayerTransform?.GetComponent<CharacterController>();
            if (playerCC != null)
            {
                // Poll every physics frame until the CharacterController AABB no longer intersects.
                // No cap: placement is already blocked when overlapping the player, so this coroutine
                // only runs for adjacent placements that clear within a few frames.
                while (true)
                {
                    Vector3 ccCenter = playerCC.transform.TransformPoint(playerCC.center);
                    Bounds ccBounds = new Bounds(ccCenter,
                        new Vector3(playerCC.radius * 2f, playerCC.height, playerCC.radius * 2f));

                    if (!placedBounds.Intersects(ccBounds)) break;

                    yield return new WaitForFixedUpdate();
                }
            }

            _collider.enabled = true;
        }

        public void Rotate()
        {
            transform.Rotate(0, 90, 0);
            Size = new Vector3Int(Size.z, Size.y, Size.x);

            // **NEW**: If the object has a RotatableSprite, tell it to cycle to the next sprite.
            _rotatableSprite?.CycleSprite();
        }

        public void SetPosition(Vector3 position)
        {
            transform.position = position;
        }

        public Vector3 GetStartPosition()
        {
            return transform.position;
        }
    }
}   