using System.Collections.Generic;
using UnityEngine;

namespace ProjectWitchcraft.Core
{
    // Single source of truth for all game assets at runtime.
    // Assign assets in the inspector. The save system uses GUIDs — no Resources.Load required.
    [CreateAssetMenu(fileName = "AssetRegistry", menuName = "ProjectWitchcraft/Asset Registry")]
    public class AssetRegistry : ScriptableObject
    {
        [SerializeField] private List<ItemData> _items = new();
        [SerializeField] private List<PlaceableItemData> _placeables = new();
        [SerializeField] private List<EquipmentItemData> _equipment = new();
        [SerializeField] private List<WeaponItemData> _weapons = new();
        [SerializeField] private List<ToolItemData> _tools = new();
        [SerializeField] private List<EntityDefinition> _entities = new();

        private Dictionary<string, ItemData> _itemsByGuid;
        private Dictionary<string, PlaceableItemData> _placeablesByGuid;
        private Dictionary<string, EquipmentItemData> _equipmentByGuid;
        private Dictionary<string, WeaponItemData> _weaponsByGuid;
        private Dictionary<string, ToolItemData> _toolsByGuid;
        private Dictionary<string, EntityDefinition> _entitiesByGuid;

        public void Initialize()
        {
            _itemsByGuid = BuildDict<ItemData>(_items);
            _placeablesByGuid = BuildDict<PlaceableItemData>(_placeables);
            _equipmentByGuid = BuildDict<EquipmentItemData>(_equipment);
            _weaponsByGuid = BuildDict<WeaponItemData>(_weapons);
            _toolsByGuid = BuildDict<ToolItemData>(_tools);
            _entitiesByGuid = BuildDict<EntityDefinition>(_entities);
        }

        private Dictionary<string, T> BuildDict<T>(List<T> list) where T : GuidAsset
        {
            var dict = new Dictionary<string, T>(list.Count);
            foreach (var item in list)
            {
                if (item == null || string.IsNullOrEmpty(item.AssetGuid)) continue;
                if (dict.ContainsKey(item.AssetGuid))
                    Debug.LogError($"[AssetRegistry] Duplicate GUID on '{item.name}'. Check for duplicate entries.");
                else
                    dict[item.AssetGuid] = item;
            }
            return dict;
        }

        public ItemData GetItemByGuid(string guid)
        {
            EnsureInitialized();
            if (_itemsByGuid.TryGetValue(guid, out var item)) return item;
            if (_placeablesByGuid.TryGetValue(guid, out var placeable)) return placeable;
            if (_equipmentByGuid.TryGetValue(guid, out var equipment)) return equipment;
            if (_weaponsByGuid.TryGetValue(guid, out var weapon)) return weapon;
            if (_toolsByGuid.TryGetValue(guid, out var tool)) return tool;
            return null;
        }

        public PlaceableItemData GetPlaceableByGuid(string guid)
        {
            EnsureInitialized();
            return _placeablesByGuid.TryGetValue(guid, out var p) ? p : null;
        }

        public EquipmentItemData GetEquipmentByGuid(string guid)
        {
            EnsureInitialized();
            return _equipmentByGuid.TryGetValue(guid, out var e) ? e : null;
        }

        public WeaponItemData GetWeaponByGuid(string guid)
        {
            EnsureInitialized();
            return _weaponsByGuid.TryGetValue(guid, out var w) ? w : null;
        }

        public ToolItemData GetToolByGuid(string guid)
        {
            EnsureInitialized();
            return _toolsByGuid.TryGetValue(guid, out var t) ? t : null;
        }

        public EntityDefinition GetEntityByGuid(string guid)
        {
            EnsureInitialized();
            return _entitiesByGuid.TryGetValue(guid, out var e) ? e : null;
        }

        private void EnsureInitialized()
        {
            if (_itemsByGuid == null) Initialize();
        }

        public IReadOnlyList<ItemData> AllItems => _items;
        public IReadOnlyList<PlaceableItemData> AllPlaceables => _placeables;
        public IReadOnlyList<EquipmentItemData> AllEquipment => _equipment;
        public IReadOnlyList<WeaponItemData> AllWeapons => _weapons;
        public IReadOnlyList<ToolItemData> AllTools => _tools;
        public IReadOnlyList<EntityDefinition> AllEntities => _entities;

        public IEnumerable<ItemData> AllItemsOfAllTypes
        {
            get
            {
                foreach (var i in _items) yield return i;
                foreach (var i in _placeables) yield return i;
                foreach (var i in _equipment) yield return i;
                foreach (var i in _weapons) yield return i;
                foreach (var i in _tools) yield return i;
            }
        }
    }
}
