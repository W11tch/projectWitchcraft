// Located at: Assets/Scripts/Core/SaveData.cs
using System.Collections.Generic;
using UnityEngine;

namespace ProjectWitchcraft.Core
{
    [System.Serializable]
    public class SaveData
    {
        // We use two lists for serialization because JsonUtility cannot handle dictionaries.
        public List<string> itemNames;
        public List<int> itemAmounts;
        public List<ObjectData> placedObjects;
        public List<ChestSaveData> chestData; // **NEW**: For saving chest inventories

        public SaveData()
        {
            itemNames = new List<string>();
            itemAmounts = new List<int>();
            placedObjects = new List<ObjectData>();
            chestData = new List<ChestSaveData>();
        }
    }

    [System.Serializable]
    public class ObjectData
    {
        public string itemDataName;
        public Vector3 position;
        public Quaternion rotation;

        // A field to store the visual rotation index for sprite based placed objects
        public int visualRotationIndex;
    }

    // **NEW**: A serializable class to store the state of a single chest's inventory.
    [System.Serializable]
    public class ChestSaveData
    {
        public string uniqueID;
        public List<string> itemNames;
        public List<int> itemQuantities;

        public ChestSaveData()
        {
            itemNames = new List<string>();
            itemQuantities = new List<int>();
        }
    }
}