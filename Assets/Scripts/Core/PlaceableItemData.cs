// Located at: Assets/Scripts/Core/PlaceableItemData.cs
using System.Collections.Generic;
using UnityEngine;

namespace ProjectWitchcraft.Core
{
    /// <summary>
    /// Defines if an item is placed on the ground, on top of another object, or either.
    /// </summary>
    [System.Serializable]
    public enum PlacementLayer { Ground, Upper, Any } // Added "Any"

    /// <summary>
    /// A container for all rules related to placing an object in the world grid.
    /// </summary>
    [System.Serializable]
    public class PlacementRules
    {
        [Tooltip("Determines if this object is a ground object, an upper object, or can be either.")]
        public PlacementLayer Layer;

        [Tooltip("A special rule for Floors. If true, it allows another 'Upper' layer object to be placed in the same grid cell.")]
        public bool AllowsStackingOnTop = false;
    }

    /// <summary>
    /// Represents an item that can be placed in the game world.
    /// Inherits from ItemData and adds placement-specific information.
    /// The cost to place this item is always 1 of the item itself.
    /// </summary>
    [CreateAssetMenu(fileName = "New PlaceableItemData", menuName = "Witchcraft/Items/Placeable Item Data")]
    public class PlaceableItemData : ItemData
    {
        [Header("Placement")]
        [Tooltip("The prefab that will be spawned in the world when this item is placed.")]
        public GameObject placedPrefab;

        [Tooltip("The size of the object in grid cells (e.g., 1x1, 2x1).")]
        public Vector2Int size = Vector2Int.one;

        [Tooltip("The set of rules governing where and how this item can be placed.")]
        public PlacementRules placementRules;
    }
}