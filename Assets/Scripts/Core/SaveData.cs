using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace ProjectWitchcraft.Core
{
    [System.Serializable]
    public class SaveData
    {
        public int version = 2;
        public PlayerSaveData player = new PlayerSaveData();
        public List<ObjectData> placedObjects = new List<ObjectData>();
        public List<WorldItemSaveData> worldItems = new List<WorldItemSaveData>();
    }

    [System.Serializable]
    public class PlayerSaveData
    {
        public List<SlotData> hotbar = new List<SlotData>();
        public List<SlotData> inventory = new List<SlotData>();
    }

    // One inventory slot — empty when itemGuid is null/empty.
    [System.Serializable]
    public class SlotData
    {
        public string itemGuid;
        public int quantity;
    }

    [System.Serializable]
    public class ObjectData
    {
        public string itemGuid;
        // Stored as separate floats so Newtonsoft.Json handles them without custom converters.
        public float px, py, pz;
        public float rx, ry, rz, rw;
        public int visualRotationIndex;
        // Populated only when this placed object is a chest. Null otherwise.
        public ChestSaveData chest;

        [JsonIgnore]
        public Vector3 Position
        {
            get => new Vector3(px, py, pz);
            set { px = value.x; py = value.y; pz = value.z; }
        }

        [JsonIgnore]
        public Quaternion Rotation
        {
            get => new Quaternion(rx, ry, rz, rw);
            set { rx = value.x; ry = value.y; rz = value.z; rw = value.w; }
        }
    }

    [System.Serializable]
    public class ChestSaveData
    {
        public string uniqueId;
        public List<SlotData> slots = new List<SlotData>();
    }

    [System.Serializable]
    public class WorldItemSaveData
    {
        public string itemGuid;
        public int quantity;
        public float px, py, pz;

        [JsonIgnore]
        public Vector3 Position
        {
            get => new Vector3(px, py, pz);
            set { px = value.x; py = value.y; pz = value.z; }
        }
    }
}
