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
            _rotatableSprite = GetComponentInChildren<RotatableSprite>();
            if (itemData != null)
                Size = new Vector3Int(itemData.size.x, 1, itemData.size.y);
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