// Located at: Assets/Scripts/BuildingSystem/PlaceableObject.cs
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

        private void Awake()
        {
            // Get the component from this object or any of its children.
            _rotatableSprite = GetComponentInChildren<RotatableSprite>();

            Collider objectCollider = GetComponent<Collider>();
            if (objectCollider != null)
            {
                Vector3 size = objectCollider.bounds.size;
                Size = new Vector3Int(Mathf.RoundToInt(size.x), Mathf.RoundToInt(size.y), Mathf.Max(1, Mathf.RoundToInt(size.z)));
            }
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