// Located at: Assets/Scripts/BuildingSystem/ScriptableObjects/BuildingData.cs
using System.Collections.Generic;
using UnityEngine;
using ProjectWitchcraft.Core;

namespace ProjectWitchcraft.BuildingSystem
{
    [CreateAssetMenu(fileName = "New BuildingData", menuName = "Building System/Building Data")]
    public class BuildingData : ScriptableObject
    {
        public enum ObjectCategory { Block, Wall, Bridge, Furniture }

        [Header("General Information")]
        public string buildingName;
        public GameObject prefab;
        public string poolTag;
        public Vector3Int size = Vector3Int.one;

        [Header("Placement & Interaction Rules")]
        public ObjectCategory Category;

        [Tooltip("For Furniture: If true, this object must be placed against a Wall or an upper-level Block.")]
        public bool RequiresWallBehind = false;

        [Tooltip("Can other objects be placed on top of this one? (e.g., for Blocks, Bridges, or tables).")]
        public bool canPlaceOnTop = false;

    }
}