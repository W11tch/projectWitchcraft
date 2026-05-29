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
            SpawnWorldItem(e.itemInstance, e.quantity, e.position, playerDrop: true);
        }

        private void OnGatherSaveData(GatherSaveDataEvent e)
        {
            e.SaveData.worldItems.Clear();
            if (worldItemsParent == null) return;

            foreach (Transform child in worldItemsParent)
            {
                if (!child.gameObject.activeSelf) continue;
                var worldItem = child.GetComponent<WorldItem>();
                if (worldItem?.ItemInstance == null || worldItem.ItemInstance.IsEmpty) continue;

                var data = new WorldItemSaveData
                {
                    itemGuid = worldItem.ItemInstance.Definition.AssetGuid,
                    quantity = worldItem.Quantity,
                    durability = worldItem.ItemInstance.CurrentDurability
                };
                data.Position = child.position;
                e.SaveData.worldItems.Add(data);
            }
        }

        private void OnApplySaveData(ApplySaveDataEvent e)
        {
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
                var itemDef = _assetRegistry.GetItemByGuid(data.itemGuid);
                if (itemDef == null)
                {
                    Debug.LogWarning($"[WorldItemSpawner] Could not find item with GUID '{data.itemGuid}' — skipping.");
                    continue;
                }
                var instance = new ItemInstance(itemDef) { CurrentDurability = data.durability };
                SpawnWorldItem(instance, data.quantity, data.Position, playerDrop: false, fromSave: true);
            }
        }

        private void SpawnWorldItem(ItemInstance itemInstance, int quantity, Vector3 position, bool playerDrop, bool fromSave = false)
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
                worldItem.InitializeFromSave(itemInstance, quantity);
            else
                worldItem.Initialize(itemInstance, quantity, playerDrop);
        }
    }
}
