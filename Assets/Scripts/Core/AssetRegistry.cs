using System.Collections.Generic;
using UnityEngine;

namespace ProjectWitchcraft.Core
{
    // Single source of truth for all game assets at runtime.
    // Assign all ItemData and PlaceableItemData assets in the inspector.
    // The save system uses GUIDs from this registry — no Resources.Load required.
    [CreateAssetMenu(fileName = "AssetRegistry", menuName = "ProjectWitchcraft/Asset Registry")]
    public class AssetRegistry : ScriptableObject
    {
        [SerializeField] private List<ItemData> _items = new();
        [SerializeField] private List<PlaceableItemData> _placeables = new();

        private Dictionary<string, ItemData> _itemsByGuid;
        private Dictionary<string, PlaceableItemData> _placeablesByGuid;

        public void Initialize()
        {
            _itemsByGuid = new Dictionary<string, ItemData>(_items.Count);
            foreach (var item in _items)
            {
                if (item == null || string.IsNullOrEmpty(item.AssetGuid)) continue;
                if (_itemsByGuid.ContainsKey(item.AssetGuid))
                    Debug.LogError($"[AssetRegistry] Duplicate GUID on item '{item.name}'. Check for duplicate entries.");
                else
                    _itemsByGuid[item.AssetGuid] = item;
            }

            _placeablesByGuid = new Dictionary<string, PlaceableItemData>(_placeables.Count);
            foreach (var p in _placeables)
            {
                if (p == null || string.IsNullOrEmpty(p.AssetGuid)) continue;
                if (_placeablesByGuid.ContainsKey(p.AssetGuid))
                    Debug.LogError($"[AssetRegistry] Duplicate GUID on placeable '{p.name}'.");
                else
                    _placeablesByGuid[p.AssetGuid] = p;
            }
        }

        public ItemData GetItemByGuid(string guid)
        {
            if (_itemsByGuid == null) Initialize();
            if (_itemsByGuid.TryGetValue(guid, out var item)) return item;
            // PlaceableItemData extends ItemData — check the placeables dictionary too.
            if (_placeablesByGuid == null) Initialize();
            return _placeablesByGuid.TryGetValue(guid, out var placeable) ? placeable : null;
        }

        public PlaceableItemData GetPlaceableByGuid(string guid)
        {
            if (_placeablesByGuid == null) Initialize();
            return _placeablesByGuid.TryGetValue(guid, out var p) ? p : null;
        }

        public IReadOnlyList<ItemData> AllItems => _items;
        public IReadOnlyList<PlaceableItemData> AllPlaceables => _placeables;
    }
}
