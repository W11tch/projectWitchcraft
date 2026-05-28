// Located at: Assets/Scripts/World/WorldItemSpawner.cs
using UnityEngine;
using ProjectWitchcraft.Core;
using ProjectWitchcraft.Managers; // You may need to add this 'using' directive

namespace ProjectWitchcraft.World
{
    /// <summary>
    /// Listens for item drop events and spawns the corresponding WorldItem prefab from the ObjectPooler.
    /// </summary>
    public class WorldItemSpawner : MonoBehaviour
    {
        [Header("Dependencies")]
        // A reference to the ObjectPooler in the scene.
        [SerializeField] private ObjectPooler objectPooler;

        [Header("Hierarchy Organization")]
        // The parent object for spawned items to keep the hierarchy clean.
        [SerializeField] private Transform worldItemsParent;

        private void OnEnable()
        {
            EventManager.AddListener<ItemDroppedInWorldEvent>(OnItemDropped);
        }

        private void OnDisable()
        {
            EventManager.RemoveListener<ItemDroppedInWorldEvent>(OnItemDropped);
        }

        private void OnItemDropped(ItemDroppedInWorldEvent e)
        {
            if (objectPooler == null)
            {
                Debug.LogError("ObjectPooler is not set on the WorldItemSpawner!");
                return;
            }

            // --- THIS IS THE CRITICAL CHANGE ---
            // We now call the ObjectPooler to get an item instead of using Instantiate.
            // The tag "WorldItemPrefab" must match the tag in your ObjectPooler's inspector.
            GameObject droppedObject = objectPooler.SpawnFromPool("WorldItemPrefab", e.position, Quaternion.identity);

            if (droppedObject == null) return;

            // Set the parent for a clean hierarchy
            if (worldItemsParent != null)
            {
                droppedObject.transform.SetParent(worldItemsParent);
            }

            // Initialize the item's data
            WorldItem worldItem = droppedObject.GetComponent<WorldItem>();
            if (worldItem != null)
            {
                worldItem.Initialize(e.itemData, e.quantity);
            }
        }
    }
}