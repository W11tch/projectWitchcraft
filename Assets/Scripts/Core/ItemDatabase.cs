// Located at: Assets/Scripts/Core/ItemDatabase.cs
using System.Collections.Generic;
using UnityEngine;

namespace ProjectWitchcraft.Core
{
    /// <summary>
    /// A ScriptableObject that holds a list of all items in the game,
    /// providing a fast way to look them up by name.
    /// </summary>
    [CreateAssetMenu(fileName = "ItemDatabase", menuName = "Witchcraft/Core/Item Database")]
    public class ItemDatabase : ScriptableObject
    {
        public List<ItemData> AllItems;

        // A dictionary for fast lookups by name.
        private Dictionary<string, ItemData> _itemMap;

        /// <summary>
        /// Initializes the database by populating the dictionary.
        /// Should be called by the InventoryManager on startup.
        /// </summary>
        public void Initialize()
        {
            _itemMap = new Dictionary<string, ItemData>();
            foreach (var item in AllItems)
            {
                if (item != null && !_itemMap.ContainsKey(item.name))
                {
                    _itemMap.Add(item.name, item);
                }
            }
        }

        /// <summary>
        /// Retrieves an ItemData asset from the database by its string name.
        /// </summary>
        public ItemData GetItemByName(string itemName)
        {
            _itemMap.TryGetValue(itemName, out var item);
            return item;
        }
    }
}