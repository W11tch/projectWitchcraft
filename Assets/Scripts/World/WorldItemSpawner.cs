// Located at: Assets/Scripts/World/WorldItemSpawner.cs
using System.Collections.Generic;
using UnityEngine;
using ProjectWitchcraft.Core;
using ProjectWitchcraft.Managers;

namespace ProjectWitchcraft.World
{
    public class WorldItemSpawner : MonoBehaviour
    {
        [Header("Dependencies")]
        [SerializeField] private ObjectPooler objectPooler;
        [SerializeField] private AssetRegistry _assetRegistry;

        [Header("Hierarchy Organization")]
        [SerializeField] private Transform worldItemsParent;

        private void OnEnable()
        {
            EventManager.AddListener<ItemDroppedInWorldEvent>(OnItemDropped);
            EventManager.AddListener<GatherSaveDataEvent>(OnGatherSaveData);
            EventManager.AddListener<ApplySaveDataEvent>(OnApplySaveData);
        }

        private void OnDisable()
        {
            EventManager.RemoveListener<ItemDroppedInWorldEvent>(OnItemDropped);
            EventManager.RemoveListener<GatherSaveDataEvent>(OnGatherSaveData);
            EventManager.RemoveListener<ApplySaveDataEvent>(OnApplySaveData);
        }

        private void OnItemDropped(ItemDroppedInWorldEvent e)
        {
            SpawnWorldItem(e.itemData, e.quantity, e.position, playerDrop: true);
        }

        private void OnGatherSaveData(GatherSaveDataEvent e)
        {
            e.SaveData.worldItems.Clear();
            if (worldItemsParent == null) return;

            foreach (Transform child in worldItemsParent)
            {
                if (!child.gameObject.activeSelf) continue;
                var worldItem = child.GetComponent<WorldItem>();
                if (worldItem?.ItemData == null) continue;

                var data = new WorldItemSaveData { quantity = worldItem.Quantity };
                data.Position = child.position;
                data.itemGuid = worldItem.ItemData.AssetGuid;
                e.SaveData.worldItems.Add(data);
            }
        }

        private void OnApplySaveData(ApplySaveDataEvent e)
        {
            // Return all live world items to the pool before restoring from save.
            if (worldItemsParent != null)
            {
                var toReturn = new List<GameObject>();
                foreach (Transform child in worldItemsParent)
                {
                    if (child.gameObject.activeSelf)
                        toReturn.Add(child.gameObject);
                }
                foreach (var go in toReturn)
                    objectPooler.ReturnToPool(go);
            }

            if (_assetRegistry == null)
            {
                Debug.LogError("[WorldItemSpawner] AssetRegistry not assigned — cannot restore world items.");
                return;
            }

            foreach (var data in e.SaveData.worldItems)
            {
                ItemData itemData = _assetRegistry.GetItemByGuid(data.itemGuid);
                if (itemData == null)
                {
                    Debug.LogWarning($"[WorldItemSpawner] Could not find item with GUID '{data.itemGuid}' — skipping.");
                    continue;
                }
                SpawnWorldItem(itemData, data.quantity, data.Position, playerDrop: false, fromSave: true);
            }
        }

        private void SpawnWorldItem(ItemData itemData, int quantity, Vector3 position, bool playerDrop, bool fromSave = false)
        {
            if (objectPooler == null)
            {
                Debug.LogError("[WorldItemSpawner] ObjectPooler is not assigned!");
                return;
            }

            GameObject droppedObject = objectPooler.SpawnFromPool("WorldItemPrefab", position, Quaternion.identity);
            if (droppedObject == null) return;

            if (worldItemsParent != null)
                droppedObject.transform.SetParent(worldItemsParent);

            WorldItem worldItem = droppedObject.GetComponent<WorldItem>();
            if (worldItem == null) return;

            if (fromSave)
                worldItem.InitializeFromSave(itemData, quantity);
            else
                worldItem.Initialize(itemData, quantity, playerDrop);
        }
    }
}
